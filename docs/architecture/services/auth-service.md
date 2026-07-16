# Auth Service

## Trách nhiệm

Đăng ký/đăng nhập cho Farmer và Buyer, phát hành và làm mới JWT, quản lý refresh token, cung cấp JWKS cho các service khác verify token, quản lý profile cơ bản.

**Không** chứa logic nghiệp vụ nông sản/marketplace — các service khác không gọi ngược vào Auth Service ngoài việc lấy JWKS.

Xem luồng xác thực tổng thể tại [../02-security-auth.md](../02-security-auth.md).

## API chính

| Method | Path | Mô tả |
|---|---|---|
| POST | `/api/auth/register` | Đăng ký (role: Farmer/Buyer, phone/email, password, fullName, provinceId) |
| POST | `/api/auth/login` | Đăng nhập → trả accessToken + refreshToken |
| POST | `/api/auth/refresh-token` | Cấp accessToken mới từ refreshToken |
| POST | `/api/auth/logout` | Thu hồi refreshToken hiện tại |
| GET | `/api/auth/me` | Lấy thông tin user hiện tại (yêu cầu JWT) |
| PUT | `/api/auth/me` | Cập nhật profile (kèm `avatarUrl` sau khi đã upload lên Cloudinary); validate `provinceId` tồn tại trong bảng `Provinces` trước khi lưu (400 nếu không hợp lệ, tránh vi phạm FK bung lỗi 500 thô) |
| POST | `/api/auth/change-password` | Đổi mật khẩu |
| GET | `/api/auth/uploads/signature` | Lấy chữ ký signed upload ảnh đại diện lên Cloudinary (mọi role) |
| GET | `/api/auth/provinces` | Danh sách 63 tỉnh/thành (id, name) — `[AllowAnonymous]`, nguồn dữ liệu tham chiếu thật duy nhất cho `provinceId`, thay cho danh sách hardcode trước đây ở frontend |
| GET | `/.well-known/jwks.json` | Public key (RS256) cho service khác verify JWT |
| GET | `/api/auth/users` | Danh sách user (Admin), phục vụ duyệt/khoá tài khoản |
| GET | `/api/auth/internal/users/lookup?ids=1,2,3` | Nội bộ — tra tên/ngày tham gia/tỉnh-thành theo id, dùng cho service khác cần denormalize thông tin user (Marketplace Service khi tạo tin đăng, AI Advisory Service cho chatbot xưng hô cá nhân hoá). Xác thực bằng header `X-Internal-Api-Key`, xem mục [Internal API key](#internal-api-key) |

## DB schema (AuthDb)

```
Users
  Id            (PK)
  PhoneNumber
  Email
  PasswordHash
  FullName
  Role          (Farmer | Buyer | Admin)
  ProvinceId    (FK -> Provinces.Id, nullable, Restrict)
  AvatarUrl
  IsActive
  CreatedAt

Provinces                  # 63 tỉnh/thành, seed cố định qua EF HasData (Id giữ đúng thứ tự
  Id            (PK)       # danh sách cũ hardcode ở frontend để User.ProvinceId hiện có không
  Name                     # cần migrate dữ liệu) — nguồn dữ liệu tham chiếu thật duy nhất,
                           # thay cho "index cosmetic" trước đây, xem GET /api/auth/provinces
                           # unique

RefreshTokens
  Id                (PK)
  UserId            (FK -> Users.Id)
  TokenHash
  ExpiresAt
  CreatedAt
  RevokedAt
  ReplacedByTokenId
```

## Internal API key

Endpoint `GET /api/auth/internal/users/lookup` không dùng JWT người dùng — xác thực bằng header
`X-Internal-Api-Key` so với **dictionary** `Internal:ApiKeys:{TênService}` (vd.
`Internal:ApiKeys:Marketplace`, `Internal:ApiKeys:AiAdvisory`), không phải 1 key dùng chung cho mọi
caller. Mỗi service gọi vào đây (Marketplace Service, AI Advisory Service) cấu hình secret
`Internal:ApiKey` (tên config phía caller không đổi) với **giá trị riêng**, khớp đúng 1 entry trong
dictionary phía Auth Service — key của service nào cần thu hồi/xoay vòng thì đổi đúng entry đó,
không ảnh hưởng service khác. Thêm caller mới chỉ cần thêm 1 dòng config ở Auth Service, không cần
sửa code (so khớp theo giá trị, không cần biết trước tên caller).

## Kafka

Publish `auth.user-updated.v1` (đã setup) — khi `FullName` hoặc `AvatarUrl` thay đổi qua `PUT /api/auth/me` (chỉ đổi `Email`/`ProvinceId` thì không publish):

```json
{ "eventId": "...", "userId": 1, "fullName": "...", "avatarUrl": "...", "occurredAt": "..." }
```

Consumer: [Marketplace Service](marketplace-service.md#kafka) — đồng bộ lại `FarmerName`/`BuyerName`/`FarmerAvatarUrl`/`BuyerAvatarUrl` đã denormalize. Publish là best-effort (bọc try/catch, chỉ log warning nếu lỗi) — không bao giờ làm fail request cập nhật profile của người dùng.

Publish tuỳ chọn (optional, Phase 3+, chưa setup): `auth.user-registered.v1`

```json
{ "eventId": "...", "userId": 1, "role": "Farmer", "fullName": "...", "provinceId": 5, "occurredAt": "..." }
```

Dùng để Notification Service gửi email chào mừng — không bắt buộc cho MVP.

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `auth:ratelimit:login:{phoneOrIp}` | Đếm số lần đăng nhập sai, chống brute-force | 15 phút |
| `auth:blacklist:{jti}` | Access token bị thu hồi trước hạn khi logout (optional hardening) | = thời gian còn lại của token |
