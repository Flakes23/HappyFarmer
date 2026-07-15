# Luồng: Chatbot tư vấn (AI Chat)

Thuộc [AI Advisory Service](../services/ai-advisory-service.md).

## Luồng xử lý

```mermaid
sequenceDiagram
  participant FE as Frontend
  participant AI as AI Advisory Service
  participant R as Redis
  participant G as Gemini API
  participant DB as AiAdvisoryDb

  FE->>AI: POST /chat/sessions (tạo session)
  AI->>DB: Insert ChatSessions
  FE->>AI: POST /chat/sessions/{id}/messages
  AI->>R: Đọc chat:session:{id}:context (sliding window ~10 tin)
  AI->>AI: Ghép system prompt + context + tin nhắn mới
  AI->>G: Gọi Gemini API (generateContent)
  G-->>AI: Phản hồi
  AI->>R: Append vào context, refresh TTL 30 phút
  AI->>DB: Insert ChatMessages (lưu vĩnh viễn)
  AI-->>FE: Trả phản hồi
```

## System prompt — nguyên tắc

- **Giọng điệu**: tiếng Việt gần gũi, dễ hiểu, dùng từ ngữ quen thuộc với nông dân, tránh thuật ngữ kỹ thuật/công nghệ. Persona: người tư vấn nông nghiệp thân thiện, kiên nhẫn.
- **Ranh giới chủ đề**: chỉ trả lời câu hỏi liên quan nông nghiệp/canh tác/sâu bệnh/thời tiết mùa vụ/giá nông sản. Câu hỏi ngoài phạm vi (chính trị, sức khỏe con người, lập trình, giải trí...) → từ chối lịch sự, gợi ý quay lại chủ đề canh tác, không cố trả lời.
- **Hỏi lại khi thiếu thông tin**: nếu tin nhắn chưa đủ dữ kiện để tư vấn chính xác (chưa rõ loại cây trồng, triệu chứng cụ thể, khu vực/mùa vụ) → hỏi lại 1 câu làm rõ thay vì đoán và đưa lời khuyên chung chung.

Cả 3 nguyên tắc trên được viết cố định trong system prompt tại `GeminiChatService.cs` (AI Advisory Service).

## Input

```json
{ "sessionId": "...", "message": "Cây lúa nhà tôi bị vàng lá phải làm sao?" }
```

## Output

```json
{ "sessionId": "...", "reply": "...", "timestamp": "..." }
```

## Ghi chú

- Redis (`chat:session:{sessionId}:context`) là nguồn context chính khi hội thoại đang hoạt động — nhanh, không cần query DB mỗi lần.
- `ChatMessages` trong DB là bản lưu vĩnh viễn, dùng khi Redis hết hạn (>30 phút không hoạt động) hoặc để xem lại lịch sử qua `GET /chat/sessions/{id}/messages`.
- Áp dụng rate limit theo `ai:ratelimit:{userId}:{date}` (xem [ai-advisory-service.md](../services/ai-advisory-service.md#redis)) để kiểm soát chi phí gọi Gemini API — check **trước** khi gọi Gemini, không gọi nếu đã vượt quota.
- **Kiểm soát tải ở tầng hệ thống** (tách biệt với quota/ngày ở trên): endpoint `POST /chat/sessions/{id}/messages` dùng `Microsoft.AspNetCore.RateLimiting` (built-in .NET, không cần package riêng) với policy `AddConcurrencyLimiter` — giới hạn số lượt gọi Gemini đồng thời (`Gemini:MaxConcurrentRequests`, mặc định 5), request vượt giới hạn sẽ **xếp hàng** (`QueueProcessingOrder.OldestFirst`, `Gemini:ConcurrencyQueueLimit` mặc định 50) thay vì bị từ chối ngay, chỉ trả `503` khi hàng đợi đầy. Không dùng message broker/Kafka cho việc này (over-engineering cho quy mô hiện tại).
- Gọi Gemini qua package chính thức `Google.GenAI` (API `generateContent`, không dùng "Interactions API" mới hơn vì SDK C# chưa hỗ trợ — xem `Services/GeminiChatService.cs`), model mặc định `gemini-3.1-flash-lite` (cấu hình qua `Gemini:Model` — `gemini-2.5-flash` không còn khả dụng cho tài khoản mới tạo; đổi từ `gemini-3.5-flash` sang bản lite vì model mới hay bị quá tải/nghẽn capacity phía Google). Bắt riêng `ClientError` (4xx, gồm rate-limit) và `ServerError` (5xx) của SDK để trả message tiếng Việt thân thiện thay vì để lỗi bung ra.
- Refusal/an toàn nội dung: nếu Gemini không trả về `Candidates` hoặc `PromptFeedback.BlockReason` có giá trị → coi là bị chặn, trả message xin lỗi tiếng Việt (tương đương check `StopReason == "refusal"` khi còn dùng Claude).
