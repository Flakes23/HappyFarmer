# Marketplace Service

## Trách nhiệm

Đăng bán nông sản (Farmer), đăng yêu cầu mua (Buyer), tìm kiếm, ghi nhận "quan tâm/liên hệ" giữa hai bên.

**Không** xử lý thanh toán (ngoài phạm vi MVP) — chỉ kết nối thông tin liên hệ.

## API chính

| Method | Path | Mô tả |
|---|---|---|
| POST | `/api/marketplace/listings` | Tạo tin đăng bán (Farmer) |
| GET | `/api/marketplace/listings?productId=&regionId=&status=` | Tìm kiếm tin đăng |
| GET | `/api/marketplace/listings/{id}` | Chi tiết tin đăng |
| PUT | `/api/marketplace/listings/{id}` | Cập nhật tin (owner) |
| PATCH | `/api/marketplace/listings/{id}/close` | Đóng tin — đã bán/hết hàng (owner) |
| POST | `/api/marketplace/buy-requests` | Đăng yêu cầu mua (Buyer) |
| GET | `/api/marketplace/buy-requests` | Tìm kiếm yêu cầu mua |
| POST | `/api/marketplace/listings/{id}/contact` | Bày tỏ quan tâm/liên hệ (Buyer) → tạo Interest, trigger thông báo |
| GET | `/api/marketplace/my-listings` | Quản lý tin đăng của tôi |
| GET | `/api/marketplace/my-interests` | Quản lý liên hệ của tôi |

## DB schema (MarketplaceDb)

```
Listings
  Id            (PK)
  FarmerId
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
