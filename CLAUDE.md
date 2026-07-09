# HappyFarmer

Nền tảng hỗ trợ nông dân: tra cứu giá nông sản, AI tư vấn canh tác (nhận diện bệnh cây, dự đoán thu hoạch), marketplace kết nối nông dân-người mua, chatbot tiếng Việt.

**Trạng thái hiện tại**: Auth Service, Market Price Service và Marketplace Service đã code xong đầy đủ (API + DB, JWT/header auth, đã test end-to-end). 2 service còn lại (AI Advisory, Notification) vẫn là skeleton rỗng. **Kiến trúc gateway đã đổi từ Nginx sang API Gateway (.NET YARP)** — project `HappyFarmer.ApiGateway` đã nối vào luồng dev: frontend gọi qua Gateway (`:5200`), Gateway tự verify JWT tập trung (JWKS từ Auth Service) rồi forward xuống service kèm header danh tính (`X-User-Id`/`X-User-Role`/`X-User-Phone`). **Auth Service, Market Price Service và Marketplace Service đều dùng chung model này**: không tự verify JWT, chỉ tin header Gateway gắn (`AddTrustedHeaderAuthentication`, xem mục Lưu ý kỹ thuật). Marketplace Service **chưa publish Kafka** (`marketplace.new-interest.v1`) — để dành Phase 4 khi Kafka được setup, xem TODO trong `ListingsController.Contact`. Frontend (`frontend/`) đã code xong đăng ký/đăng nhập/xem giá nông sản + **toàn bộ UI Chợ nông sản** (đăng bán kèm ảnh, tìm mua, liên hệ, quản lý tin/liên hệ của tôi — React + Tailwind + shadcn/ui), đã test end-to-end qua API Gateway với backend thật (browser thật, Playwright). **Upload ảnh dùng Cloudinary (signed upload) nhưng chưa có tài khoản Cloudinary thật** — code đã xong, chỉ chờ điền `CloudName`/`ApiKey`/`ApiSecret` qua `dotnet user-secrets` ở Marketplace Service (xem mục Hạ tầng local) thì upload ảnh mới chạy được, các tính năng khác không phụ thuộc việc này. Repo đã push lên GitHub public: `https://github.com/Flakes23/HappyFarmer`.

## Backend solution — `HappyFarmer_Backend/HappyFarmer_Backend.slnx`

