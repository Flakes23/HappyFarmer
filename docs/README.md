# HappyFarmer — Tài liệu kiến trúc

Nền tảng hỗ trợ nông dân: tra cứu giá nông sản theo thời gian thực, AI tư vấn canh tác (nhận diện bệnh cây, dự đoán thời điểm thu hoạch), kết nối trực tiếp nông dân với người mua, và chatbot tư vấn nông nghiệp bằng tiếng Việt.

Thư mục này chứa toàn bộ tài liệu kiến trúc — **đã khớp với code thật** (Auth Service, Market Price Service, Marketplace Service, AI Advisory Service done; Notification Service vẫn skeleton), không còn là tài liệu thiết kế thuần tuý. Đọc theo thứ tự dưới đây để nắm được bức tranh tổng thể trước khi đi vào chi tiết từng service — xem `CLAUDE.md` ở root repo để biết trạng thái tổng quan mới nhất và bảng tra cứu nhanh theo chủ đề.

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
7. [Frontend](architecture/05-frontend.md) — routing, pattern hub `/tu-van-ai`, Cloudinary upload dùng chung, chat SignalR, state management

## Tech stack tóm tắt

| Layer | Công nghệ |
|---|---|
| API Gateway | .NET + YARP (routing, verify JWT tập trung) |
| Backend | .NET microservices (5 service độc lập phía sau Gateway) |
| Frontend | React + TypeScript + Vite, TailwindCSS, shadcn/ui, TanStack Query + Zustand |
| Database | SQL Server + EF Core Migration (database-per-service) |
| Message Queue | Kafka (KRaft mode) — hiện chỉ dùng cho `auth.user-updated.v1` |
| Cache | Redis |
| Vector search (RAG) | Qdrant |
| AI | Gemini API (`Google.GenAI` — chat + function-calling, vision cho nhận diện bệnh cây, embedding cho RAG), OpenWeatherMap API (thời tiết) |
| Containerize | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Deploy | VPS (Docker Compose) cho backend, Vercel cho frontend |
