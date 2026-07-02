# Luồng: Chatbot tư vấn (AI Chat)

Thuộc [AI Advisory Service](../services/ai-advisory-service.md).

## Luồng xử lý

```mermaid
sequenceDiagram
  participant FE as Frontend
  participant AI as AI Advisory Service
  participant R as Redis
  participant C as Claude API
  participant DB as AiAdvisoryDb

  FE->>AI: POST /chat/sessions (tạo session)
  AI->>DB: Insert ChatSessions
  FE->>AI: POST /chat/sessions/{id}/messages
  AI->>R: Đọc chat:session:{id}:context (sliding window ~10 tin)
  AI->>AI: Ghép system prompt + context + tin nhắn mới
  AI->>C: Gọi Claude API
  C-->>AI: Phản hồi
  AI->>R: Append vào context, refresh TTL 30 phút
  AI->>DB: Insert ChatMessages (lưu vĩnh viễn)
  AI-->>FE: Trả phản hồi
```

## System prompt — nguyên tắc

Giọng điệu tiếng Việt gần gũi, dễ hiểu, dùng từ ngữ quen thuộc với nông dân, tránh thuật ngữ kỹ thuật/công nghệ. Persona: người tư vấn nông nghiệp thân thiện, kiên nhẫn.

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
- Áp dụng rate limit theo `ai:ratelimit:{userId}:{date}` (xem [ai-advisory-service.md](../services/ai-advisory-service.md#redis)) để kiểm soát chi phí gọi Claude API.
