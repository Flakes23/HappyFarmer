# 🌾 HappyFarmer

**Nền tảng số hỗ trợ nông dân Việt Nam** — tra cứu giá nông sản theo thời gian thực, AI tư vấn canh tác, và kết nối trực tiếp nông dân với người mua, không qua trung gian.

## Vấn đề

Nông dân Việt Nam hiện đang đối mặt với ba khó khăn lặp lại mỗi vụ mùa:

- **Thông tin giá cả mù mờ** — phụ thuộc hoàn toàn vào thương lái, không biết giá thị trường thực tế để mặc cả.
- **Thiếu chuyên gia tư vấn kịp thời** — cây bị bệnh hay đến lúc thu hoạch, thường phải đoán hoặc chờ hỏi người quen.
- **Đầu ra bấp bênh** — không có kênh trực tiếp để tiếp cận người mua, phải bán qua nhiều lớp trung gian và mất giá trị.

HappyFarmer giải quyết cả ba bằng một nền tảng duy nhất: dữ liệu giá minh bạch, AI tư vấn nông nghiệp bằng tiếng Việt, và một marketplace kết nối trực tiếp.

## Tính năng chính

### 📈 Giá nông sản thời gian thực
Tra cứu và theo dõi biến động giá nông sản theo khu vực, nhận thông báo khi giá mặt hàng quan tâm thay đổi.

### 🩺 Nhận diện bệnh cây bằng AI
Nông dân chụp ảnh lá cây, hệ thống dùng Gemini Vision để nhận diện bệnh và sinh lời khuyên điều trị dễ hiểu, bằng tiếng Việt.

### 🌾 Dự đoán thời điểm thu hoạch
Dự đoán thời điểm thu hoạch tối ưu dựa trên dữ liệu canh tác và thời tiết, giúp nông dân canh đúng thời điểm để được giá tốt và chất lượng nông sản cao nhất.

### 💬 Chatbot tư vấn nông nghiệp
Trợ lý AI trò chuyện bằng tiếng Việt, trả lời câu hỏi canh tác dựa trên kho tài liệu nông nghiệp (RAG) kết hợp dữ liệu giá/tin đăng/thời tiết thời gian thực.

### 🤝 Marketplace kết nối trực tiếp
Nông dân đăng tin bán nông sản, người mua tìm kiếm và liên hệ trực tiếp — không qua trung gian, không ẩn phí. Có chat real-time giữa hai bên sau khi liên hệ.

## Ảnh minh hoạ

_(sẽ bổ sung screenshot/demo khi giao diện các module hoàn thiện)_

## Công nghệ

Hệ thống được xây dựng theo kiến trúc microservices:

- **Backend**: 5 microservice .NET độc lập phía sau API Gateway (.NET + YARP), mỗi service một database riêng (SQL Server)
- **Frontend**: React + TypeScript + Vite, TailwindCSS, shadcn/ui
- **AI**: Google Gemini (chat, function-calling, vision cho nhận diện bệnh cây, embedding), Qdrant cho RAG
- **Hạ tầng**: Kafka (sự kiện bất đồng bộ), Redis (cache), Docker Compose, CI/CD qua GitHub Actions

Chi tiết đầy đủ về kiến trúc, luồng dữ liệu, và quyết định thiết kế: xem [docs/README.md](docs/README.md).

## Cấu trúc thư mục

```
HappyFarmer/
├── frontend/              # React + TypeScript + Vite
├── HappyFarmer_Backend/   # 5 microservice .NET + API Gateway
├── docs/                  # Tài liệu kiến trúc chi tiết
├── knowledge-base/        # Tài liệu nguồn cho RAG chatbot
└── docker-compose.yml     # Hạ tầng dev (SQL Server, Redis, Kafka, Qdrant)
```

## Tài liệu

- [docs/README.md](docs/README.md) — tài liệu kiến trúc đầy đủ: sơ đồ hệ thống, từng service, luồng dữ liệu AI, hạ tầng & triển khai, roadmap
