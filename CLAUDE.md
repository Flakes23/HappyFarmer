# HappyFarmer

Nền tảng hỗ trợ nông dân: tra cứu giá nông sản, AI tư vấn canh tác (nhận diện bệnh cây, dự đoán thu hoạch), marketplace kết nối nông dân-người mua, chatbot tiếng Việt.

**Trạng thái hiện tại**: đã scaffold xong solution backend (`HappyFarmer_Backend/`, .NET 10) — 5 project Web API rỗng (chưa có logic nghiệp vụ) + 1 shared class library. Frontend chưa bắt đầu.

## Backend solution — `HappyFarmer_Backend/HappyFarmer_Backend.slnx`

| Service (xem docs tương ứng) | Project path |
|---|---|
| [Auth Service](docs/architecture/services/auth-service.md) | `src/Services/AuthService/HappyFarmer.AuthService.Api/` |
| [Market Price Service](docs/architecture/services/market-price-service.md) | `src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/` |
| [AI Advisory Service](docs/architecture/services/ai-advisory-service.md) | `src/Services/AiAdvisoryService/HappyFarmer.AiAdvisoryService.Api/` |
| [Marketplace Service](docs/architecture/services/marketplace-service.md) | `src/Services/MarketplaceService/HappyFarmer.MarketplaceService.Api/` |
| [Notification Service](docs/architecture/services/notification-service.md) | `src/Services/NotificationService/HappyFarmer.NotificationService.Api/` |
| Thư viện dùng chung (Kafka DTO, JWT helper — chưa có nội dung, tạo dần theo nhu cầu) | `src/Shared/HappyFarmer.Shared.Contracts/` |

Mỗi service project đã reference `HappyFarmer.Shared.Contracts`. Build toàn solution: `dotnet build` tại `HappyFarmer_Backend/`.

## Bảng tra cứu tài liệu — đọc đúng 1 file, đừng quét cả docs/

| Hỏi về... | Đọc file |
|---|---|
| Sơ đồ tổng thể, service nào gọi service nào, có API Gateway không | `docs/architecture/01-overview.md` |
| JWT, đăng nhập, phân quyền role, CORS, rate-limit | `docs/architecture/02-security-auth.md` |
| API/DB/Kafka/Redis của **Auth Service** | `docs/architecture/services/auth-service.md` |
| API/DB/Kafka/Redis của **Market Price Service** | `docs/architecture/services/market-price-service.md` |
| API/DB/Kafka/Redis của **AI Advisory Service** | `docs/architecture/services/ai-advisory-service.md` |
| API/DB/Kafka/Redis của **Marketplace Service** | `docs/architecture/services/marketplace-service.md` |
| API/DB/Kafka/Redis của **Notification Service** | `docs/architecture/services/notification-service.md` |
| Luồng Kafka giá đổi → thông báo | `docs/architecture/data-flows/market-price-to-notification.md` |
| Luồng chụp ảnh cây bệnh → Claude Vision | `docs/architecture/data-flows/ai-disease-detection-flow.md` |
| Luồng chatbot tư vấn | `docs/architecture/data-flows/ai-chatbot-flow.md` |
| Luồng dự đoán thời điểm thu hoạch | `docs/architecture/data-flows/ai-harvest-prediction-flow.md` |
| Docker Compose, biến môi trường, CI/CD, deploy | `docs/architecture/03-infrastructure-deployment.md` |
| Thứ tự triển khai theo phase | `docs/architecture/04-roadmap.md` |

## Cách hỏi để tiết kiệm token nhất

- Nêu rõ tên service hoặc tên luồng trong câu hỏi (vd. "sửa API đăng ký ở Auth Service" thay vì "sửa auth") — nhờ bảng trên, việc đó giúp đi thẳng vào đúng file thay vì phải Grep/Explore toàn bộ `docs/`.
- Khi cần sửa nhiều file liên quan (vd. đổi tên 1 Kafka topic), hỏi rõ luôn: "đổi topic X ở cả market-price-service.md, notification-service.md và data-flows/market-price-to-notification.md" — 3 nơi duy nhất chứa tên topic đó.
- Nếu câu hỏi chỉ liên quan tới 1 service, không cần nhắc tới các service khác — file service được viết độc lập, chỉ link chéo khi cần.
