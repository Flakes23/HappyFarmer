using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UglyToad.PdfPig;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Configuration;

const int MinExtractedTextLength = 200;
const int ChunkSize = 1000;
const int ChunkOverlap = 150;
// Gemini free tier giới hạn 100 request embedding/phút — dãn cách 700ms/chunk để không vượt quota
// (60_000ms / 700ms ≈ 85 request/phút, có dư so với 100), thay vì bắn tuần tự hết tốc độ.
const int DelayBetweenChunksMs = 700;
const int MaxRetriesPerChunk = 3;

// URL trang chi tiết thật trên khuyennongvn.gov.vn cho từng tài liệu (theo đúng tên file không dấu
// ở knowledge-base/raw-sources/) — dùng để chatbot dẫn người dùng về ĐÚNG trang của tài liệu đang
// trích dẫn, thay vì tự host lại file gốc (rủi ro bản quyền, xem knowledge-base/raw-sources/README.md).
// Tài liệu nào không có trong dictionary này (vd. "Cay an qua" — bị bỏ qua lúc ingest vì 0 ký tự
// trích được, chưa từng vào RAG) thì sourceUrl gửi lên là null, không tự bịa link.
var sourceUrlByDocument = new Dictionary<string, string>
{
    ["Tiep can thi truong va phuong phap tieu thu nong san"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/tiep-can-thi-truong-va-phuong-phap-tieu-thu-nong-san-31967.html",
    ["Huong dan ky thuat cho nong dan khao sat thi truong"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-ky-thuat-cho-nong-dan-khao-sat-thi-truong-31990.html",
    ["Huong dan ky thuat cho nong dan tiep thi"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-ky-thuat-cho-nong-dan-tiep-thi-31988.html",
    ["Huong dan ky thuat cho nong dan GAP - An toan thuc pham"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-ky-thuat-cho-nong-dan-gap-an-toan-thuc-pham-31987.html",
    ["Canh tac lua va ca phe giam nhe tac dong bien doi khi hau"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/canh-tac-lua-va-ca-phe-giam-nhe-tac-dong-den-bien-doi-khi-hau-31971.html",
    ["Giai phap ky thuat thich ung thien tai - Trong trot"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/giai-phap-ky-thuat-nham-thich-ung-va-giam-thieu-thiet-hai-do-cac-loai-hinh-thien-tai-trong-linh-vuc-trong-trot-31530.html",
    ["Huong dan phuc hoi vuon ho tieu sau bao lu"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-phuc-hoi-va-cham-soc-vuon-ho-tieu-sau-bao-lu-31720.html",
    ["Huong dan phuc hoi vuon ca phe sau bao lu"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-phuc-hoi-va-cham-soc-vuon-ca-phe-sau-bao-lu-31722.html",
    ["Huong dan phuc hoi vuon sau rieng sau bao lu"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-phuc-hoi-va-cham-soc-vuon-sau-rieng-sau-bao-lu-31721.html",
    ["Bien phap khac phuc ung ngap lua sau mua bao"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/bien-phap-khac-phuc-ung-ngap-sau-mua-bao-doi-voi-lua-28153.html",
    ["Huong dan xu ly vung dat bi vui lap sau bao lu"] = "https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn/huong-dan-xu-ly-cac-vung-dat-bi-vui-lap-sau-bao-lu-28261.html",
};

var apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5224";
var apiKey = configuration["Internal:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "CHANGE_ME_VIA_USER_SECRETS")
{
    Console.WriteLine("Thiếu Internal:ApiKey. Chạy: dotnet user-secrets set Internal:ApiKey <giá trị đã dùng cho Internal:IngestApiKey của HappyFarmer.AiAdvisoryService.Api>");
    return 1;
}

var sourceFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuration["SourceFolder"] ?? "../../../../knowledge-base/raw-sources"));
if (!Directory.Exists(sourceFolder))
{
    Console.WriteLine($"Không tìm thấy thư mục nguồn: {sourceFolder}");
    return 1;
}

using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl), Timeout = TimeSpan.FromMinutes(2) };
http.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

Console.WriteLine("== HappyFarmer RAG Ingestor ==");
Console.WriteLine($"API: {apiBaseUrl}");
Console.WriteLine($"Thư mục nguồn: {sourceFolder}");
Console.WriteLine();

var pdfFiles = Directory.GetFiles(sourceFolder, "*.pdf").OrderBy(f => f).ToList();
if (pdfFiles.Count == 0)
{
    Console.WriteLine("Không tìm thấy file PDF nào.");
    return 1;
}

var totalChunksSent = 0;
var totalChunksFailed = 0;

foreach (var filePath in pdfFiles)
{
    var sourceDocument = Path.GetFileNameWithoutExtension(filePath);
    Console.WriteLine($"[{sourceDocument}]");

    string text;
    try
    {
        using var document = PdfDocument.Open(filePath);
        text = string.Join(" ", document.GetPages().Select(p => p.Text));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  LỖI khi đọc PDF: {ex.Message}. Bỏ qua file này.");
        continue;
    }

    var normalized = Regex.Replace(text, @"\s+", " ").Trim();
    if (normalized.Length < MinExtractedTextLength)
    {
        Console.WriteLine($"  Trích được quá ít text ({normalized.Length} ký tự) — nhiều khả năng là bản scan/ảnh, PdfPig không OCR được. Bỏ qua file này.");
        continue;
    }

    var chunks = ChunkText(normalized, ChunkSize, ChunkOverlap);
    Console.WriteLine($"  {normalized.Length} ký tự -> {chunks.Count} chunk.");

    var sourceUrl = sourceUrlByDocument.GetValueOrDefault(sourceDocument);
    if (sourceUrl is null)
    {
        Console.WriteLine("  Không có URL nguồn trong mapping — chatbot sẽ không chèn link cho tài liệu này.");
    }

    for (var i = 0; i < chunks.Count; i++)
    {
        var payload = new { SourceDocument = sourceDocument, ChunkIndex = i, Text = chunks[i], SourceUrl = sourceUrl };
        var sent = false;

        for (var attempt = 1; attempt <= MaxRetriesPerChunk && !sent; attempt++)
        {
            try
            {
                var response = await http.PostAsJsonAsync("/api/ai-advisory/internal/knowledge-ingest", payload);
                if (response.IsSuccessStatusCode)
                {
                    sent = true;
                    totalChunksSent++;
                }
                else if (attempt < MaxRetriesPerChunk)
                {
                    var backoffSeconds = 10 * attempt;
                    Console.WriteLine($"  Chunk #{i} lỗi ({(int)response.StatusCode}, lần {attempt}/{MaxRetriesPerChunk}) — có thể do vượt quota Gemini, chờ {backoffSeconds}s rồi thử lại...");
                    await Task.Delay(TimeSpan.FromSeconds(backoffSeconds));
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"  -> LỖI chunk #{i} sau {MaxRetriesPerChunk} lần thử ({(int)response.StatusCode}): {body}");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Không để 1 request mạng bị treo/timeout làm crash cả tiến trình ingest — log rồi
                // thử lại (hoặc bỏ qua chunk này nếu đã hết lượt thử) và tiếp tục các chunk còn lại.
                if (attempt < MaxRetriesPerChunk)
                {
                    Console.WriteLine($"  Chunk #{i} lỗi mạng (lần {attempt}/{MaxRetriesPerChunk}): {ex.Message}. Thử lại sau 10s...");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
                else
                {
                    Console.WriteLine($"  -> LỖI chunk #{i} sau {MaxRetriesPerChunk} lần thử (lỗi mạng): {ex.Message}");
                }
            }
        }

        if (!sent)
        {
            totalChunksFailed++;
        }

        await Task.Delay(DelayBetweenChunksMs);
    }
}

Console.WriteLine();
Console.WriteLine($"Hoàn tất: {totalChunksSent} chunk đã ingest thành công, {totalChunksFailed} chunk lỗi.");
return 0;

static List<string> ChunkText(string text, int chunkSize, int overlap)
{
    var chunks = new List<string>();
    var start = 0;

    while (start < text.Length)
    {
        var end = Math.Min(start + chunkSize, text.Length);

        // Ưu tiên cắt ở ranh giới câu (. ! ?) gần cuối chunk thay vì cắt cứng giữa từ/câu.
        if (end < text.Length)
        {
            var searchStart = Math.Max(start, end - 200);
            var lastBoundary = text.LastIndexOfAny(['.', '!', '?'], end - 1, end - searchStart);
            if (lastBoundary > start)
            {
                end = lastBoundary + 1;
            }
        }

        chunks.Add(text[start..end].Trim());
        if (end >= text.Length) break;
        start = Math.Max(end - overlap, start + 1);
    }

    return chunks;
}
