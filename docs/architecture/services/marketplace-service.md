# Marketplace Service

## Trách nhiệm

Đăng bán nông sản (Farmer), đăng yêu cầu mua (Buyer), tìm kiếm, ghi nhận "quan tâm/liên hệ" giữa hai bên.

**Không** xử lý thanh toán (ngoài phạm vi MVP) — chỉ kết nối thông tin liên hệ.

## API chính

| Method | Path | Mô tả |
|---|---|---|
| POST | `/api/marketplace/listings` | Tạo tin đăng bán (Farmer) |
| GET | `/api/marketplace/listings?productId=&regionId=&status=&search=&minPrice=&maxPrice=&sort=&page=&pageSize=` | Tìm kiếm tin đăng (phân trang, `sort`: `newest`\|`price_asc`\|`price_desc`) — trả về `PagedResult<ListingResponse>` |
| GET | `/api/marketplace/listings/{id}` | Chi tiết tin đăng |
| PUT | `/api/marketplace/listings/{id}` | Cập nhật tin (owner) |
| PATCH | `/api/marketplace/listings/{id}/close` | Đóng tin — đã bán/hết hàng (owner) |
| POST | `/api/marketplace/buy-requests` | Đăng yêu cầu mua (Buyer) |
| GET | `/api/marketplace/buy-requests?productId=&regionId=&status=&search=&minPrice=&maxPrice=&sort=&page=&pageSize=` | Tìm kiếm yêu cầu mua (phân trang, cùng tham số như trên) — trả về `PagedResult<BuyRequestResponse>` |
| POST | `/api/marketplace/listings/{id}/contact` | Bày tỏ quan tâm/liên hệ (Buyer) → tạo Interest, trigger thông báo |
| POST | `/api/marketplace/buy-requests/{id}/contact` | Bày tỏ quan tâm/liên hệ với yêu cầu mua (Farmer) → tạo Interest |
| GET | `/api/marketplace/my-listings` | Quản lý tin đăng của tôi |
| GET | `/api/marketplace/my-interests` | Quản lý liên hệ của tôi |
| GET | `/api/marketplace/my-interests/unread-count` | Tổng số tin nhắn chưa đọc của user (badge) |
| GET | `/api/marketplace/my-interests/{id}/messages` | Lịch sử tin nhắn của 1 Interest |
| POST | `/api/marketplace/my-interests/{id}/messages` | Gửi tin nhắn trong 1 Interest — xem mục [Chat real-time (SignalR)](#chat-real-time-signalr) |
| POST | `/api/marketplace/my-interests/{id}/read` | Đánh dấu đã đọc tin nhắn của 1 Interest (cập nhật `InitiatorReadAt`/`TargetReadAt`) |
| GET | `/api/marketplace/uploads/signature` | Sinh chữ ký signed upload lên Cloudinary (Farmer, dùng khi đăng ảnh tin bán — xem mục Upload ảnh) |
| POST | `/api/marketplace/internal/backfill-avatars` | Nội bộ (xác thực bằng `X-Internal-Api-Key`, không phải JWT) — đồng bộ lại FarmerName/BuyerName/avatar cho dữ liệu tạo trước khi có Kafka `auth.user-updated.v1`, chạy tay khi cần |

## DB schema (MarketplaceDb)

```
Listings
  Id            (PK)
  FarmerId
  FarmerName        (denormalize lúc tạo tin, tự cập nhật lại qua Kafka auth.user-updated.v1)
  FarmerJoinedAt
  FarmerAvatarUrl
  ProductId
  Quantity
  Unit
  PricePerUnit
  RegionId
  Description
  Status        (Active | Sold | Closed | Expired)
  CreatedAt
  ExpiresAt

ListingImages
  Id            (PK)
  ListingId     (FK -> Listings.Id)
  ImageUrl
  SortOrder

BuyRequests
  Id                (PK)
  BuyerId
  BuyerName         (denormalize lúc tạo, tự cập nhật lại qua Kafka auth.user-updated.v1)
  BuyerJoinedAt
  BuyerAvatarUrl
  ProductId
  DesiredQuantity
  RegionId
  MaxPricePerUnit
  Description
  Status
  CreatedAt

Interests
  Id                (PK)
  ListingId         (FK -> Listings.Id, nullable)
  BuyRequestId      (FK -> BuyRequests.Id, nullable)
  InitiatorUserId
  TargetUserId
  Message
  Status            (Pending | Responded)
  InitiatorReadAt       # null nếu InitiatorUserId chưa đọc tin nhắn mới nhất
  TargetReadAt          # null nếu TargetUserId chưa đọc tin nhắn mới nhất
  CreatedAt

Messages
  Id                (PK)
  InterestId        (FK -> Interests.Id)
  SenderUserId
  Content
  CreatedAt
```

## Kafka

### Publish

`marketplace.new-interest.v1` (**chưa wiring** — chờ Notification Service, xem TODO trong `ListingsController.Contact`/`BuyRequestsController`):

```json
{
  "eventId": "...",
  "listingId": 42,
  "targetUserId": 7,
  "initiatorUserId": 15,
  "message": "...",
  "occurredAt": "..."
}
```

Consumer (khi làm): xem [Notification Service](notification-service.md).

### Subscribe

`auth.user-updated.v1` (đã setup, consumer group `marketplace-service-group`) — khi [Auth Service](auth-service.md#kafka) báo `FullName`/`AvatarUrl` đổi, cập nhật lại `Listings.FarmerName`/`FarmerAvatarUrl` và `BuyRequests.BuyerName`/`BuyerAvatarUrl` tương ứng (xem `UserProfileUpdatedConsumer`, `DenormalizedUserSyncService` — dùng chung logic với `InternalController.BackfillAvatars`). Xử lý idempotent (ghi đè cùng giá trị nhiều lần vô hại) — lỗi xử lý 1 message chỉ log rồi bỏ qua, không retry/dead-letter (cache denormalize không critical, tự khớp lại ở lần đổi profile tiếp theo).

## Redis

Optional, nice-to-have — không critical cho MVP:

| Key | Mục đích | TTL |
|---|---|---|
| `marketplace:listings:search:{queryHash}` | Cache trang kết quả tìm kiếm phổ biến | 5 phút |

## Chat real-time (SignalR)

Mỗi `Interest` có 1 luồng tin nhắn (bảng `Messages`) giữa `InitiatorUserId` và `TargetUserId`. Ngoài REST (`GET/POST /api/marketplace/my-interests/{id}/messages`) để load lịch sử/gửi tin, còn có `ChatHub` (SignalR) đẩy tin nhắn real-time:

- Frontend kết nối `/api/marketplace/hubs/chat` (qua API Gateway, bearer token) rồi `invoke('JoinConversation', interestId)`/`'LeaveConversation'` khi vào/rời trang chi tiết 1 Interest.
- Khi có tin nhắn mới, Hub gửi event `ReceiveMessage` cho các client đang join đúng `interestId`.
- Có thêm event toàn cục `UnreadCountChanged` (không cần join theo interest) để cập nhật badge tổng số tin chưa đọc ở mọi trang.
- Đây là tính năng phát sinh thêm ngoài phạm vi thiết kế ban đầu (roadmap Phase 6 từng liệt kê SignalR như "stretch, tương lai") — đã build và dùng thật ở Marketplace Service, không phải kế hoạch nữa.

## Upload ảnh (Cloudinary)

Ảnh tin đăng (`Listings.ImageUrls`, gửi kèm khi tạo tin) lưu trên Cloudinary (free tier), không tự lưu file trên server. Dùng **signed upload**: frontend gọi `GET /api/marketplace/uploads/signature` (yêu cầu đăng nhập Farmer) để lấy chữ ký sinh từ Cloudinary API Secret (`CloudinarySignatureService`, chỉ nằm ở backend), rồi POST file thẳng lên `api.cloudinary.com` kèm chữ ký đó — API Secret không bao giờ lộ ra frontend. Cấu hình qua section `Cloudinary` trong `appsettings.Development.json` (`CloudName`, `ApiKey`, `ApiSecret` — 3 giá trị này set qua `dotnet user-secrets`, không hardcode).
