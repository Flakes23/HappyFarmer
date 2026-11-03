# Nhật ký ôn M3 — Marketplace Service + Chat real-time (SignalR)

> File học cá nhân, gitignored. Tài liệu chính thức: `docs/architecture/services/marketplace-service.md`,
> `docs/architecture/05-frontend.md` mục 4.

## Luồng "liên hệ" = tạo record `Interest`

Click "Liên hệ người bán" + gõ tin nhắn đầu tiên → tạo ngay 1 `Interest` (`Entities/Interest.cs`),
không chỉ là UI state:
- `InitiatorUserId` = người bấm liên hệ (buyer với listing, hoặc farmer với buy-request).
- `TargetUserId` = chủ tin đăng/yêu cầu mua (bên bị liên hệ).
- Gắn với **đúng 1 trong 2**: `ListingId` HOẶC `BuyRequestId` (không bao giờ cả 2) — cùng 1 bảng
  `Interest` dùng chung cho cả 2 chiều (buyer liên hệ listing / farmer phản hồi buy-request).
- Tin nhắn đầu tiên lưu ngay trên field `Interest.Message`.
- `Status`: `Pending` → `Responded` khi **Target** (bên bị liên hệ) gửi tin đầu tiên trả lời
  (`InterestsController.SendMessage` dòng 92-95) — status theo dõi "bên kia đã trả lời chưa",
  không phải "đã có chat hay chưa".

Sau đó cả 2 bên chat qua `/marketplace/my-interests` (`InterestThreadPage`, tab "Liên hệ của tôi").

## Vì sao gửi tin nhắn đi qua REST rồi mới broadcast qua Hub (KHÔNG phải vì WebSocket handshake)

Nhớ nhầm ban đầu: nghĩ do SignalR dùng WebSocket "bắt tay" — sai chỗ. WebSocket chỉ là **transport
để đẩy tin real-time** cho client đang kết nối, không liên quan tới việc chọn REST-trước-rồi-broadcast.

Lý do thật: `ChatHub` **không có method nào cho client gọi để gửi tin** — chỉ push, không nhận.
Gửi tin nhắn là `[HttpPost]` REST bình thường (`InterestsController.SendMessage`): validate người
gọi là participant → lưu `Message` vào DB → cập nhật `Interest.Status` nếu cần → `SaveChangesAsync()`
→ **sau đó mới** gọi `hubContext.Clients.Group(ChatHub.GroupName(id)).SendAsync("ReceiveMessage", ...)`
để đẩy real-time cho ai đang mở thread.

**Lý do kiến trúc**: tránh **dual write path**. Nếu cho client gọi thẳng Hub để gửi tin, phải lặp
lại y hệt logic validate + lưu DB + update status bên trong Hub method → 2 đường code dễ lệch nhau
theo thời gian. Ngoài ra REST + DB write là nguồn sự thật đáng tin cậy duy nhất ("tin đã lưu hay
chưa"), còn WebSocket không đảm bảo delivery (client mất kết nối sẽ mất tin nếu chỉ gửi qua Hub).

## Đồng bộ FarmerName/avatar qua Kafka — nhớ đúng

Auth Service publish `auth.user-updated.v1` khi `FullName`/`AvatarUrl` đổi → Marketplace Service
(`UserProfileUpdatedConsumer.cs`) subscribe, cập nhật lại bản sao denormalize (`FarmerName`/avatar
lưu sẵn trong Listing/Interest để không phải gọi Auth Service mỗi lần hiển thị).
