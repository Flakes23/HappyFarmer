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
| PUT | `/api/auth/me` | Cập nhật profile (kèm `avatarUrl` sau khi đã upload lên Cloudinary) |
| POST | `/api/auth/change-password` | Đổi mật khẩu |
| GET | `/api/auth/uploads/signature` | Lấy chữ ký signed upload ảnh đại diện lên Cloudinary (mọi role) |
| GET | `/.well-known/jwks.json` | Public key (RS256) cho service khác verify JWT |
| GET | `/api/auth/users` | Danh sách user (Admin), phục vụ duyệt/khoá tài khoản |

## DB schema (AuthDb)

```
Users
  Id            (PK)
  PhoneNumber
  Email
  PasswordHash
  FullName
  Role          (Farmer | Buyer | Admin)
  ProvinceId
  AvatarUrl
  IsActive
  CreatedAt

RefreshTokens
  Id                (PK)
  UserId            (FK -> Users.Id)
  TokenHash
  ExpiresAt
  CreatedAt
  RevokedAt
  ReplacedByTokenId
```

## Kafka

Publish tuỳ chọn (optional, Phase 3+): `auth.user-registered.v1`

```json
{ "eventId": "...", "userId": 1, "role": "Farmer", "fullName": "...", "provinceId": 5, "occurredAt": "..." }
```

Dùng để Notification Service gửi email chào mừng — không bắt buộc cho MVP.

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `auth:ratelimit:login:{phoneOrIp}` | Đếm số lần đăng nhập sai, chống brute-force | 15 phút |
| `auth:blacklist:{jti}` | Access token bị thu hồi trước hạn khi logout (optional hardening) | = thời gian còn lại của token |
