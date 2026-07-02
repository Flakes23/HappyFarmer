# Market Price Service

## Trách nhiệm

Danh mục nông sản, khu vực/chợ, giá hiện tại & lịch sử. Nhận dữ liệu từ crawler (chợ đầu mối) và từ cộng đồng (có duyệt). Publish sự kiện khi giá thay đổi.

**Không** xử lý AI, **không** xử lý gửi thông báo (chỉ publish event, Notification Service lo phần gửi).

## API chính

| Method | Path | Mô tả |
|---|---|---|
| GET | `/api/market-price/products` | Danh sách nông sản |
| GET | `/api/market-price/regions` | Danh sách khu vực/chợ |
| GET | `/api/market-price/prices?productId=&regionId=&date=` | Giá hiện tại/theo ngày |
| GET | `/api/market-price/prices/{productId}/history?regionId=&from=&to=` | Lịch sử giá (cho biểu đồ) |
| GET | `/api/market-price/prices/trending` | Top biến động giá |
| POST | `/api/market-price/prices` | Nhập giá cộng đồng (Farmer) → status=Pending |
| POST | `/api/market-price/prices/{id}/approve` | Duyệt giá cộng đồng (Admin) |
| POST | `/api/market-price/internal/crawl-ingest` | Nhận dữ liệu từ crawler job/cronjob (internal API key) |

## DB schema (MarketPriceDb)

```
Products
  Id            (PK)
  NameVi
  Category
  Unit
  ImageUrl

Regions
  Id            (PK)
  ProvinceName
  MarketName
  Lat
  Lon

PriceEntries               # append-only
  Id              (PK)
  ProductId       (FK -> Products.Id)
  RegionId        (FK -> Regions.Id)
  Price
  Source          (Crawled | Community | Admin)
  SubmittedByUserId
  Status          (Pending | Approved | Rejected)
  EffectiveDate
  CreatedAt
```

"Giá hiện tại" của một `ProductId + RegionId` = bản ghi `Approved` mới nhất (không update tại chỗ, luôn insert bản ghi mới).

## Kafka

Publish `market-price.price-changed.v1` khi một `PriceEntries` mới chuyển sang `Approved`:

```json
{
  "eventId": "...",
  "productId": 1,
  "productName": "Cà chua",
  "regionId": 5,
  "regionName": "Chợ đầu mối Đà Lạt",
  "oldPrice": 12000,
  "newPrice": 15000,
  "changePercent": 25.0,
  "effectiveDate": "2026-07-02",
  "occurredAt": "..."
}
```

Consumer: xem [Notification Service](notification-service.md) và luồng chi tiết tại [../data-flows/market-price-to-notification.md](../data-flows/market-price-to-notification.md).

## Redis

| Key | Mục đích | TTL |
|---|---|---|
| `market:price:current:{productId}:{regionId}` | Cache JSON giá hiện tại, tránh query DB liên tục | 10 phút |
| `market:price:trending` | Cache danh sách trending | 15 phút |
