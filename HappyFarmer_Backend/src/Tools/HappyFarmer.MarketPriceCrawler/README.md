# HappyFarmer Market Price Crawler

Console app nhỏ crawl giá nông sản từ 3 nguồn công khai, ghép nhóm theo (Sản phẩm, Khu vực)
và trung bình cộng nếu nhiều nguồn cùng báo giá cho đúng 1 khu vực, rồi gửi vào
`POST /api/market-price/internal/crawl-ingest` của `HappyFarmer.MarketPriceService.Api`.

## Nguồn dữ liệu

| Nguồn | Vùng gán | Ghi chú |
|---|---|---|
| `giaca.nsvl.com.vn` | Vĩnh Long / Chợ Vĩnh Long | Trung tâm Khuyến nông Vĩnh Long (Sở NN&PTNT) |
| `thucphamdongxanh.com` | TP. Hồ Chí Minh / Chợ đầu mối (tổng hợp) | Giá chợ đầu mối khu vực TP.HCM, không gắn 1 chợ cụ thể |
| `banggianongsan.com` | Theo tỉnh ngay trong tên hàng (Gia Lai, Đồng Nai, Đắk Nông...) | Hồ tiêu, cà phê |

Danh mục sản phẩm/khu vực crawler dùng được seed sẵn trong migration
`SeedCrawlerCatalog` của `HappyFarmer.MarketPriceService.Api`. Muốn crawl thêm mặt hàng/vùng
mới: thêm `HasData` trong `MarketPriceDbContext` + thêm entry vào file mapping tương ứng
trong `Mapping/`.

## Chạy thử cục bộ

```bash
# 1. Hạ tầng
docker compose up -d          # ở thư mục gốc repo

# 2. Áp migration (bao gồm seed catalog)
cd HappyFarmer_Backend/src/Services/MarketPriceService/HappyFarmer.MarketPriceService.Api
dotnet ef database update

# 3. Set API key nội bộ — PHẢI giống nhau ở cả 2 project
dotnet user-secrets set Internal:ApiKey "<giá trị bất kỳ>"
cd ../../../Tools/HappyFarmer.MarketPriceCrawler
dotnet user-secrets set Internal:ApiKey "<giá trị giống bước trên>"

# 4. Chạy API (terminal riêng), rồi chạy crawler
dotnet run --project ../../Services/MarketPriceService/HappyFarmer.MarketPriceService.Api
dotnet run
```

Kiểm tra kết quả: `GET http://localhost:5262/api/market-price/prices/{productId}/history?regionId=...`
hoặc xem log console của crawler (in rõ nguồn nào góp giá bao nhiêu → giá cuối cùng).

## Lên lịch chạy tự động (Windows Task Scheduler)

1. `dotnet publish -c Release -o publish` tại thư mục project này.
2. Task Scheduler → Create Task:
   - **Trigger**: Daily, ~05:30 (sau khung giờ chốt giá chợ đầu mối 2–5h sáng).
   - **Action**: chạy `publish\HappyFarmer.MarketPriceCrawler.exe`, "Start in" = thư mục `publish`.
   - Log ra file: đổi Action thành chạy qua `cmd.exe /c "HappyFarmer.MarketPriceCrawler.exe >> logs\crawl.log 2>&1"`.
3. `appsettings.json` + `Mapping/*.json` đã được copy vào thư mục `publish` cùng exe (cấu hình
   `CopyToOutputDirectory` trong `.csproj`) — không cần copy tay.
4. `Internal:ApiKey` đọc qua user-secrets chỉ hoạt động khi chạy bằng `dotnet run`/`dotnet exec`
   trên máy dev đã `dotnet user-secrets set`. Khi publish exe độc lập chạy qua Task Scheduler,
   secrets vẫn nằm trong file JSON của user-secrets ở `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`
   nên vẫn đọc được miễn chạy cùng máy đã set — nếu deploy sang máy khác, đặt `Internal:ApiKey`
   qua biến môi trường `Internal__ApiKey` thay vì user-secrets.

## Giới hạn đã biết (ghi rõ trong báo cáo đồ án)

- Danh mục sản phẩm/khu vực là tập con nhỏ, chọn thủ công dựa trên dữ liệu thật khảo sát được
  từ 3 nguồn tại thời điểm viết — không phải toàn bộ nông sản Việt Nam.
- `thucphamdongxanh.com`/`banggianongsan.com` là các trang thương mại/tổng hợp tư nhân, không
  phải cơ quan nhà nước — độ tin cậy thấp hơn `giaca.nsvl.com.vn` (Sở NN&PTNT Vĩnh Long).
- Cấu trúc HTML của cả 3 trang có thể đổi bất kỳ lúc nào (không có API chính thức) — scraper
  dựa trên cấu trúc `<table>` khảo sát được, cần kiểm tra lại nếu trang nguồn đổi giao diện.
- `giaca.nsvl.com.vn` chỉ hiển thị **ngẫu nhiên một phần** trong số các nhóm hàng (rau củ, trái
  cây, thủy sản, gia súc-gia cầm...) mỗi lần tải trang — đã xác nhận qua nhiều lần fetch trực
  tiếp (kể cả bằng `curl`, không liên quan đến code crawler). Nhóm "Rau, Củ" không phải lúc nào
  cũng có mặt. `NsvlScraper` đã có retry (tối đa 5 lần, cách nhau 500ms) nhưng nếu chu kỳ xoay
  vòng của trang dài hơn khoảng retry, lượt chạy đó sẽ không lấy được gì từ nguồn này — hệ thống
  vẫn hoạt động bình thường với 2 nguồn còn lại, không phải lỗi crash.
