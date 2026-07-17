# HappyFarmer RAG Ingestor

Console app đọc file PDF tài liệu kỹ thuật nông nghiệp ở `knowledge-base/raw-sources/` (root repo),
trích text, cắt nhỏ (chunk), rồi gửi vào `POST /api/ai-advisory/internal/knowledge-ingest` của
`HappyFarmer.AiAdvisoryService.Api` — service đó tự embed (Gemini) và lưu vào Qdrant. Xem kiến trúc
đầy đủ tại `docs/architecture/data-flows/ai-chatbot-flow.md#rag--tra-cứu-tài-liệu-nông-nghiệp`.

Đây là công cụ nạp dữ liệu **1 lần/thủ công** (không chạy tự động theo lịch như
`HappyFarmer.MarketPriceCrawler`) — chạy lại khi thêm/sửa tài liệu nguồn.

## Chạy thử cục bộ

```bash
# 1. Hạ tầng (bao gồm Qdrant)
docker compose up -d          # ở thư mục gốc repo

# 2. Set API key nội bộ — PHẢI khớp Internal:IngestApiKey của AI Advisory Service
cd HappyFarmer_Backend/src/Services/AiAdvisoryService/HappyFarmer.AiAdvisoryService.Api
dotnet user-secrets set Internal:IngestApiKey "<giá trị bất kỳ>"
cd ../../../Tools/HappyFarmer.RagIngestor
dotnet user-secrets set Internal:ApiKey "<giá trị giống bước trên>"

# 3. Chạy AI Advisory Service (terminal riêng), rồi chạy tool này
dotnet run --project ../../Services/AiAdvisoryService/HappyFarmer.AiAdvisoryService.Api
dotnet run
```

Kiểm tra kết quả: Qdrant dashboard `http://localhost:6333/dashboard` → collection `knowledge_chunks`
→ số point phải khớp tổng số chunk log ra cuối chương trình. Hoặc hỏi thử chatbot 1 câu chỉ tài liệu
mới trả lời được, xem có trích dẫn đúng tên tài liệu không.

## Cấu hình (`appsettings.json`)

| Key | Mặc định | Ý nghĩa |
|---|---|---|
| `Api:BaseUrl` | `http://localhost:5224` | Base URL AI Advisory Service (port dev thật, không qua Gateway) |
| `SourceFolder` | `../../../../knowledge-base/raw-sources` | Đường dẫn **tương đối theo thư mục làm việc lúc chạy** `dotnet run` (tức thư mục project này) — không phải tương đối theo `bin/Debug/...` |

## Giới hạn đã biết

- **Không OCR** — dùng `UglyToad.PdfPig`, chỉ trích được text nhúng sẵn trong PDF. File dạng
  scan/ảnh trích ra rất ít/0 ký tự sẽ tự động bị bỏ qua (log rõ trong console), không chặn các file
  khác. Muốn xử lý được nhóm này cần thêm bước OCR (vd. Tesseract) — chưa làm.
- **Rate limit Gemini Embedding API (free tier)**: giới hạn 100 request/phút. Tool tự giãn cách
  ~700ms/chunk + retry backoff (tối đa 3 lần, chờ tăng dần) khi gặp lỗi — đủ để chạy hết ~700 chunk
  (12 tài liệu hiện tại) mà không bị chặn quota, nhưng chạy khá lâu (~10 phút). Nếu thêm nhiều tài
  liệu hơn đáng kể, cân nhắc tăng `DelayBetweenChunksMs` hoặc nâng cấp gói trả phí Gemini.
- **Chunking đơn giản** (cắt cố định theo ký tự, ưu tiên ranh giới câu) — không phải chunking theo
  ngữ nghĩa (semantic chunking). Đủ dùng cho quy mô 12 tài liệu hiện tại.
- **Idempotent theo (sourceDocument, chunkIndex)** — Point ID trong Qdrant là hash tất định, không
  phải random. Chạy lại tool với cùng file sẽ ghi đè đúng chunk cũ, không tạo trùng lặp — nhưng nếu
  **đổi tên file nguồn** thì chunk cũ (theo tên cũ) sẽ không bị xoá tự động, phải xoá tay qua Qdrant
  dashboard/API nếu muốn dọn dẹp.
