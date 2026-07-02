# AI Advisory Service

## Trách nhiệm

Ba luồng AI cốt lõi:
1. Nhận diện bệnh cây trồng từ ảnh (Claude Vision)
2. Chatbot tư vấn canh tác bằng tiếng Việt (Claude)
3. Dự đoán thời điểm thu hoạch tối ưu (OpenWeatherMap + Claude)

Chi tiết từng luồng (input/output schema, xử lý lỗi) nằm ở các file riêng trong [../data-flows/](../data-flows/):
- [ai-disease-detection-flow.md](../data-flows/ai-disease-detection-flow.md)
- [ai-chatbot-flow.md](../data-flows/ai-chatbot-flow.md)
- [ai-harvest-prediction-flow.md](../data-flows/ai-harvest-prediction-flow.md)

## API chính

| Method | Path | Mô tả |
|---|---|---|
| POST | `/api/ai-advisory/disease-detection` | Upload ảnh (multipart) → trả kết quả chẩn đoán |
| GET | `/api/ai-advisory/disease-detection/history` | Lịch sử chẩn đoán của farmer |
| POST | `/api/ai-advisory/chat/sessions` | Tạo phiên chat mới |
| POST | `/api/ai-advisory/chat/sessions/{id}/messages` | Gửi tin nhắn, nhận phản hồi AI |
| GET | `/api/ai-advisory/chat/sessions/{id}/messages` | Lấy lịch sử hội thoại (fallback khi Redis hết hạn) |
| POST | `/api/ai-advisory/harvest-prediction` | Dự đoán thời điểm thu hoạch tối ưu |
| GET | `/api/ai-advisory/crop-profiles` | Danh mục cây trồng (tra cứu tham chiếu) |

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
  StartedAt
  LastActivityAt
  Status

ChatMessages
  Id          (PK)
  SessionId   (FK -> ChatSessions.Id)
  Sender      (User | AI)
  Content
  CreatedAt

CropProfiles
  Id                (PK)
  CropTypeCode
  CropNameVi
  AvgDaysToHarvest
  IdealTempMin
  IdealTempMax
  IdealRainfallMm
  Notes

HarvestPredictions
  Id                    (PK)
  FarmerId
  CropType
  PlantingDate
  Location
  RecommendedStartDate
  RecommendedEndDate
  ConfidenceLevel
  ReasoningText
  WeatherSummaryJson
  CreatedAt
```

## Kafka

Không bắt buộc — xử lý đồng bộ trong request/response.

Optional nâng cao (Phase 6): publish `ai-advisory.disease-detected.v1` nếu chuyển sang xử lý nền (background job) cho ảnh lớn.

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `chat:session:{sessionId}:context` | Mảng JSON các tin nhắn gần nhất (sliding window ~10 tin) | 30 phút, refresh mỗi tin nhắn mới |
| `weather:forecast:{locationKey}` | Cache response OpenWeatherMap, tránh vượt rate limit free tier | 3 giờ |
| `ai:ratelimit:{userId}:{date}` | Đếm số lượt gọi Claude/user/ngày, kiểm soát chi phí API | 24 giờ |
