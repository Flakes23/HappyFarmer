# AI Advisory Service

## Trách nhiệm

Ba luồng AI cốt lõi:
1. Nhận diện bệnh cây trồng từ ảnh (Claude Vision — chưa build)
2. Chatbot tư vấn canh tác bằng tiếng Việt (Gemini)
3. Dự đoán thời điểm thu hoạch tối ưu (OpenWeatherMap + Gemini)

Chi tiết từng luồng (input/output schema, xử lý lỗi) nằm ở các file riêng trong [../data-flows/](../data-flows/):
- [ai-disease-detection-flow.md](../data-flows/ai-disease-detection-flow.md)
- [ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md)
- [ai-harvest-prediction-flow.md](../data-flows/ai-harvest-prediction-flow.md)

## API chính

| Method | Path | Mô tả |
|---|---|---|
| POST | `/api/ai-advisory/disease-detection` | Upload ảnh (multipart) → trả kết quả chẩn đoán |
| GET | `/api/ai-advisory/disease-detection/history` | Lịch sử chẩn đoán của farmer |
| GET | `/api/ai-advisory/chat/sessions` | Danh sách phiên chat của user, sort theo hoạt động gần nhất |
| POST | `/api/ai-advisory/chat/sessions` | Tạo phiên chat mới |
| DELETE | `/api/ai-advisory/chat/sessions/{id}` | Xóa phiên chat (cascade xóa tin nhắn + Redis context) |
| POST | `/api/ai-advisory/chat/sessions/{id}/messages` | Gửi tin nhắn, nhận phản hồi AI |
| GET | `/api/ai-advisory/chat/sessions/{id}/messages` | Lấy lịch sử hội thoại (fallback khi Redis hết hạn) |
| POST | `/api/ai-advisory/harvest-prediction` | Dự đoán thời điểm thu hoạch tối ưu (không giới hạn theo danh sách cây, xem [ai-harvest-prediction-flow.md](../data-flows/ai-harvest-prediction-flow.md)) |
| GET | `/api/ai-advisory/harvest-prediction/history` | Lịch sử dự đoán thu hoạch của farmer |

## DB schema (AiAdvisoryDb)

```
DiseaseDetections
  Id                  (PK)
  FarmerId
  ImageUrl
  CropType
  DiseaseName
  ConfidenceScore
  Severity
  AnalysisResultJson
  CreatedAt

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
| `ai:ratelimit:{feature}:{userId}:{date}` | Đếm số lượt gọi Gemini/user/ngày theo từng tính năng (`chat` hoặc `harvest`) — tách riêng để dùng hết quota 1 tính năng không chặn tính năng kia | 24 giờ |
