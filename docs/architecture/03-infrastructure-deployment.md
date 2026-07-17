# Hạ tầng & triển khai

> Đã deploy thật (không còn là kế hoạch) — chỉ Auth Service, Market Price Service, Marketplace
> Service + API Gateway. AI Advisory/Notification Service (còn skeleton) **chưa** nằm trong lần
> deploy này, để dành khi 2 service đó code xong. **Kafka đã setup** (single-node KRaft,
> `apache/kafka` image) — nhưng hiện chỉ phục vụ 1 topic `auth.user-updated.v1` giữa Auth Service
> và Marketplace Service; `marketplace.new-interest.v1` (Marketplace → Notification) vẫn để dành
> Phase 4 khi Notification Service code xong.

## 1. Hạ tầng thật đang chạy

| Thành phần | Ở đâu | Ghi chú |
|---|---|---|
| Backend (Gateway + 3 service + SQL Server + Redis + Kafka + Caddy) | 1 VPS Azure (Ubuntu 24.04, `Standard_B2als_v2` — 2 vCPU/4GB, gói **Azure for Students**) | Domain tạm `https://20.196.128.100.sslip.io` (chưa có domain thật — [sslip.io](https://sslip.io) tự phân giải `<ip>.sslip.io` về đúng IP, không cần đăng ký). RAM khá sát (4GB) — xem mục 2 về cách chia bộ nhớ giữa SQL Server/Kafka |
| Frontend | Vercel (`https://happy-farmer-beige.vercel.app`) | Deploy tự động theo Git integration, Root Directory = `frontend/` |
| CI/CD | GitHub Actions — `.github/workflows/deploy.yml` | Trigger khi push nhánh `master` (không phải `main` — nhánh chính của repo này là `master`) |

**Vì sao Azure thay vì Oracle Cloud/DigitalOcean như bàn ban đầu**: thử Oracle Cloud Free Tier
trước nhưng gặp lỗi tạo account (capacity/region, không liên quan tới thẻ) lặp lại nhiều lần
không qua được — chuyển sang Azure for Students (không cần khai báo thẻ tín dụng, $100 credit)
để không bị kẹt ở bước đăng ký.

## 2. Docker Compose topology (`docker-compose.prod.yml` ở root repo)

| Container | Image | Expose ra host/public? |
|---|---|---|
| `caddy` | `caddy:2-alpine` | **Có** — 80/443 (duy nhất container này chạm public Internet trực tiếp) |
| `gateway` | `ghcr.io/flakes23/happyfarmer-gateway` | Không (chỉ `caddy` gọi vào qua network nội bộ) |
| `auth-service` | `ghcr.io/flakes23/happyfarmer-auth-service` | Không |
| `market-price-service` | `ghcr.io/flakes23/happyfarmer-market-price-service` | Không |
| `marketplace-service` | `ghcr.io/flakes23/happyfarmer-marketplace-service` | Không |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | Chỉ bind `127.0.0.1:1433` (loopback VPS) — dùng để SSH tunnel chạy migration từ xa, không lộ ra Internet |
| `redis` | `redis:7-alpine` | Không |
| `kafka` | `apache/kafka:latest` | Không (chỉ 3 service .NET gọi qua network nội bộ `kafka:9092`, không ai từ ngoài VPS cần chạm trực tiếp) |

Database hiện có 3 (không phải 5 như kế hoạch cũ, vì AI Advisory/Notification chưa deploy):
`HappyFarmer_AuthDb`, `HappyFarmer_MarketPriceDb`, `HappyFarmer_MarketplaceDb` — cùng 1 instance
SQL Server, tách theo database logic (trade-off logical isolation, xem lý do ở bản kế hoạch gốc
— vẫn giữ nguyên).

**Chia RAM giữa SQL Server và Kafka** (VPS chỉ 4GB tổng, xem mục 1): `sqlserver` được cap
`MSSQL_MEMORY_LIMIT_MB: 2048` (mặc định SQL Server cố chiếm gần hết RAM host nếu không giới hạn),
`kafka` giới hạn heap JVM `KAFKA_HEAP_OPTS: -Xmx512m -Xms512m` (single-node KRaft, không có khối
lượng sự kiện lớn nên không cần heap to). Nên `free -h`/`docker stats --no-stream` kiểm tra lại
sau mỗi lần deploy có đổi cấu hình 2 service này. Nếu VPS không đủ tài nguyên, rollback an toàn:
`docker compose -f docker-compose.prod.yml stop kafka`, bỏ service `kafka` + 2 dòng
`Kafka__BootstrapServers`/`depends_on: kafka` khỏi `docker-compose.prod.yml` rồi redeploy — Auth
Service/Marketplace Service đã xử lý Kafka down gracefully (publish/consume best-effort), không
gãy tính năng chính khi thiếu Kafka.

**Dockerfile mỗi service** nằm cùng thư mục `Program.cs` của service đó (vd.
`src/Services/AuthService/HappyFarmer.AuthService.Api/Dockerfile`) — build context phải là
`HappyFarmer_Backend/` (thư mục chứa `.slnx`) vì mọi service đều tham chiếu chéo tới
`src/Shared/HappyFarmer.Shared.Contracts`.

**Khác với kế hoạch gốc**: TLS **không** do API Gateway tự làm — có thêm **Caddy** đứng trước
Gateway, tự lấy chứng chỉ Let's Encrypt (HTTP-01 challenge qua domain sslip.io) và reverse-proxy
vào `gateway:8080`. Cấu hình ở file `Caddyfile` (root repo), chỉ 3 dòng. Lý do: tự cấu hình Kestrel
HTTPS + cert renewal thủ công trong YARP phức tạp hơn nhiều so với để Caddy làm (auto HTTPS là
tính năng mặc định của Caddy, gần như không cần cấu hình).

**Volumes**: `sqlserver-data`, `redis-data`, `auth-keys` (giữ RSA key JWT của Auth Service qua
các lần redeploy — mất key = mọi token cũ bị vô hiệu), `caddy-data`/`caddy-config` (cert Let's
Encrypt, tránh xin cấp lại mỗi lần redeploy vì Let's Encrypt có rate limit).

## 3. Biến môi trường thật đang dùng

Không dùng `appsettings.Production.json` — toàn bộ cấu hình production (kể cả các giá trị không
bí mật như route nội bộ) truyền qua `environment:` trong `docker-compose.prod.yml`, giá trị thật
lấy từ file `.env` (không commit) trên VPS, do CI/CD tự ghi từ GitHub Secrets mỗi lần deploy
(xem mục 4). Xem `.env.prod.example` ở root repo để biết đủ danh sách biến cần thiết
(`SQLSERVER_SA_PASSWORD`, `INTERNAL_API_KEY`, `CLOUDINARY_CLOUD_NAME`/`ApiKey`/`ApiSecret`,
`CORS_ALLOWED_ORIGINS`) — chưa cần `SMTP_*`/`ANTHROPIC_API_KEY`/`OPENWEATHERMAP_API_KEY` vì các
tính năng dùng chúng chưa deploy. **Không có `KAFKA_BOOTSTRAP_SERVERS` dạng secret** — giá trị
`Kafka__BootstrapServers: "kafka:9092"` hardcode thẳng trong `docker-compose.prod.yml` (giống
`ConnectionStrings__Redis: "redis:6379"`), vì đây là tên DNS nội bộ Docker, không phải bí mật.

## 4. CI/CD (GitHub Actions — `.github/workflows/deploy.yml`)

Job `build-and-push` (matrix 4 service) → job `deploy` (cần `build-and-push` xong hết mới chạy):

1. **Build & push**: `docker/build-push-action` build từng Dockerfile (matrix 5 entry: 3 service +
   Gateway + `market-price-crawler` — crawler cũng build/push image mỗi lần deploy dù không chạy
   cùng `up -d`, xem mục 5), push lên **GitHub Container Registry**
   (`ghcr.io/flakes23/happyfarmer-<service>:latest` + `:<git-sha>`), đăng nhập bằng
   `secrets.GITHUB_TOKEN` có sẵn (không cần tạo Personal Access Token riêng như bản kế hoạch cũ
   ghi `GHCR_TOKEN`).
2. **Deploy**: `appleboy/scp-action` chép `docker-compose.prod.yml` + `Caddyfile` sang VPS →
   `appleboy/ssh-action` SSH vào VPS, ghi đè `.env` từ GitHub Secrets, `docker login ghcr.io`,
   `docker compose pull && docker compose up -d`, dọn image cũ (`docker image prune -f`).
3. **Migration**: **không** tự chạy khi service khởi động (khác kế hoạch gốc — service hiện tại
   không gọi `Database.Migrate()` trong `Program.cs`). Cách thật đang làm: SQL Server chỉ bind
   loopback VPS (mục 2), nên chạy `dotnet ef database update` **thủ công từ máy local**, trỏ qua
   SSH tunnel: `ssh -i <key> -L 14330:127.0.0.1:1433 <user>@<vps-ip>` rồi
   `dotnet ef database update --connection "Server=127.0.0.1,14330;Database=<TênDb>;User Id=sa;Password=<mật khẩu>;TrustServerCertificate=True;Encrypt=False"`
   chạy ở đúng thư mục project của từng service. **Lưu ý**: dùng `Server=localhost,...` từng bị
   timeout khó hiểu dù tunnel hoạt động bình thường — đổi sang `127.0.0.1` tường minh mới chạy
   được (nghi do `Microsoft.Data.SqlClient` xử lý resolve "localhost" không nhất quán trên
   Windows khi có cả IPv4/IPv6).
4. **Frontend**: Vercel tự deploy theo Git integration (không cần workflow riêng) — nhưng bắt
   buộc phải có `frontend/vercel.json` với rewrite rule `"/(.*)" -> "/index.html"`, nếu không
   mọi route ngoài `/` (vd. `/prices`, F5 reload) sẽ bị Vercel trả 404 vì React Router chỉ xử lý
   route ở phía client, server Vercel không có file thật ở path đó.
5. **Secrets cần khai báo trong GitHub repo Settings → Secrets and variables → Actions**:
   `VPS_HOST`, `VPS_USER`, `VPS_SSH_KEY` (nội dung private key `.pem` đầy đủ), rồi 6 biến ở mục 3
   (`SQLSERVER_SA_PASSWORD`, `INTERNAL_API_KEY`, `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`,
   `CLOUDINARY_API_SECRET`, `CORS_ALLOWED_ORIGINS`). Không cần `GHCR_TOKEN` riêng.

## 5. Nạp dữ liệu Market Price — lần đầu (thủ công) + hằng ngày (tự động)

**Lần đầu sau deploy** (database production mới chỉ có schema rỗng sau migration, chưa có
sản phẩm/giá nào) — chạy `HappyFarmer.MarketPriceCrawler` (`src/Tools/HappyFarmer.MarketPriceCrawler/`)
từ máy local, nhắm vào domain Gateway thật để nạp dữ liệu lần đầu:
```
dotnet user-secrets set "Api:BaseUrl" "https://20.196.128.100.sslip.io"
dotnet user-secrets set "Internal:ApiKey" "<đúng giá trị INTERNAL_API_KEY đã dùng cho VPS>"
dotnet run
```
Endpoint `POST /api/market-price/internal/crawl-ingest` là `[AllowAnonymous]` ở tầng ASP.NET
(Gateway không chặn), tự xác thực riêng bằng header `X-Internal-Api-Key` — không cần JWT người
dùng, Gateway chỉ forward nguyên header này xuống Market Price Service.

**Hằng ngày sau đó (tự động)** — `.github/workflows/crawl-daily.yml` chạy theo lịch
(`schedule: cron: "30 22 * * *"`, tức 05:30 giờ Việt Nam, sau khung giờ chốt giá chợ đầu mối
2-5h sáng), SSH vào VPS và chạy:
```
docker compose -f docker-compose.prod.yml --profile cron pull market-price-crawler
docker compose -f docker-compose.prod.yml --profile cron run --rm market-price-crawler
```
Service `market-price-crawler` trong `docker-compose.prod.yml` được gắn `profiles: ["cron"]` nên
**không** tự khởi động cùng `docker compose up -d` của `deploy.yml` (sẽ chạy xong rồi thoát ngay,
không phải long-running service) — chỉ được kích hoạt tường minh qua `--profile cron` bởi workflow
lịch trên. Gọi thẳng `http://market-price-service:8080` qua network nội bộ Docker
(`happyfarmer-net`), không qua Gateway/domain public vì đây là service-to-service trong cùng VPS.
Image crawler được build/push GHCR bởi chính `deploy.yml` (mục 4) mỗi khi push `master`, nên
`crawl-daily.yml` không cần build gì, chỉ pull tag `latest` mới nhất. Có thể trigger thủ công qua
`workflow_dispatch` (tab Actions trên GitHub) nếu cần chạy ngay ngoài lịch.

## 6. Việc còn thiếu / để dành Phase sau

- **AI Advisory Service, Notification Service**: chưa có Dockerfile, chưa nằm trong
  `docker-compose.prod.yml`, chưa có route trong Gateway production — thêm khi 2 service code
  xong (theo đúng mẫu 3 service đã deploy: Dockerfile cùng thư mục `Program.cs`, thêm service +
  route/cluster vào compose + Gateway env, thêm secrets tương ứng). Riêng AI Advisory Service khi
  deploy còn cần thêm **service `qdrant`** vào `docker-compose.prod.yml` (đã có ở
  `docker-compose.yml` local, xem `docs/architecture/data-flows/ai-chatbot-flow.md#rag--tra-cứu-tài-liệu-nông-nghiệp`)
  kèm volume riêng để không mất dữ liệu RAG mỗi lần redeploy, và chạy lại
  `HappyFarmer.RagIngestor` nhắm vào domain thật 1 lần để nạp tài liệu (tương tự mục 5 với
  `HappyFarmer.MarketPriceCrawler`).
- **Kafka**: đã setup ở cả local lẫn production, nhưng chỉ dùng cho `auth.user-updated.v1`
  (Auth → Marketplace). Marketplace Service vẫn chưa publish `marketplace.new-interest.v1` (xem
  TODO trong `ListingsController.Contact`/`BuyRequestsController`) — để dành khi Notification
  Service code xong, hạ tầng broker đã sẵn nên chỉ cần thêm producer call + consumer, không cần
  setup lại Kafka.
- **Domain thật**: hiện dùng `sslip.io` tạm — khi có domain riêng chỉ cần đổi 1 dòng trong
  `Caddyfile` (Caddy tự xin lại cert Let's Encrypt cho domain mới) và trỏ DNS domain đó về IP VPS.
- **Migration tự động khi deploy**: hiện vẫn làm thủ công qua SSH tunnel (mục 4.3) — có thể cải
  thiện sau bằng cách thêm 1 job trong CI/CD chạy migration qua container SDK tạm trên VPS, thay
  vì phải làm tay từ máy local mỗi lần có migration mới.
- **CORS_ALLOWED_ORIGINS**: phải cập nhật tay trong GitHub Secrets + chạy lại workflow
  (`workflow_dispatch`) mỗi khi đổi domain frontend — chưa tự động hoá.
