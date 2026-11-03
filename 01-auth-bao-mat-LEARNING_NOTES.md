# Nhật ký ôn M1 — Auth & bảo mật liên service

> File học cá nhân, gitignored. Tài liệu chính thức: `docs/architecture/02-security-auth.md`,
> `docs/architecture/services/auth-service.md`.

## RS256 vs HMAC

- HMAC (HS256): 1 khóa bí mật dùng chung để ký + verify — muốn service khác tự verify được thì
  phải phát tán khóa bí mật ra nhiều nơi → rủi ro lộ khóa tăng theo số service.
- RS256: 2 khóa. Private key **chỉ Auth Service giữ** để ký (`JwtTokenService.GenerateAccessToken`,
  `HappyFarmer.AuthService.Api/Services/JwtTokenService.cs`). Public key phát tán thoải mái để
  verify, không dùng để giả mạo token được.
- `WellKnownController.GetJwks` (`Controllers/WellKnownController.cs`) expose
  `GET /.well-known/jwks.json`, export `ExportParameters(includePrivateParameters: false)` — chỉ
  đưa modulus/exponent public. Chỉ **API Gateway** fetch endpoint này (cache lại, không gọi Auth
  Service mỗi request).

## Refresh token — rotation, lưu hash, không lưu plaintext

Code: `RefreshToken.cs` (entity), `JwtTokenService.GenerateRefreshToken()`,
`AuthController.RefreshToken` (dòng 87-107).

- Sinh 64 byte random làm token thật; DB **chỉ lưu SHA256 hash** (`TokenHash`) — plaintext chỉ trả
  về client 1 lần lúc login/refresh, không bao giờ lưu trong DB.
- Entity có `RevokedAt` + `ReplacedByTokenId` — chuỗi rotation: mỗi lần refresh, token cũ bị
  revoke ngay và nối sang token mới qua `ReplacedByTokenId`.
- Check hợp lệ: `if (existing is null || !existing.IsActive)` (IsActive = chưa revoke + chưa hết hạn).

## Gap thật cần nhớ — chưa có reuse-detection

Nếu 1 refresh token **đã bị revoke** (đã dùng rồi) bị dùng lại — dấu hiệu kinh điển của token bị
đánh cắp (cả chủ thật và kẻ trộm cùng có token, ai dùng trước thì token bị revoke, người dùng sau
sẽ gặp lỗi) — code hiện tại chỉ trả lỗi "không hợp lệ hoặc hết hạn" **giống hệt** trường hợp hết
hạn bình thường, KHÔNG có logic "phát hiện reuse → revoke toàn bộ chain token của user". Đây là
gap thật so với pattern chuẩn (OWASP refresh-token rotation khuyến nghị). Nếu bị hỏi phỏng vấn về
bảo mật refresh token, đây là điểm nên chủ động nói là "biết thiếu, chưa làm" thay vì bị hỏi bất ngờ.

## Trusted Header pattern (Gateway → service phía sau)

`TrustedHeaderAuthenticationHandler.cs` (`HappyFarmer.Shared.Contracts/Auth/`) — đọc thẳng header
`X-User-Id`/`X-User-Role`/`X-User-Phone` build `ClaimsPrincipal`, **không verify chữ ký JWT** ở
service phía sau nữa — tin tưởng hoàn toàn Gateway đã verify. Dùng ở Auth/Market Price/Marketplace/
AI Advisory Service. Chỉ an toàn vì kiến trúc target không public các service này ra ngoài (chỉ
Gateway gọi tới được) — ở local dev do service vẫn bind port ra host nên lý thuyết có thể bypass
bằng cách gọi thẳng port + tự gắn header giả (trade-off chấp nhận ở dev).

## Rate limit đăng nhập

`LoginRateLimiter.cs` — đếm sai trong Redis, key `auth:ratelimit:login:{phoneOrIp}`, khóa sau
**5 lần sai trong 15 phút** (`MaxAttempts = 5`, `Window = 15 phút`), reset counter khi login thành công.
