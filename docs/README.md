# HappyFarmer — Tài liệu kiến trúc

Nền tảng hỗ trợ nông dân: tra cứu giá nông sản theo thời gian thực, AI tư vấn canh tác (nhận diện bệnh cây, dự đoán thời điểm thu hoạch), kết nối trực tiếp nông dân với người mua, và chatbot tư vấn nông nghiệp bằng tiếng Việt.

Thư mục này chứa toàn bộ tài liệu kiến trúc dùng làm nền tảng tham chiếu khi triển khai từng service. Đây là tài liệu thiết kế — chưa có code, đọc theo thứ tự dưới đây để nắm được bức tranh tổng thể trước khi đi vào chi tiết từng service.

## Thứ tự đọc gợi ý

1. [Tổng quan kiến trúc](architecture/01-overview.md) — sơ đồ hệ thống, nguyên tắc giao tiếp giữa các service
2. [Auth & bảo mật](architecture/02-security-auth.md) — luồng JWT, phân quyền
3. Chi tiết từng service:
   - [Auth Service](architecture/services/auth-service.md)
   - [Market Price Service](architecture/services/market-price-service.md)
   - [AI Advisory Service](architecture/services/ai-advisory-service.md)
   - [Marketplace Service](architecture/services/marketplace-service.md)
   - [Notification Service](architecture/services/notification-service.md)
4. Luồng dữ liệu chi tiết (data flows):
   - [Market Price → Notification](architecture/data-flows/market-price-to-notification.md)
   - [Nhận diện bệnh cây (AI Vision)](architecture/data-flows/ai-disease-detection-flow.md)
   - [Chatbot tư vấn (AI Chat)](architecture/data-flows/ai-chatbot-flow.md)
   - [Dự đoán thời điểm thu hoạch (AI Harvest Prediction)](architecture/data-flows/ai-harvest-prediction-flow.md)
5. [Hạ tầng & triển khai](architecture/03-infrastructure-deployment.md) — Docker Compose, biến môi trường, CI/CD
6. [Roadmap triển khai](architecture/04-roadmap.md) — thứ tự các phase tham khảo

## Tech stack tóm tắt

| Layer | Công nghệ |
|---|---|
| Backend | .NET microservices (5 service độc lập) |
| Frontend | ReactJS + TailwindCSS |
| Database | SQL Server + EF Core Migration (database-per-service) |
| Message Queue | Kafka (KRaft mode) |
| Cache | Redis |
| AI | Claude API (vision + chatbot), OpenWeatherMap API (thời tiết) |
| Containerize | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Deploy | VPS (Docker Compose) cho backend, Vercel cho frontend |
