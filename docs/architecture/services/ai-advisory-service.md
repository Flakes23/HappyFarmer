# AI Advisory Service

## Trách nhiệm

Ba luồng AI cốt lõi:
1. Nhận diện bệnh cây trồng từ ảnh (Gemini Vision + Cloudinary)
2. Chatbot tư vấn canh tác bằng tiếng Việt (Gemini, có function-calling gọi sang Market Price
   Service/Marketplace Service/Auth Service để trả lời bằng dữ liệu thật — xem
   [ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md))
3. Dự đoán thời điểm thu hoạch tối ưu (OpenWeatherMap + Gemini)

Chi tiết từng luồng (input/output schema, xử lý lỗi) nằm ở các file riêng trong [../data-flows/](../data-flows/):
- [ai-disease-detection-flow.md](../data-flows/ai-disease-detection-flow.md)
- [ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md)
- [ai-harvest-prediction-flow.md](../data-flows/ai-harvest-prediction-flow.md)

Là service **duy nhất** trong hệ thống chủ động gọi service-to-service tới 3 service khác (Market
Price Service, Marketplace Service, Auth Service) qua network nội bộ (không qua Gateway) — dùng
`IHttpClientFactory` + `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler`, retry
+ circuit-breaker + timeout, cấu hình chặt hơn mặc định vì nằm trong 1 lượt chat của user: attempt
timeout 2s, tổng timeout 5s, tối đa 2 lần retry). Endpoint Market Price/Marketplace Service gọi tới
đều public (`[AllowAnonymous]`); riêng Auth Service dùng key riêng, xem
[auth-service.md#internal-api-key](auth-service.md#internal-api-key).

Chatbot còn có **RAG** (Retrieval-Augmented Generation) — trả lời dựa trên tài liệu kỹ thuật nông
nghiệp (phi cấu trúc, không query được bằng ID như dữ liệu ở trên) qua tool `search_knowledge_base`.
Vector embedding (Gemini Embedding API) lưu trong **Qdrant** (service riêng, xem `docker-compose.yml`
ở root repo) — không lưu song song trong SQL Server vì payload của Qdrant đã chứa cả nội dung chunk.
Tài liệu nạp vào qua tool console riêng `HappyFarmer.RagIngestor` (`src/Tools/`, chạy thủ công/offline),
không qua API Gateway. Chi tiết kiến trúc RAG xem
[ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md#rag--tra-cứu-tài-liệu-nông-nghiệp).

## API chính

| Method | Path | Mô tả |
|---|---|---|
| GET | `/api/ai-advisory/disease-detection/cloudinary-signature` | Ký request để FE upload ảnh thẳng lên Cloudinary (API secret không rời server) |
| POST | `/api/ai-advisory/disease-detection` | Gửi `imageUrl` (đã upload Cloudinary) → trả kết quả chẩn đoán, không giới hạn theo danh sách cây (xem [ai-disease-detection-flow.md](../data-flows/ai-disease-detection-flow.md)) |
| GET | `/api/ai-advisory/disease-detection/history` | Lịch sử chẩn đoán của farmer |
| GET | `/api/ai-advisory/disease-detection/{id}` | Chi tiết 1 lần chẩn đoán |
| DELETE | `/api/ai-advisory/disease-detection/{id}` | Xóa 1 lần chẩn đoán |
| GET | `/api/ai-advisory/chat/sessions` | Danh sách phiên chat của user, sort theo hoạt động gần nhất |
| POST | `/api/ai-advisory/chat/sessions` | Tạo phiên chat mới |
| DELETE | `/api/ai-advisory/chat/sessions/{id}` | Xóa phiên chat (cascade xóa tin nhắn + Redis context) |
| POST | `/api/ai-advisory/chat/sessions/{id}/messages` | Gửi tin nhắn, nhận phản hồi AI — kèm `cards` (giá/tin đăng) nếu chatbot tra được dữ liệu thật qua function-calling, xem [ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md) |
| GET | `/api/ai-advisory/chat/sessions/{id}/messages` | Lấy lịch sử hội thoại (fallback khi Redis hết hạn) |
| POST | `/api/ai-advisory/harvest-prediction` | Dự đoán thời điểm thu hoạch tối ưu (không giới hạn theo danh sách cây, xem [ai-harvest-prediction-flow.md](../data-flows/ai-harvest-prediction-flow.md)) |
| GET | `/api/ai-advisory/harvest-prediction/history` | Lịch sử dự đoán thu hoạch của farmer |
| GET | `/api/ai-advisory/harvest-prediction/weather-forecast` | Đọc forecast OpenWeatherMap đã cache theo địa điểm (`geo:province:*`/`weather:forecast:*`, xem mục Redis) — không gọi Gemini |
| POST | `/api/ai-advisory/internal/knowledge-ingest` | Nội bộ — nhận 1 chunk tài liệu RAG (`sourceDocument`, `chunkIndex`, `text`) từ `HappyFarmer.RagIngestor`, embed qua Gemini rồi lưu vào Qdrant. Xác thực `X-Internal-Api-Key` so với `Internal:IngestApiKey` (khác `Internal:ApiKey` — cái đó là key service này tự dùng khi gọi ra Auth Service) |

## DB schema (AiAdvisoryDb)

```
DiseaseDetections
  Id                       (PK)
  FarmerId
  ImageUrl                     # URL Cloudinary (frontend upload thẳng, BE chỉ lưu URL)
  CropTypeHint                 # gợi ý tùy chọn từ farmer, không phải nguồn xác định chính
  Note                         # ghi chú tùy chọn từ farmer
  IsHealthy                    # true = cây khỏe mạnh, không phát hiện bệnh (vẫn là kết quả hợp lệ)
  IdentifiedCropType           # Gemini tự nhận diện từ ảnh
  DiseaseName                  # null nếu IsHealthy=true
  ConfidenceScore
  Severity                     # null nếu IsHealthy=true
  Description
  TreatmentOrganicJson
  TreatmentChemicalJson
  PreventionTipsJson
  RecommendedActionsJson
  CreatedAt
                                # Lưu ý: ảnh không hợp lệ (isValidPlantImage=false từ Gemini) bị từ chối
                                # (422) ở controller, KHÔNG tới bước insert — bảng này chỉ chứa kết quả hợp lệ.

ChatSessions
  Id              (PK)
  FarmerId
  Title           (tự sinh từ tin nhắn đầu tiên, dùng cho sidebar lịch sử ở frontend)
  StartedAt
  LastActivityAt
  Status

ChatMessages
  Id          (PK)
  SessionId   (FK -> ChatSessions.Id)
  Sender      (User | AI)
  Content
  CreatedAt
  CardsJson   # card giá/tin đăng (JSON, polymorphic PriceCard|ListingCard) chatbot tra được qua
              # function-calling — chỉ tin nhắn AI mới set, null nếu lượt đó không tra dữ liệu gì

CropProfiles                 # bảng OVERRIDE tùy chọn — hiện chỉ seed 1 dòng (Lúa), KHÔNG giới hạn
  Id                (PK)      # phạm vi cây trồng được dự đoán (cây không có ở đây vẫn dự đoán được,
  CropTypeCode                # Gemini tự dùng kiến thức nông học chung) — xem ai-harvest-prediction-flow.md
  CropNameVi                  # unique
  AvgDaysToHarvest
  IdealTempMin
  IdealTempMax
  IdealRainfallMm
  Notes

HarvestPredictions
  Id                       (PK)
  FarmerId
  CropType                     # raw text farmer nhập, không FK tới CropProfiles
  PlantingDate
  Location
  RecommendedStartDate
  RecommendedEndDate
  ConfidenceLevel
  ReasoningText
  RiskFactorsJson
  WeatherSummaryJson           # null nếu weatherDataIncluded=false
  UsedVerifiedCropProfile      # true nếu match được CropProfiles, để trả về farmer (transparency)
  WeatherDataIncluded          # true nếu ngày thu hoạch dự kiến nằm trong 5 ngày dự báo được (free tier)
  CreatedAt
```

## Kafka

Không bắt buộc — xử lý đồng bộ trong request/response.

Optional nâng cao (Phase 6): publish `ai-advisory.disease-detected.v1` nếu chuyển sang xử lý nền (background job) cho ảnh lớn.

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `chat:session:{sessionId}:context` | Mảng JSON các tin nhắn gần nhất (sliding window ~10 tin) | 30 phút, refresh mỗi tin nhắn mới |
| `geo:province:{provinceName}` | Cache tọa độ lat/lon (OpenWeatherMap Geocoding API) — tọa độ tỉnh không đổi | 30 ngày |
| `weather:forecast:{lat},{lon}` | Cache response OpenWeatherMap 5 Day/3 Hour Forecast (free tier — KHÔNG phải 16 ngày) | 3 giờ |
| `ai:ratelimit:{feature}:{userId}:{date}` | Đếm số lượt gọi Gemini/user/ngày theo từng tính năng (`chat`/`harvest`/`disease`) — tách riêng để dùng hết quota 1 tính năng không chặn tính năng kia | 24 giờ |
