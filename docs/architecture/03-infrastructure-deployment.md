# Hạ tầng & triển khai

## 1. Docker Compose topology

Mô tả kiến trúc container (chưa tạo file `docker-compose.yml` thật ở giai đoạn tài liệu này):

| Container | Vai trò | Expose ra host/public? |
|---|---|---|
| `api-gateway` | API Gateway (.NET YARP): routing theo path prefix, verify JWT tập trung, TLS termination | Có — 80/443 |
| `auth-service` | Auth Service (.NET) | Không |
| `market-price-service` | Market Price Service (.NET) | Không |
| `ai-advisory-service` | AI Advisory Service (.NET) | Không |
| `marketplace-service` | Marketplace Service (.NET) | Không |
| `notification-service` | Notification Service (.NET) | Không |
| `sqlserver` | 1 instance SQL Server duy nhất, host 5 database logic (`AuthDb`, `MarketPriceDb`, `AiAdvisoryDb`, `MarketplaceDb`, `NotificationDb`) | Không |
| `redis` | 1 instance dùng chung, namespace theo prefix key | Không |
| `kafka` | Chế độ KRaft (không cần Zookeeper riêng), 1 broker | Không |
| `kafka-ui` | Debug topic/message (chỉ dev) | Dev only |

**Trade-off SQL Server dùng chung**: mỗi service chỉ có connection string trỏ vào database riêng của mình (logical database-per-service). Đây là "logical isolation" chứ chưa phải "physical isolation" (5 container SQL Server riêng) — chấp nhận được ở cấp độ học tập/chi phí gần 0, tiết kiệm tài nguyên VPS free-tier.

**Volumes**: `sqlserver-data`, `redis-data` (tuỳ chọn AOF), `kafka-data`, `ai-uploads` (ảnh bệnh cây).

**Frontend**: không đóng gói trong compose production (deploy Vercel riêng). Có thể có `docker-compose.dev.yml` thêm container `frontend` (Vite dev server) chỉ cho local dev.

## 2. Networking

- Một bridge network duy nhất `happyfarmer-net` dùng chung cho toàn bộ container.
- Chỉ `api-gateway` (và `kafka-ui` khi dev) expose port ra ngoài; `sqlserver`, `redis`, `kafka`, 5 service .NET chỉ giao tiếp nội bộ qua tên container (xem [service discovery](01-overview.md#4-service-discovery-ở-mức-docker-compose)).

## 3. Biến môi trường (`.env.example`)

```
ANTHROPIC_API_KEY=
OPENWEATHERMAP_API_KEY=
JWT_ISSUER=happyfarmer-auth
JWT_AUDIENCE=happyfarmer-services
JWT_PRIVATE_KEY_PATH=/keys/private.pem      # chỉ auth-service dùng
JWT_PUBLIC_KEY_URL=http://auth-service:8080/.well-known/jwks.json
SQLSERVER_SA_PASSWORD=
REDIS_CONNECTION=redis:6379
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
CORS_ALLOWED_ORIGINS=https://happyfarmer.vercel.app,http://localhost:5173
SMTP_HOST=
SMTP_USER=
SMTP_PASS=
```

Không commit `.env` thật — chỉ commit `.env.example`.

## 4. CI/CD (GitHub Actions)

1. **On Pull Request**: restore + build + unit test (matrix theo từng service `.csproj`), lint frontend (eslint), `docker build` validate (không push).
2. **On merge main**: build & test lại → `docker build` 6 image (API Gateway + 5 service), tag `git-sha` + `latest` → push GitHub Container Registry (ghcr.io, miễn phí) → SSH vào VPS (vd. `appleboy/ssh-action`) chạy `docker compose pull && docker compose up -d`.
3. **Migration**: mỗi service tự chạy `dotnet ef database update` khi khởi động (guard theo `ASPNETCORE_ENVIRONMENT`) — đơn giản hoá cho dự án nhỏ, chấp nhận trade-off so với migration job riêng.
4. **Frontend**: không cần workflow riêng — Vercel tự deploy khi push (Git integration).
5. **Secrets cần khai báo trong GitHub repo settings**: `VPS_HOST`, `VPS_SSH_KEY`, `ANTHROPIC_API_KEY`, `OPENWEATHERMAP_API_KEY`, `GHCR_TOKEN`.

## 5. Deploy

- **Backend**: VPS (Oracle Cloud free tier / DigitalOcean) chạy `docker compose up -d` với toàn bộ container ở mục 1.
- **Frontend**: Vercel, tự động deploy theo Git integration.