| Service (xem docs tương ứng) | Project path | Trạng thái |
|---|---|---|
| [API Gateway](docs/architecture/01-overview.md#3-có-cần-api-gatewaybff-không) (.NET YARP) | `src/Gateway/HappyFarmer.ApiGateway/` | Done (routing + verify JWT tập trung) |
| [Auth Service](docs/architecture/services/auth-service.md) | `src/Services/AuthService/HappyFarmer.AuthService.Api/` | Done |
| [Market Price Service](docs/architecture/services/market-price-service.md) | `src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api/` | Done |
| [AI Advisory Service](docs/architecture/services/ai-advisory-service.md) | `src/Services/AiAdvisoryService/HappyFarmer.AiAdvisoryService.Api/` | Skeleton |
| [Marketplace Service](docs/architecture/services/marketplace-service.md) | `src/Services/MarketplaceService/HappyFarmer.MarketplaceService.Api/` | Done (chưa Kafka) |
| [Notification Service](docs/architecture/services/notification-service.md) | `src/Services/NotificationService/HappyFarmer.NotificationService.Api/` | Skeleton |
| Thư viện dùng chung — `Auth/` (JWT helper verify token qua JWKS cho Gateway + header helper cho service phía sau) | `src/Shared/HappyFarmer.Shared.Contracts/` | Có nội dung |

Mỗi service project đã reference `HappyFarmer.Shared.Contracts`. Build toàn solution: `dotnet build` tại `HappyFarmer_Backend/`.

## Frontend — `frontend/`

- Stack: React + TypeScript + Vite, TailwindCSS v3, shadcn/ui (Radix), TanStack Query + Zustand (session, persist localStorage), React Hook Form + Zod, react-router-dom, recharts.
- Đã code xong: đăng ký, đăng nhập (+ refresh-token tự động khi access token hết hạn), xem giá nông sản (tìm kiếm theo tên + lọc theo danh mục/loại/khu vực, phân trang, top biến động giá, biểu đồ lịch sử giá), **Chợ nông sản** (`/marketplace`: đăng bán kèm ảnh Cloudinary + sửa/đóng tin (Farmer), đăng yêu cầu mua (Buyer), liên hệ tin đăng, xem liên hệ/tin của tôi — route guard theo role qua `RequireAuth`).
- Chạy dev: `npm run dev` tại `frontend/` → `http://localhost:5173` (port cố định, khớp `Cors:AllowedOrigins` của API Gateway — đổi port thì phải sửa CORS ở Gateway, không phải ở từng service).
- Gọi qua API Gateway (`:5200`, xem `src/Gateway/HappyFarmer.ApiGateway/`) qua 1 biến duy nhất `VITE_API_GATEWAY_URL` trong `.env` (copy từ `.env.example`) — Gateway route `/api/auth/*`, `/api/market-price/*`, `/api/marketplace/*` xuống đúng service. Chạy dev cần bật cả 4 process: Auth Service (`:5242`), Market Price Service (`:5262`), Marketplace Service (`:5247`), API Gateway (`:5200`) — thiếu Gateway thì frontend không gọi được API nào (đã bỏ hẳn URL service lẻ khỏi frontend).
- `provinceId` trong form đăng ký hiện dùng danh sách 63 tỉnh/thành hardcode ở `frontend/src/lib/provinces.ts` (`id` chỉ là index cosmetic) — AuthService chưa có bảng tỉnh/thành thật, thay khi có API chính thức.

## Hạ tầng local (đã setup)

- Docker Desktop đã cài, data relocate sang `D:\Docker` (không đụng ổ C).
- `docker-compose.yml` ở root: chạy `sqlserver` (SQL Server 2022, port 1433) + `redis` (port 6379) — chưa có Kafka (để dành Phase 4). Lệnh: `docker compose up -d` tại root repo.
- `.env` (gitignored) chứa `SQLSERVER_SA_PASSWORD` — xem `.env.example` để biết các biến cần thiết.
- Connection string thật + secrets (Internal API key của Market Price Service, `Cloudinary:CloudName`/`ApiKey`/`ApiSecret` của Marketplace Service...) lưu qua `dotnet user-secrets` ở từng project, KHÔNG nằm trong `appsettings.*.json` (chỉ có placeholder `CHANGE_ME_VIA_USER_SECRETS`) — Cloudinary secrets hiện vẫn là placeholder vì chưa có tài khoản thật, xem mục Trạng thái hiện tại.
- Port từng service khi chạy dev (theo `Properties/launchSettings.json`): Auth Service `:5242`, Market Price Service `:5262`, Marketplace Service `:5247`, API Gateway `:5200` (Gateway là cổng duy nhất frontend gọi vào — xem mục Frontend).

## Lưu ý kỹ thuật quan trọng

- **JWT**: Auth Service ký token bằng RSA key cục bộ (RS256, tự sinh vào `keys/jwt-private.pem` nếu chưa có — đã gitignore) qua `.well-known/jwks.json`. Chỉ **API Gateway** verify chữ ký JWT (fetch JWKS qua helper dùng chung `HappyFarmer.Shared.Contracts/Auth/`, gọi `AddRemoteJwtAuthentication(configuration)` trong `Program.cs` của Gateway) rồi gắn header `X-User-Id`/`X-User-Role`/`X-User-Phone` khi forward. Auth Service, Market Price Service và Marketplace Service **không tự verify JWT nữa** — dùng `AddTrustedHeaderAuthentication()` (cùng namespace) để đọc thẳng 3 header đó, `[Authorize(Roles=...)]` vẫn hoạt động y hệt vì `TrustedHeaderAuthenticationHandler` build `ClaimsIdentity` với `nameType: "sub"`, `roleType: "role"` giống JWT trước đây — **dùng `AddTrustedHeaderAuthentication()`, không phải `AddRemoteJwtAuthentication()`**, khi thêm auth cho AI Advisory/Notification Service sau này (chỉ Gateway mới cần verify JWT gốc). Các service dùng model này cũng **không tự cấu hình CORS** — chỉ Gateway mới cần vì browser chỉ gọi tới Gateway.
- **Cạm bẫy cần biết (trade-off đã chấp nhận)**: `TrustedHeaderAuthenticationHandler` tin tưởng tuyệt đối 3 header trên, không kiểm tra request có thực sự đi qua Gateway hay không. Ở local dev, Auth Service (`:5242`)/Market Price Service (`:5262`)/Marketplace Service (`:5247`) vẫn bind port ra host nên ai đó gọi thẳng vào port đó (bỏ qua Gateway) và tự gắn `X-User-Id`/`X-User-Role` sẽ **bypass được auth hoàn toàn** — chỉ an toàn ở production vì kiến trúc target không public port 2 service này ra ngoài (chỉ Gateway mới gọi tới được, xem `docs/architecture/03-infrastructure-deployment.md`). Không viết thêm code "phòng vệ" cho việc này ở service — đây là model bảo mật cố ý (network isolation, không phải verify ở mỗi service).
- **Cạm bẫy đã gặp (lịch sử, vẫn áp dụng cho Gateway)**: `JwtBearerOptions` mặc định tự remap claim type ngắn (`"role"`, `"sub"`) sang URI dài `ClaimTypes.*`, khiến `RoleClaimType`/`NameClaimType` cấu hình không khớp claim thực tế trong token → mọi `[Authorize(Roles=...)]` âm thầm trả 403 dù token đúng role. Đã fix bằng `options.MapInboundClaims = false;` trong shared helper (`ServiceCollectionExtensions.AddRemoteJwtAuthentication`, chỉ còn Gateway dùng hàm này) — **bắt buộc giữ dòng này** nếu sau này có service nào khác cần tự verify JWT trực tiếp (hiếm, vì chuẩn giờ là tin header Gateway gắn).
- **Cạm bẫy đã gặp**: `Program.cs` của Auth Service và Market Price Service từng có `UseHttpsRedirection()`, khiến khi chạy `dotnet run` bình thường (Kestrel mở cả HTTP lẫn HTTPS theo `launchSettings.json`), mọi request từ frontend vào port HTTP dev (5242/5262) bị redirect sang port HTTPS dev (7283/7074) — mà chứng chỉ dev chưa được trình duyệt tin cậy nên request thất bại (chỉ đổi thứ tự với `UseCors()` là chưa đủ, vẫn còn bị redirect). Đã **bỏ hẳn `UseHttpsRedirection()`** ở cả 2 service (Marketplace Service tạo mới sau này cũng không có), vì theo kiến trúc TLS chỉ xử lý ở API Gateway khi deploy production (`docs/architecture/03-infrastructure-deployment.md`), các service nội bộ không cần tự redirect HTTPS — **không thêm lại middleware này** khi tạo Program.cs cho AI Advisory/Notification Service sau này, và cũng không thêm ở API Gateway trong môi trường dev nội bộ.
- **Bảng màu cố định của Frontend** ("Đồng quê ấm áp") — chỉ dùng các token sau cho mọi component (không dùng màu mặc định Tailwind/shadcn như `blue-500`, `gray-200`...), khai báo ở `frontend/src/index.css` (CSS variables, giá trị hex thô, không wrap `hsl()`) và map trong `frontend/tailwind.config.js`:

  | Token | Hex | Dùng cho |
  |---|---|---|
  | `primary` | `#2F855A` | Nút chính, header, link |
  | `primary-light` | `#48BB78` | Hover, badge, focus ring |
  | `accent` | `#DD6B20` | CTA phụ, chỉ báo giá tăng |
  | `background` | `#FFFBEB` | Nền trang |
  | `surface` | `#FFFFFF` | Nền card/form |
  | `text` / `text-muted` | `#1A202C` / `#6B7280` | Chữ chính / chữ phụ |
  | `border` | `#E2DCC8` | Viền, input |
  | `secondary` | `#F3EAD3` | Nền phụ |
  | `error` | `#E53E3E` | Lỗi, giá giảm |
  | `success` | `#38A169` | Thành công |

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
