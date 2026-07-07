# Tổng quan kiến trúc

## 1. Sơ đồ tổng thể

```mermaid
graph TD
  U["Nông dân / Người mua<br/>ReactJS + Tailwind"] -->|HTTPS| GW["API Gateway (.NET YARP)<br/>path-based routing + verify JWT"]
  GW -->|"/api/auth/*"| AUTH[Auth Service]
  GW -->|"/api/market-price/*"| MP[Market Price Service]
  GW -->|"/api/ai-advisory/*"| AI[AI Advisory Service]
  GW -->|"/api/marketplace/*"| MKT[Marketplace Service]
  GW -->|"/api/notification/*"| NOTI[Notification Service]

  MP -- publish event --> KAFKA[("Kafka - KRaft")]
  MKT -- publish event --> KAFKA
  KAFKA -- subscribe --> NOTI

  AI --> CLAUDE[Claude API]
  AI --> OWM[OpenWeatherMap API]
  AI --> REDIS[("Redis")]
  MP --> REDIS
  NOTI --> REDIS

  AUTH --> SQL1[("SQL Server: AuthDb")]
  MP --> SQL2[("SQL Server: MarketPriceDb")]
  AI --> SQL3[("SQL Server: AiAdvisoryDb")]
  MKT --> SQL4[("SQL Server: MarketplaceDb")]
  NOTI --> SQL5[("SQL Server: NotificationDb")]
```

Frontend (React) chỉ nói chuyện với một cổng public duy nhất — API Gateway. Gateway verify JWT tập trung rồi route theo tiền tố path tới đúng service nội bộ, kèm danh tính người dùng qua header tin cậy (xem [02-security-auth.md](02-security-auth.md#2-luồng-verify-token-liên-service)). Kafka, Redis, SQL Server nằm hoàn toàn trong mạng nội bộ Docker, không public ra ngoài.

## 2. Nguyên tắc giao tiếp

- **Đồng bộ (REST/HTTPS)** — dùng cho mọi thao tác cần phản hồi ngay cho người dùng: đăng nhập, xem giá, đăng tin, gọi AI. Toàn bộ giao tiếp Frontend ↔ Service đi qua API Gateway bằng REST.
- **Bất đồng bộ (Kafka)** — chỉ dùng cho sự kiện "báo cho bên khác biết, không cần phản hồi ngay":
  - `market-price.price-changed.v1`
  - `marketplace.new-interest.v1`
  - (optional) `auth.user-registered.v1`

  Frontend không bao giờ giao tiếp trực tiếp với Kafka.
- **Service-to-service REST trực tiếp** (đồng bộ giữa 2 backend service) bị hạn chế tối đa để tránh coupling chặt. Nếu service A cần dữ liệu của service B thường xuyên, ưu tiên:
  1. Frontend gọi cả hai service rồi ghép dữ liệu ở client, hoặc
  2. Service B publish event, service A giữ một bản sao cục bộ (data duplication có kiểm soát — đúng tinh thần microservices).

  Ví dụ áp dụng: Notification Service tự giữ bảng `PriceWatchSubscriptions` thay vì gọi ngược Market Price Service ở mỗi request.

## 3. Có cần API Gateway/BFF không?

**Quyết định (cập nhật): dùng full API Gateway**, xây bằng **.NET + YARP** (project `HappyFarmer.ApiGateway`, thư mục `src/Gateway/`), thay cho phương án Nginx reverse-proxy "dumb" ban đầu.

Gateway đảm nhiệm:
- **Routing** theo path prefix (`/api/{service-prefix}/*`) tới đúng service nội bộ — giữ nguyên convention cũ.
- **TLS termination** ở tầng production (VPS).
- **Xác thực JWT tập trung**: Gateway verify chữ ký + issuer/audience/lifetime (RS256, JWKS từ Auth Service, dùng lại `AddRemoteJwtAuthentication` trong `HappyFarmer.Shared.Contracts`). Token hợp lệ → Gateway gắn header danh tính rồi forward; token thiếu/sai → Gateway forward tiếp **không kèm header** (Gateway hiện không tự chặn theo route, xem ghi chú bên dưới), và service phía sau tự quyết định 401 qua `[Authorize]` vì không thấy header.
- **Forward danh tính** qua header nội bộ tin cậy (`X-User-Id`, `X-User-Role`, `X-User-Phone`) cho service phía sau đọc bằng `AddTrustedHeaderAuthentication` (đã migrate xong ở Auth Service + Market Price Service), thay vì mỗi service tự verify lại token — xem chi tiết luồng tại [02-security-auth.md](02-security-auth.md#2-luồng-verify-token-liên-service).

Lý do đổi quyết định:
- YARP là thư viện .NET chính thức của Microsoft, tận dụng được kiến thức .NET sẵn có của solution thay vì học thêm cú pháp `nginx.conf`.
- Xác thực JWT một chỗ duy nhất giúp tránh lặp lại logic JWT Bearer ở cả 5 service, dễ thêm rate-limiting/aggregation sau này (không cần đợi tới Phase 6+ như dự tính cũ).
- Vẫn đảm bảo một cổng public duy nhất (giải quyết CORS, giấu port nội bộ) — giống mô hình production thật.

**Trade-off cần lưu ý**: Gateway giờ là single point of failure về cả routing lẫn auth — nếu Gateway lỗi, toàn bộ hệ thống không truy cập được (trước đây mỗi service vẫn tự đứng độc lập nếu Nginx lỗi ở tầng khác). Chấp nhận đánh đổi này vì đơn giản hoá được auth logic đáng kể ở quy mô dự án hiện tại.

**Đã migrate xong**: Auth Service và Market Price Service không còn tự verify JWT — dùng `AddTrustedHeaderAuthentication` (cùng thư viện chung) để tin header do Gateway gắn. Việc này chỉ an toàn vì kiến trúc target không public 2 service này ra ngoài (chỉ Gateway gọi tới được); ở local dev do 2 service vẫn bind port ra host nên về lý thuyết có thể bị bypass nếu gọi thẳng port và tự gắn header — chấp nhận trade-off này ở dev, xem `CLAUDE.md`.

## 4. Service discovery ở mức Docker Compose

Không dùng Consul/Eureka/Kubernetes. Dùng chính DNS nội bộ của Docker Compose network: mỗi service có `container_name` cố định (`auth-service`, `market-price-service`, `ai-advisory-service`, `marketplace-service`, `notification-service`), các service khác gọi qua `http://<container_name>:8080`. Vì mạng Docker Compose là tĩnh trên single-host, không cần cơ chế discovery động.

## 5. Bảng ma trận giao tiếp

| Từ | Đến | Kiểu | Mục đích |
|---|---|---|---|
| Frontend | Tất cả service (qua API Gateway) | REST | CRUD, gọi AI |
| Market Price Service | Kafka topic `market-price.price-changed.v1` | Publish | Báo giá thay đổi |
| Marketplace Service | Kafka topic `marketplace.new-interest.v1` | Publish | Báo có người quan tâm tin đăng |
| Notification Service | 2 topic trên | Subscribe | Sinh thông báo, gửi kênh phù hợp |
| API Gateway | Auth Service `.well-known/jwks.json` | REST (cache) | Lấy public key verify JWT tập trung — chỉ Gateway fetch, service phía sau không tự verify JWT nữa |

Chi tiết từng service xem tại thư mục [services/](services/), chi tiết từng luồng sự kiện/AI xem tại [data-flows/](data-flows/).
