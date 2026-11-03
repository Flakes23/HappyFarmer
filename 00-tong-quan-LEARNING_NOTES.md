# Nhật ký ôn M0 — Bức tranh tổng thể & nguyên tắc kiến trúc

> File này để **học cá nhân**, không phải tài liệu chính thức — gitignored (`*_LEARNING_NOTES.md`).
> Ghi lại **vì sao** chọn kiến trúc này, không chỉ **cái gì** đã có. Tài liệu chính thức khớp code
> thật nằm ở `docs/architecture/01-overview.md` và `04-roadmap.md`.

## Vì sao microservices thay vì monolith

Lý do hay nói ra ("dễ bảo trì, dễ mở rộng, sau này scale lên Kubernetes") **đúng nhưng chưa phải
lý do thật/thuyết phục nhất**, vì đồ án chưa có tải thật để chứng minh cần scale. Dễ bị bẻ lại
kiểu YAGNI: "sao không làm monolith trước, tách khi thực sự cần?".

**Lý do thật, nên nói thẳng khi trình bày**: chủ động chọn độ phức tạp cao hơn cần thiết so với
quy mô thật của bài toán, để **luyện đúng các pattern hệ thống phân tán thật** — service-to-service
auth, event-driven (Kafka), database-per-service, resilience pipeline, API Gateway tập trung.
"Có thể scale lên K8s sau này" là lợi ích phụ đi kèm, không phải động lực chính lúc quyết định.

## Nguyên tắc giao tiếp giữa các service

| Kiểu | Dùng khi | Ví dụ |
|---|---|---|
| REST đồng bộ (qua API Gateway) | User gọi, cần phản hồi ngay để cập nhật UI | Login, xem giá, đăng tin, gọi AI |
| Kafka bất đồng bộ | "Báo cho bên khác biết", không cần phản hồi ngay, có thể nhiều service cùng lắng nghe | `auth.user-updated.v1`, `market-price.price-changed.v1` (kế hoạch) |
| REST service-to-service trực tiếp (2 backend gọi nhau) | **Hạn chế tối đa** — ưu tiên FE gọi cả 2 rồi ghép ở client, hoặc publish event + giữ bản sao cục bộ ở service cần | — |

**Ngoại lệ duy nhất** của hàng cuối: **AI Advisory Service** chủ động gọi REST trực tiếp sang
Market Price / Marketplace / **Auth Service** (Auth là bên bị gọi, không phải bên gọi — nhớ nhầm
lúc ôn). Lý do được phép phá luật: chatbot function-calling cần dữ liệu **thật, tức thời** (giá,
tin đăng, thông tin user) để trả lời chính xác — data duplicate cục bộ kiểu event-driven thông
thường có thể bị cũ/sai lệch đúng lúc cần chính xác nhất.

## API Gateway: Nginx → .NET/YARP

Đổi từ dự định ban đầu (Nginx reverse-proxy "dumb") sang viết Gateway bằng .NET + YARP. Lý do cụ thể:
- YARP là thư viện chính thức của .NET → tận dụng kiến thức .NET sẵn có, đỡ học thêm cú pháp `nginx.conf`.
- Gom **verify JWT về đúng 1 chỗ** (Gateway) thay vì lặp lại logic JWT Bearer ở cả 5 service.
- Vẫn giữ 1 cổng public duy nhất (giải quyết CORS, giấu port nội bộ) — giống mô hình production thật.

**Trade-off phải đánh đổi** (câu hỏi bị hụt lúc ôn, cần nhớ kỹ): Gateway giờ là **single point of
failure cho cả routing lẫn auth** — Gateway chết thì toàn hệ thống không vào được, dù các service
phía sau vẫn chạy tốt. Trước đây (Nginx chỉ routing) mỗi service vẫn tự đứng độc lập được nếu tầng
ngoài lỗi. Chấp nhận đánh đổi vì gom auth logic về 1 chỗ đáng giá hơn rủi ro SPOF ở quy mô hiện tại.

## Việc cần nhớ kỹ hơn cho lần ôn sau

- Ngoại lệ REST service-to-service là **AI Advisory Service**, không phải Auth Service — Auth chỉ
  là 1 trong 3 service bị gọi tới.
- Khi giải thích "vì sao microservices", ưu tiên nói lý do học tập (luyện pattern phân tán thật)
  trước, "khả năng scale tương lai" chỉ nên là ý phụ, tránh nghe như biện minh sáo rỗng.
- Trade-off SPOF của Gateway (routing + auth gộp 1 chỗ) là câu hay bị hỏi vặn, cần trả lời được
  ngay không cần nhớ lại.
