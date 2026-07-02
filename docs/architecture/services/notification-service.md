# Notification Service

## Trách nhiệm

Subscribe các sự kiện Kafka từ Market Price Service và Marketplace Service, lưu Notification, quản lý danh sách theo dõi giá (`PriceWatchSubscriptions` — bản sao cục bộ, **không** gọi ngược Market Price Service). Gửi thông báo qua kênh in-app (bắt buộc MVP), email (Phase 4), push notification (stretch, Phase 6).

## API chính

| Method | Path | Mô tả |
|---|---|---|
| GET | `/api/notification/notifications?unread=` | Danh sách thông báo của user |
| PATCH | `/api/notification/notifications/{id}/read` | Đánh dấu đã đọc |
| POST | `/api/notification/watchlist` | Đăng ký theo dõi giá theo product + region |
| DELETE | `/api/notification/watchlist/{id}` | Huỷ theo dõi |
| GET | `/api/notification/preferences` | Xem cấu hình kênh nhận |
| POST | `/api/notification/preferences` | Cập nhật cấu hình kênh nhận (email/in-app/push) |

## DB schema (NotificationDb)

```
Notifications
  Id                (PK)
  UserId
  Type              (PriceChange | NewInterest | AdvisoryReady | System)
  Title
  Body
  RelatedEntityId
  Channel           (InApp | Email | Push)
  Status            (Unread | Read | Sent | Failed)
  CreatedAt

PriceWatchSubscriptions
  Id            (PK)
  UserId
  ProductId
  RegionId
  CreatedAt

NotificationPreferences
  UserId        (PK)
  EmailEnabled
  PushEnabled
  InAppEnabled
```

## Kafka — subscribe

| Topic | Consumer group | Xử lý |
|---|---|---|
| `market-price.price-changed.v1` | `notification-service-group` | Tra `PriceWatchSubscriptions` khớp `productId` + `regionId` → tạo Notification cho từng user theo dõi |
| `marketplace.new-interest.v1` | `notification-service-group` | Tạo Notification cho `targetUserId` (farmer) |

Chi tiết luồng end-to-end: [../data-flows/market-price-to-notification.md](../data-flows/market-price-to-notification.md).

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `notification:unread-count:{userId}` | Cache số lượng chưa đọc (cache-aside, invalidate khi có Notification mới hoặc đánh dấu đã đọc) | 60 giây (fallback) |
