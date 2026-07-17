using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using HappyFarmer.AiAdvisoryService.Api.Dtos;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record ChatReplyResult(bool Success, string? Reply, string? FallbackMessage, List<ChatCard>? Cards = null);

/// <summary>
/// Gọi Gemini qua Google.GenAI SDK chính thức (client-manage lịch sử hội thoại, giống pattern cũ
/// dùng cho Claude — API generateContent, không dùng Interactions API vì SDK C# chưa hỗ trợ).
///
/// Chatbot có function-calling tới Market Price Service/Marketplace Service/Auth Service để trả lời
/// bằng dữ liệu thật của hệ thống thay vì chỉ dựa vào kiến thức nền của Gemini — xem
/// docs/architecture/data-flows/ai-chatbot-flow.md. Card hiển thị (giá/tin đăng) do CHÍNH BACKEND build
/// từ dữ liệu thô các service trả về, không phụ thuộc Gemini tự format JSON — tránh rủi ro model bịa
/// số liệu khi "tường thuật" lại kết quả tool-call.
/// </summary>
public class GeminiChatService(
    Client client,
    IConfiguration configuration,
    ILogger<GeminiChatService> logger,
    MarketPriceServiceClient marketPriceClient,
    MarketplaceServiceClient marketplaceClient,
    AuthServiceClient authServiceClient,
    GeminiEmbeddingService embeddingService,
    QdrantKnowledgeService knowledgeService)
{
    private const int MaxToolIterations = 4;
    private const int MaxCardsPerCall = 5;
    private const int MaxSearchResults = 8;

    // Persona (giọng điệu) + ranh giới chủ đề + hướng dẫn hỏi lại khi thiếu thông tin.
    private const string SystemPrompt = """
        Bạn là trợ lý tư vấn canh tác nông nghiệp riêng của HappyFarmer, nói chuyện thân thiện, kiên nhẫn,
        gần gũi bằng tiếng Việt như đang trò chuyện với nông dân. Tránh thuật ngữ kỹ thuật/công nghệ.

        CHỈ trả lời các câu hỏi liên quan đến nông nghiệp, canh tác, cây trồng, sâu bệnh, thời tiết
        ảnh hưởng mùa vụ, giá nông sản, mua bán nông sản. Nếu người dùng hỏi chủ đề khác (chính trị, sức
        khỏe con người, lập trình, giải trí...), hãy lịch sự từ chối và gợi ý quay lại các chủ đề canh tác
        bạn có thể hỗ trợ, không cố trả lời.

        Bạn có các công cụ tra cứu dữ liệu THẬT của hệ thống HappyFarmer:
        - search_products / search_regions: tìm ID chính xác của 1 loại nông sản/khu vực theo tên.
        - get_current_prices / get_price_history / get_price_trend: giá nông sản thật, cập nhật theo ngày
          (nhận productId/regionId dạng số, KHÔNG nhận tên).
        - search_marketplace_listings: tin đăng bán nông sản thật đang có trên Chợ nông sản (nhận
          productId/regionId dạng số, KHÔNG nhận tên).
        - get_my_profile: tên và tỉnh/thành đã đăng ký của người dùng đang trò chuyện.
        - search_knowledge_base: tra cứu tài liệu kỹ thuật nông nghiệp (kỹ thuật canh tác, phục hồi
          vườn sau thiên tai, tiếp cận thị trường...) khi câu hỏi cần kiến thức chuyên sâu hơn kiến
          thức nền của bạn. Mỗi kết quả trả về kèm `sourceDocument` (tên tài liệu) và `sourceUrl`
          (link trang chi tiết CHÍNH XÁC của tài liệu đó, có thể null). Khi dùng thông tin từ tool
          này: LUÔN trích dẫn tên tài liệu nguồn dạng link markdown trỏ đúng `sourceUrl` của tài liệu
          đó (vd. "Theo tài liệu [tên tài liệu](sourceUrl)...") để người dùng bấm vào xem/tải bản gốc
          — nếu `sourceUrl` là null thì chỉ nêu tên tài liệu bằng chữ thường, KHÔNG tự bịa link. Nếu
          dùng nhiều tài liệu khác nguồn trong 1 câu trả lời, mỗi tài liệu link đúng URL riêng của nó,
          không dùng lẫn URL của tài liệu khác. Nói rõ đây là tài liệu tham khảo kỹ thuật — với tình
          huống nghiêm trọng vẫn khuyên người dùng hỏi thêm cán bộ khuyến nông/chuyên gia thật.

        QUAN TRỌNG: get_current_prices, get_price_history, search_marketplace_listings chỉ nhận ID số,
        không nhận tên. Khi người dùng nhắc tên nông sản/khu vực bằng chữ, BẠN PHẢI gọi search_products/
        search_regions trước để lấy đúng ID rồi mới gọi tool tương ứng — không tự đoán ID. Nếu
        search_products/search_regions trả về nhiều kết quả khớp, chọn kết quả rõ ràng khớp nhất, hoặc
        hỏi lại người dùng để làm rõ nếu thực sự mơ hồ. Nếu không nêu rõ khu vực, có thể gọi get_my_profile
        rồi dùng tỉnh/thành của người dùng (qua search_regions) làm khu vực mặc định thay vì hỏi lại ngay —
        chỉ hỏi lại khi get_my_profile không có thông tin tỉnh/thành.
        Khi công cụ báo lỗi hoặc không có dữ liệu, hãy nói thật với người dùng thay vì bịa số liệu.

        Nếu tin nhắn của người dùng chưa đủ thông tin để tư vấn chính xác (chưa rõ loại cây trồng,
        triệu chứng cụ thể, khu vực/mùa vụ), hãy hỏi lại một câu hỏi làm rõ thay vì đoán và đưa ra
        lời khuyên chung chung.
        """;

    public async Task<ChatReplyResult> GetReplyAsync(List<ChatTurn> history, string userMessage, int userId, CancellationToken ct)
    {
        var model = configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite";

        var contents = history
            .Select(t => new Content
            {
                Role = t.Role == "user" ? "user" : "model",
                Parts = [new Part { Text = t.Content }],
            })
            .ToList();
        contents.Add(new Content { Role = "user", Parts = [new Part { Text = userMessage }] });

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] },
            MaxOutputTokens = 1024,
            // Nhiệt độ thấp — câu trả lời tư vấn nên nhất quán/factual hơn là "sáng tạo", nhất là khi
            // đã có dữ liệu thật từ function-calling (không cần model tự do diễn giải số liệu).
            Temperature = 0.2,
            // Tắt thinking — chatbot tư vấn ngắn không cần bước suy luận sâu, và thinking mặc định
            // (AUTOMATIC) là nguyên nhân gây độ trễ lớn (nhiều giây) cho một câu trả lời hội thoại đơn giản.
            ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 },
            Tools = BuildTools(),
            ToolConfig = new ToolConfig
            {
                FunctionCallingConfig = new FunctionCallingConfig { Mode = FunctionCallingConfigMode.Auto },
            },
        };

        var cards = new List<ChatCard>();

        try
        {
            for (var iteration = 0; ; iteration++)
            {
                var response = await client.Models.GenerateContentAsync(model, contents, config, ct);

                var blockReason = response.PromptFeedback?.BlockReason;
                var candidate = response.Candidates?.FirstOrDefault();
                if (candidate is null || blockReason is not null)
                {
                    logger.LogWarning("Gemini từ chối trả lời. BlockReason: {BlockReason}", blockReason);
                    return new ChatReplyResult(false, null, "Xin lỗi, tôi không thể trả lời câu hỏi này. Bạn hỏi cách khác được không?");
                }

                var functionCalls = response.FunctionCalls;
                if (functionCalls is not { Count: > 0 })
                {
                    var text = response.Text ?? candidate.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
                    return new ChatReplyResult(true, text, null, cards.Count > 0 ? cards : null);
                }

                if (iteration >= MaxToolIterations)
                {
                    logger.LogWarning("Chatbot vượt giới hạn {Max} vòng tool-call, dừng lại", MaxToolIterations);
                    return new ChatReplyResult(false, null, "Câu hỏi này cần tra cứu quá nhiều bước, bạn thử hỏi cụ thể hơn nhé.");
                }

                // Echo lại turn "model" (chứa FunctionCall) rồi turn "user" (chứa FunctionResponse) —
                // bắt buộc theo đúng thứ tự API mong đợi, chỉ tồn tại trong nội bộ vòng lặp này, không
                // được lưu vào lịch sử hội thoại lâu dài (ChatMessage chỉ lưu text cuối cùng).
                contents.Add(candidate.Content!);

                var responseParts = new List<Part>();
                foreach (var call in functionCalls)
                {
                    var result = await ExecuteFunctionCallAsync(call, userId, cards, ct);
                    responseParts.Add(Part.FromFunctionResponse(call.Name!, result, null));
                }
                contents.Add(new Content { Role = "user", Parts = responseParts });
            }
        }
        catch (ServerError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi server");
            return new ChatReplyResult(false, null, "Dịch vụ tư vấn tạm thời gián đoạn, vui lòng thử lại sau.");
        }
        catch (ClientError ex)
        {
            logger.LogWarning(ex, "Gemini API lỗi client (rate limit/key/model)");
            return new ChatReplyResult(false, null, "Hệ thống đang bận hoặc có lỗi cấu hình, vui lòng thử lại sau ít phút.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi gọi Gemini API");
            return new ChatReplyResult(false, null, "Đã có lỗi xảy ra, vui lòng thử lại sau.");
        }
    }

    private async Task<Dictionary<string, object>> ExecuteFunctionCallAsync(
        FunctionCall call, int userId, List<ChatCard> cards, CancellationToken ct)
    {
        var args = call.Args ?? [];
        try
        {
            return call.Name switch
            {
                "search_products" => await HandleSearchProductsAsync(args, ct),
                "search_regions" => await HandleSearchRegionsAsync(args, ct),
                "get_current_prices" => await HandleGetCurrentPricesAsync(args, cards, ct),
                "get_price_trend" => await HandleGetPriceTrendAsync(cards, ct),
                "get_price_history" => await HandleGetPriceHistoryAsync(args, cards, ct),
                "search_marketplace_listings" => await HandleSearchListingsAsync(args, cards, ct),
                "get_my_profile" => await HandleGetMyProfileAsync(userId, ct),
                "search_knowledge_base" => await HandleSearchKnowledgeBaseAsync(args, ct),
                _ => new Dictionary<string, object> { ["error"] = $"Không rõ hàm {call.Name}." },
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Lỗi khi thực thi function-call {Name}", call.Name);
            return new Dictionary<string, object> { ["error"] = "Có lỗi khi tra cứu dữ liệu, vui lòng thử lại." };
        }
    }

    private async Task<Dictionary<string, object>> HandleGetCurrentPricesAsync(
        Dictionary<string, object> args, List<ChatCard> cards, CancellationToken ct)
    {
        var productId = GetIntArg(args, "productId");
        if (productId is null)
        {
            return new Dictionary<string, object> { ["error"] = "Thiếu productId — hãy gọi search_products trước để lấy đúng ID." };
        }
        var regionId = GetIntArg(args, "regionId");

        var prices = await marketPriceClient.GetCurrentPricesAsync(productId, regionId, ct);
        if (prices.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = "Chưa có dữ liệu giá cho nông sản/khu vực này." };
        }

        foreach (var p in prices.Take(MaxCardsPerCall))
        {
            cards.Add(new PriceCard(p.ProductId, p.ProductName, p.RegionName, p.Price, null, p.Unit, BuildPriceUrl(p.ProductId)));
        }

        return new Dictionary<string, object>
        {
            ["output"] = prices.Select(p => new { p.ProductName, p.RegionName, p.Price, p.Unit, p.EffectiveDate }).ToList(),
        };
    }

    private async Task<Dictionary<string, object>> HandleGetPriceTrendAsync(List<ChatCard> cards, CancellationToken ct)
    {
        var trending = await marketPriceClient.GetTrendingAsync(ct);
        if (trending.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = "Chưa có dữ liệu biến động giá." };
        }

        foreach (var t in trending.Take(MaxCardsPerCall))
        {
            cards.Add(new PriceCard(t.ProductId, t.ProductName, t.RegionName, t.CurrentPrice, t.ChangePercent, t.Unit, BuildPriceUrl(t.ProductId)));
        }

        return new Dictionary<string, object>
        {
            ["output"] = trending.Select(t => new { t.ProductName, t.RegionName, t.CurrentPrice, t.ChangePercent, t.Unit }).ToList(),
        };
    }

    private async Task<Dictionary<string, object>> HandleGetPriceHistoryAsync(
        Dictionary<string, object> args, List<ChatCard> cards, CancellationToken ct)
    {
        var productId = GetIntArg(args, "productId");
        if (productId is null)
        {
            return new Dictionary<string, object> { ["error"] = "Thiếu productId — hãy gọi search_products trước để lấy đúng ID." };
        }
        var regionId = GetIntArg(args, "regionId");

        var history = await marketPriceClient.GetPriceHistoryAsync(productId.Value, regionId, ct);
        if (history.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = "Chưa có đủ dữ liệu lịch sử giá cho nông sản này." };
        }

        var product = (await marketPriceClient.GetProductsByIdsAsync([productId.Value], ct)).FirstOrDefault();
        var region = regionId is null ? null : (await marketPriceClient.GetRegionsByIdsAsync([regionId.Value], ct)).FirstOrDefault();

        var latest = history[^1];
        cards.Add(new PriceCard(
            productId.Value, product?.NameVi ?? "Không rõ", region?.ProvinceName ?? "Toàn quốc",
            latest.Price, null, latest.Unit, BuildPriceUrl(productId.Value)));

        return new Dictionary<string, object>
        {
            ["output"] = history.Select(h => new { h.EffectiveDate, h.Price, h.Unit }).ToList(),
        };
    }

    private async Task<Dictionary<string, object>> HandleSearchListingsAsync(
        Dictionary<string, object> args, List<ChatCard> cards, CancellationToken ct)
    {
        var productId = GetIntArg(args, "productId");
        var regionId = GetIntArg(args, "regionId");
        var maxPrice = GetDecimalArg(args, "maxPrice");

        var listings = await marketplaceClient.SearchListingsAsync(productId, regionId, maxPrice, MaxCardsPerCall, ct);
        if (listings.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = "Hiện không có tin đăng nào phù hợp." };
        }

        // Resolve tên sản phẩm/khu vực để hiển thị card — ListingResponse gốc chỉ có id. Chỉ tra
        // đúng vài id thật sự xuất hiện trong kết quả (batch by-ids), không tải toàn bộ catalog.
        var products = await marketPriceClient.GetProductsByIdsAsync(listings.Select(l => l.ProductId), ct);
        var regions = await marketPriceClient.GetRegionsByIdsAsync(listings.Select(l => l.RegionId), ct);
        var productsById = products.ToDictionary(p => p.Id);
        var regionsById = regions.ToDictionary(r => r.Id);

        foreach (var l in listings)
        {
            var productDisplay = productsById.GetValueOrDefault(l.ProductId)?.NameVi ?? "Không rõ";
            var regionDisplay = regionsById.GetValueOrDefault(l.RegionId)?.ProvinceName ?? "Không rõ";
            cards.Add(new ListingCard(
                l.Id, productDisplay, regionDisplay, l.PricePerUnit, l.Quantity, l.Unit,
                l.ImageUrls.FirstOrDefault(), l.FarmerName, BuildListingUrl(l.Id)));
        }

        return new Dictionary<string, object>
        {
            ["output"] = listings.Select(l => new
            {
                productName = productsById.GetValueOrDefault(l.ProductId)?.NameVi,
                regionName = regionsById.GetValueOrDefault(l.RegionId)?.ProvinceName,
                l.Quantity,
                l.Unit,
                l.PricePerUnit,
                l.FarmerName,
            }).ToList(),
        };
    }

    private async Task<Dictionary<string, object>> HandleGetMyProfileAsync(int userId, CancellationToken ct)
    {
        var user = await authServiceClient.GetUserAsync(userId, ct);
        return user is null
            ? new Dictionary<string, object> { ["error"] = "Không lấy được thông tin người dùng hiện tại." }
            : new Dictionary<string, object> { ["output"] = new { user.FullName, user.ProvinceName } };
    }

    /// <summary>
    /// RAG — tìm đoạn tài liệu gần nghĩa nhất với câu hỏi (Qdrant, embedding bất đối xứng
    /// RETRIEVAL_QUERY). Không tạo ChatCard vì đây là văn bản tham khảo, không phải dữ liệu có cấu
    /// trúc để hiển thị card như giá/tin đăng.
    /// </summary>
    private async Task<Dictionary<string, object>> HandleSearchKnowledgeBaseAsync(Dictionary<string, object> args, CancellationToken ct)
    {
        var query = GetStringArg(args, "query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new Dictionary<string, object> { ["error"] = "Thiếu câu hỏi để tra cứu tài liệu." };
        }

        var queryEmbedding = await embeddingService.EmbedQueryAsync(query, ct);
        var results = await knowledgeService.SearchAsync(queryEmbedding, topK: 5, ct);
        if (results.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = "Không tìm thấy tài liệu liên quan." };
        }

        // sourceUrl trả riêng theo từng tài liệu (không phải 1 link chung) — không tự host/phân phối
        // lại file gốc (rủi ro bản quyền), chỉ dẫn đúng trang chi tiết của CHÍNH tài liệu vừa trích để
        // người dùng tự xem/tải. Có thể null nếu tài liệu không xác định được URL nguồn — khi đó
        // Gemini không được tự bịa link (xem hướng dẫn trong system prompt).
        return new Dictionary<string, object>
        {
            ["output"] = results.Select(r => new { sourceDocument = r.SourceDocument, text = r.Text, sourceUrl = r.SourceUrl }).ToList(),
        };
    }

    /// <summary>
    /// Tool "tìm kiếm" — trả về ứng viên kèm ID để Gemini TỰ CHỌN/hỏi lại khi mơ hồ, thay vì backend
    /// âm thầm lấy kết quả đầu tiên (đây là lý do các tool "hành động" bên trên chỉ nhận ID, không
    /// còn nhận tên tự do).
    /// </summary>
    private async Task<Dictionary<string, object>> HandleSearchProductsAsync(Dictionary<string, object> args, CancellationToken ct)
    {
        var query = GetStringArg(args, "query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new Dictionary<string, object> { ["error"] = "Thiếu từ khoá tìm kiếm." };
        }

        var products = await marketPriceClient.SearchProductsAsync(query, ct);
        if (products.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = $"Không tìm thấy nông sản nào khớp '{query}'." };
        }

        return new Dictionary<string, object>
        {
            ["output"] = products.Take(MaxSearchResults).Select(p => new { productId = p.Id, p.NameVi, p.Unit }).ToList(),
        };
    }

    private async Task<Dictionary<string, object>> HandleSearchRegionsAsync(Dictionary<string, object> args, CancellationToken ct)
    {
        var query = GetStringArg(args, "query");
        if (string.IsNullOrWhiteSpace(query))
        {
            return new Dictionary<string, object> { ["error"] = "Thiếu từ khoá tìm kiếm." };
        }

        var regions = await marketPriceClient.GetRegionsAsync(ct);
        var matches = regions
            .Where(r => r.ProvinceName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        r.MarketName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(MaxSearchResults)
            .ToList();

        if (matches.Count == 0)
        {
            return new Dictionary<string, object> { ["output"] = $"Không tìm thấy khu vực nào khớp '{query}'." };
        }

        return new Dictionary<string, object>
        {
            ["output"] = matches.Select(r => new { regionId = r.Id, r.ProvinceName, r.MarketName }).ToList(),
        };
    }

    private string BuildPriceUrl(int productId) => $"{configuration["Frontend:BaseUrl"]}/prices/{productId}";

    private string BuildListingUrl(int listingId) => $"{configuration["Frontend:BaseUrl"]}/marketplace/listings/{listingId}";

    private static string? GetStringArg(Dictionary<string, object> args, string key)
    {
        if (!args.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            string s => string.IsNullOrWhiteSpace(s) ? null : s,
            JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
            _ => value.ToString(),
        };
    }

    private static decimal? GetDecimalArg(Dictionary<string, object> args, string key)
    {
        if (!args.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            decimal d => d,
            double db => (decimal)db,
            JsonElement { ValueKind: JsonValueKind.Number } je => je.GetDecimal(),
            _ => null,
        };
    }

    private static int? GetIntArg(Dictionary<string, object> args, string key)
    {
        if (!args.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            int i => i,
            long l => (int)l,
            double db => (int)db,
            JsonElement { ValueKind: JsonValueKind.Number } je => je.GetInt32(),
            _ => null,
        };
    }

    private static List<Tool> BuildTools() =>
    [
        new Tool
        {
            FunctionDeclarations =
            [
                new FunctionDeclaration
                {
                    Name = "search_products",
                    Description = "Tìm ID chính xác của 1 loại nông sản theo tên. BẮT BUỘC gọi tool này trước khi gọi " +
                                  "get_current_prices/get_price_history/search_marketplace_listings nếu người dùng nhắc " +
                                  "tên nông sản bằng chữ (các tool đó chỉ nhận productId dạng số, không nhận tên).",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["query"] = new() { Type = Google.GenAI.Types.Type.String, Description = "Tên nông sản, ví dụ: cà chua, lúa" },
                        },
                        Required = ["query"],
                    },
                },
                new FunctionDeclaration
                {
                    Name = "search_regions",
                    Description = "Tìm ID chính xác của 1 khu vực/tỉnh theo tên. BẮT BUỘC gọi tool này trước khi gọi " +
                                  "get_current_prices/get_price_history/search_marketplace_listings nếu người dùng nhắc " +
                                  "khu vực bằng chữ (các tool đó chỉ nhận regionId dạng số, không nhận tên).",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["query"] = new() { Type = Google.GenAI.Types.Type.String, Description = "Tên tỉnh/khu vực, ví dụ: Lâm Đồng" },
                        },
                        Required = ["query"],
                    },
                },
                new FunctionDeclaration
                {
                    Name = "get_current_prices",
                    Description = "Tra giá nông sản hiện tại theo productId (lấy từ search_products), tuỳ chọn theo regionId. " +
                                  "Dùng khi người dùng hỏi giá bao nhiêu tiền một loại nông sản cụ thể.",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["productId"] = new() { Type = Google.GenAI.Types.Type.Integer, Description = "ID sản phẩm, lấy từ search_products" },
                            ["regionId"] = new() { Type = Google.GenAI.Types.Type.Integer, Description = "ID khu vực, lấy từ search_regions — để trống nếu không hỏi khu vực cụ thể" },
                        },
                        Required = ["productId"],
                    },
                },
                new FunctionDeclaration
                {
                    Name = "get_price_trend",
                    Description = "Lấy danh sách nông sản có biến động giá mạnh nhất gần đây (tăng/giảm). " +
                                  "Dùng khi người dùng hỏi về biến động giá chung, không hỏi 1 nông sản cụ thể.",
                    Parameters = new Schema { Type = Google.GenAI.Types.Type.Object, Properties = new Dictionary<string, Schema>() },
                },
                new FunctionDeclaration
                {
                    Name = "get_price_history",
                    Description = "Lấy lịch sử giá khoảng 3 tháng gần nhất của 1 loại nông sản (theo productId từ " +
                                  "search_products) để phân tích xu hướng tăng/giảm theo thời gian.",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["productId"] = new() { Type = Google.GenAI.Types.Type.Integer },
                            ["regionId"] = new() { Type = Google.GenAI.Types.Type.Integer },
                        },
                        Required = ["productId"],
                    },
                },
                new FunctionDeclaration
                {
                    Name = "search_marketplace_listings",
                    Description = "Tìm tin đăng bán nông sản thật đang có trên Chợ nông sản HappyFarmer, theo productId/" +
                                  "regionId (từ search_products/search_regions). Dùng khi người dùng muốn mua hoặc hỏi " +
                                  "có ai đang bán loại nông sản nào đó không.",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["productId"] = new() { Type = Google.GenAI.Types.Type.Integer, Description = "Để trống nếu tìm mọi loại nông sản" },
                            ["regionId"] = new() { Type = Google.GenAI.Types.Type.Integer },
                            ["maxPrice"] = new() { Type = Google.GenAI.Types.Type.Number, Description = "Giá tối đa mỗi đơn vị, nếu người dùng có nêu ngân sách" },
                        },
                    },
                },
                new FunctionDeclaration
                {
                    Name = "get_my_profile",
                    Description = "Lấy tên và tỉnh/thành đã đăng ký của người dùng đang trò chuyện để xưng hô cá nhân hóa.",
                    Parameters = new Schema { Type = Google.GenAI.Types.Type.Object, Properties = new Dictionary<string, Schema>() },
                },
                new FunctionDeclaration
                {
                    Name = "search_knowledge_base",
                    Description = "Tra cứu tài liệu kỹ thuật nông nghiệp (kỹ thuật canh tác, phục hồi vườn sau thiên tai, " +
                                  "tiếp cận thị trường...) theo ngữ nghĩa câu hỏi. Dùng khi câu hỏi cần kiến thức chuyên " +
                                  "sâu/cụ thể hơn kiến thức nền, đặc biệt các tình huống thiên tai/khắc phục sự cố cây trồng.",
                    Parameters = new Schema
                    {
                        Type = Google.GenAI.Types.Type.Object,
                        Properties = new Dictionary<string, Schema>
                        {
                            ["query"] = new() { Type = Google.GenAI.Types.Type.String, Description = "Câu hỏi hoặc chủ đề cần tra cứu" },
                        },
                        Required = ["query"],
                    },
                },
            ],
        },
    ];
}
