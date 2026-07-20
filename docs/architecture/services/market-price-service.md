# Market Price Service

## Trách nhiệm

Danh mục nông sản (Category → SubCategory → Product), khu vực/nguồn giá, giá hiện tại & lịch sử. Nhận dữ liệu từ crawler (`src/Tools/HappyFarmer.MarketPriceCrawler`, xem mục [Crawler](#crawler) bên dưới) và từ cộng đồng (có duyệt). Publish sự kiện khi giá thay đổi.

**Không** xử lý AI, **không** xử lý gửi thông báo (chỉ publish event, Notification Service lo phần gửi).

## API chính

| Method | Path | Mô tả |
|---|---|---|
| GET | `/api/market-price/products` | Toàn bộ danh sách nông sản (kèm `subCategoryId`/`subCategoryName`/`categoryId`/`categoryName`), không phân trang |
| GET | `/api/market-price/products/by-ids?ids=1,2,3` | Tra tên/đơn vị theo đúng vài Id cụ thể — dùng khi caller (vd. chatbot AI Advisory resolve tên hiển thị cho vài listing/giá vừa tìm được) không muốn tải toàn bộ catalog rồi tự lọc |
| GET | `/api/market-price/regions` | Danh sách khu vực/nguồn giá |
| GET | `/api/market-price/regions/by-ids?ids=1,2,3` | Tương tự `products/by-ids` nhưng cho khu vực |
| GET | `/api/market-price/categories` | Danh sách Category cha (hiện chỉ có 1: "Rau, củ, quả") |
| GET | `/api/market-price/categories/{categoryId}/sub-categories` | Danh sách SubCategory con của 1 Category (Dạng lá, Dạng củ, Dạng quả (trái), Dạng nấm, Dạng hạt, Rau gia vị tây, Trái cây, Rau củ quả khác) |
| GET | `/api/market-price/prices?productId=&regionId=&date=&search=&categoryId=&subCategoryId=&page=&pageSize=` | Giá hiện tại/theo ngày, có lọc theo tên (search, khớp `Contains`)/Category/SubCategory, **trả về `PagedResult<PriceResponse>`** (`page` mặc định 1, `pageSize` mặc định 20, tối đa 100) |
| GET | `/api/market-price/prices/{productId}/history?regionId=&from=&to=` | Lịch sử giá (cho biểu đồ) — trả nguyên danh sách, không phân trang, mỗi điểm kèm `variant` |
| GET | `/api/market-price/prices/trending` | Top biến động giá (kèm `variant`) |
| POST | `/api/market-price/prices` | Nhập giá cộng đồng (Farmer) → status=Pending |
| POST | `/api/market-price/prices/{id}/approve` | Duyệt giá cộng đồng (Admin) |
| POST | `/api/market-price/internal/crawl-ingest` | Nhận dữ liệu từ crawler (internal API key) — xem [Crawler](#crawler) |

## DB schema (MarketPriceDb)

```
Categories
  Id            (PK)
  Name                          # unique

SubCategories
  Id            (PK)
  CategoryId    (FK -> Categories.Id)
  Name                          # unique theo (CategoryId, Name)

Products
  Id            (PK)
  NameVi                        # unique — crawler find-or-create theo đúng cột này
  SubCategoryId (FK -> SubCategories.Id)
  Unit                          # đơn vị/khối lượng thật (vd "1kg", "150g"), KHÔNG phải đơn vị bán chung chung
  ImageUrl

Regions
  Id            (PK)
  ProvinceName
  MarketName                    # unique theo (ProvinceName, MarketName) — mỗi nguồn crawl khác nhau map ra 1 Region riêng
  Lat
  Lon

PriceEntries               # append-only
  Id              (PK)
  ProductId       (FK -> Products.Id)
  RegionId        (FK -> Regions.Id)
  Price
  Unit            # đơn vị/khối lượng thật của ĐÚNG bản ghi giá này (vd "150g" vs "1kg" của cùng 1 Product) — null nếu Source=Admin/Community
  Source          (Crawled | Community | Admin)
  SubmittedByUserId
  Status          (Pending | Approved | Rejected)
  EffectiveDate
  CreatedAt
```

"Giá hiện tại" của một `(ProductId, RegionId, Unit)` = bản ghi `Approved` mới nhất (không update tại chỗ, luôn insert bản ghi mới) — **nhóm theo cả `Unit`**, không chỉ `ProductId + RegionId`, vì 1 sản phẩm có thể bán nhiều quy cách (khối lượng) khác nhau với giá khác hẳn nhau (vd "Đậu đỏ" quy cách 150g và 1kg là 2 `PriceEntries` riêng cùng chung 1 `Product`). Lưu ý `PriceEntries.Unit` khác `Products.Unit`: `Products.Unit` chỉ là đơn vị mặc định/gần nhất của Product (có thể "nhảy" giữa các quy cách nếu Product có nhiều `PriceEntries.Unit` khác nhau), còn `PriceEntries.Unit` mới là đơn vị chính xác của từng dòng giá.

## Crawler

`src/Tools/HappyFarmer.MarketPriceCrawler` (console app riêng, chạy thủ công/cronjob) — hiện chỉ crawl **đúng 1 nguồn**: `thucphamnhanh.com/rau-cu-qua/` (HTML tĩnh, WooCommerce, phân trang thật ~7 trang/303 sản phẩm). Các nguồn trước đó (thucphamdongxanh.com, Bách hóa xanh, WinMart) đã bị gỡ bỏ vì không ổn định (API nội bộ không công khai, dễ vỡ) hoặc kết quả xấu (search fuzzy trả giá không nhất quán).

- **Không seed catalog cố định** — crawler gửi thẳng tên thô (`CategoryName`/`SubCategoryName`/`ProductName`/`ProductUnit`/`RegionProvinceName`/`RegionMarketName`) lên `POST .../internal/crawl-ingest`, server tự **find-or-create** theo đúng thứ tự Category → SubCategory → Product → Region (khớp theo tên, xem `InternalController.CrawlIngest`) rồi mới insert `PriceEntry`. Vì vậy danh mục sản phẩm hoàn toàn do dữ liệu crawl quyết định, không có migration seed Product/Region nào nữa. `CrawlIngestRequest.ProductUnit` (đơn vị mặc định của Product) và `CrawlIngestRequest.Unit` (đơn vị chính xác của dòng giá này, lưu vào `PriceEntry.Unit`) là 2 field khác nhau, đừng nhầm.
- **Tách khối lượng khỏi tên**: tên gốc trên trang thường có hậu tố khối lượng sau dấu `–` (vd `"Nấm Hải Sản – Gói 150g"`) — crawler tách phần này ra, đưa vào `PriceEntry.Unit` (vd `"150g"`), giữ tên sản phẩm sạch (`"Nấm Hải Sản"`). Nhờ vậy nhiều quy cách của cùng 1 mặt hàng vẫn chung 1 `Product`, không bị trùng `NameVi`.
- **Bẫy đã gặp, đã fix trong code**: sản phẩm hết hàng lâu ngày trang hiện chữ "Liên hệ 090 1805550" thay vì giá số — nếu đọc nhầm số điện thoại thành giá sẽ ra giá sai (đã thực sự xảy ra: "901.805.550đ"). Crawler chỉ tin giá lấy từ đúng span `woocommerce-Price-amount`, không có thì bỏ qua sản phẩm đó lần chạy này.

## Kafka

`market-price.price-changed.v1` (**chưa wiring** — `PricesController.ApprovePrice` hiện chỉ đổi status + invalidate cache Redis, chưa có `AddKafkaProducer`/publish nào trong code — để dành Phase 4 cùng lúc với Notification Service). Schema dự kiến khi làm:

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
