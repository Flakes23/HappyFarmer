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
| GET | `/api/marketplace/my-listings` | Quản lý tin đăng của tôi |
| GET | `/api/marketplace/my-interests` | Quản lý liên hệ của tôi |
| GET | `/api/marketplace/uploads/signature` | Sinh chữ ký signed upload lên Cloudinary (Farmer, dùng khi đăng ảnh tin bán — xem mục Upload ảnh) |

## DB schema (MarketplaceDb)

```
Listings
  Id            (PK)
  FarmerId
  FarmerName        (denormalize 1 lần lúc tạo tin, xem AuthServiceClient)
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
  BuyerName         (denormalize 1 lần lúc tạo, xem AuthServiceClient)
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
  CreatedAt
```

## Kafka

Publish `marketplace.new-interest.v1`:

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

Consumer: xem [Notification Service](notification-service.md).

## Redis

Optional, nice-to-have — không critical cho MVP:

| Key | Mục đích | TTL |
|---|---|---|
| `marketplace:listings:search:{queryHash}` | Cache trang kết quả tìm kiếm phổ biến | 5 phút |

## Upload ảnh (Cloudinary)

Ảnh tin đăng (`Listings.ImageUrls`, gửi kèm khi tạo tin) lưu trên Cloudinary (free tier), không tự lưu file trên server. Dùng **signed upload**: frontend gọi `GET /api/marketplace/uploads/signature` (yêu cầu đăng nhập Farmer) để lấy chữ ký sinh từ Cloudinary API Secret (`CloudinarySignatureService`, chỉ nằm ở backend), rồi POST file thẳng lên `api.cloudinary.com` kèm chữ ký đó — API Secret không bao giờ lộ ra frontend. Cấu hình qua section `Cloudinary` trong `appsettings.Development.json` (`CloudName`, `ApiKey`, `ApiSecret` — 3 giá trị này set qua `dotnet user-secrets`, không hardcode).
