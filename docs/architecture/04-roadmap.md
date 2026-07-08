# Roadmap triển khai (tham khảo)

Thứ tự các phase dưới đây là gợi ý dựa trên phụ thuộc giữa các service — có thể điều chỉnh theo tiến độ thực tế, không phải cam kết cố định.

| Phase | Nội dung | Ghi chú |
|---|---|---|
| 0 | Scaffolding: solution structure .NET, shared contracts library (Kafka event DTO, JWT helper), API Gateway skeleton (YARP), skeleton docker-compose, CI cơ bản | Nền tảng dùng chung cho mọi service |
| 1 | [Auth Service](services/auth-service.md) + [Market Price Service](services/market-price-service.md) (xem giá, seed data thay vì crawler thật) + Frontend cơ bản (đăng ký/đăng nhập/xem giá) + API Gateway (.NET YARP) | Có thể demo sớm |
| 2 | [Marketplace Service](services/marketplace-service.md) (đăng bán/mua/liên hệ) + hoàn thiện phân quyền role | **Đã xong API + DB + UI frontend** (đăng bán kèm ảnh Cloudinary, tìm kiếm, liên hệ, buy-requests, my-listings/my-interests, phân quyền Farmer/Buyer qua header Gateway, test end-to-end qua browser thật). Còn thiếu duy nhất: publish Kafka `marketplace.new-interest.v1` (để dành Phase 4) |
| 3 | [AI Advisory Service](services/ai-advisory-service.md) — 3 luồng thật (Claude Vision, chatbot, harvest prediction) + Redis cho AI | Phần "điểm nhấn" công nghệ của dự án |
| 4 | Kafka: Market Price → Notification, Marketplace → Notification; [Notification Service](services/notification-service.md) in-app + email | Hoàn thiện kiến trúc event-driven |
| 5 | CI/CD đầy đủ + deploy VPS + Vercel + hardening bảo mật (rate limit, blacklist JWT, HTTPS) + health checks | Sẵn sàng demo production |
| 6 (stretch) | Realtime (SignalR), Web Push notification, matching engine tự động nông dân-người mua, đa ngôn ngữ | Không bắt buộc |

Xem chi tiết kiến trúc từng thành phần tại [../README.md](../README.md).
