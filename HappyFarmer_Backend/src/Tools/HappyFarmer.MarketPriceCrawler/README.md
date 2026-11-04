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

## Lên lịch chạy tự động

**Trên production (VPS)**: đã tự động qua GitHub Actions — `.github/workflows/crawl-daily.yml`
chạy theo lịch mỗi ngày (mặc định 05:30 giờ Việt Nam), SSH vào VPS và kích hoạt service
`market-price-crawler` (`profiles: ["cron"]` trong `docker-compose.prod.yml`, không chạy cùng
`up -d` bình thường). Image được build/push GHCR bởi `deploy.yml` mỗi lần push `master`. Xem
chi tiết ở `docs/architecture/03-infrastructure-deployment.md#5-nạp-dữ-liệu-market-price--lần-đầu-thủ-công--hằng-ngày-tự-động`.
Muốn chạy ngay ngoài lịch: tab **Actions** trên GitHub → chọn workflow "Daily market price crawl"
→ **Run workflow**.

**Trên máy Windows cá nhân** (chạy dev/thử nghiệm, không phải cách dùng ở production):

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

## Tắt/bật từng nguồn khi thử

Công tắc `Crawler:EnableThucPhamNhanh` (mặc định `true`) trong `appsettings.Local.json`: đặt `false` để chỉ chạy các nguồn
khác (vd. BHX) mà không crawl lại thucphamnhanh.com; xoá dòng hoặc đặt `true` để bật lại.

## Nguồn Bách Hóa Xanh (tự gọi API, chỉ chạy trên máy bạn)

`BachHoaXanhApiScraper` gọi `POST /gw/Category/AjaxProduct` của Bách Hóa Xanh cho từng cửa hàng đã cấu hình
(phân trang 6 sản phẩm/trang tới hết `data.total`; tối đa 10 cửa hàng, nghỉ 3 giây mỗi lần gọi, gặp lỗi HTTP là
dừng). Đây là API phía sau website BHX, **không công bố cho bên thứ ba** — BHX có thể đổi/chặn bất cứ lúc nào, và
bạn tự chịu trách nhiệm về việc dùng nó. Nguồn này mặc định **tắt**: chỉ bật khi có file cấu hình dưới đây, nên
chạy trên VPS/CI (GitHub Actions) không bị ảnh hưởng.

Tạo file `appsettings.Local.json` cạnh `appsettings.json` (đã được ignore, **không commit**):

```json
{
  "BachHoaXanh": {
    "ApiKey": "<giá trị header xapikey lấy từ yêu cầu AjaxProduct trong DevTools>",
    "ReverseHost": "http://bhxapi.live",
    "CategoryIds": [8785],
    "UseListPrice": false,
    "PriorityProductIdsByCategory": { "8785": "" },
    "Stores": [
      { "Province": "TP. Hồ Chí Minh", "ProvinceId": 1027, "StoreId": 2546 }
    ]
  }
}
```

- `ProvinceId`/`StoreId`: lấy từ body của yêu cầu `AjaxProduct` trong DevTools sau khi đổi địa chỉ giao hàng sang tỉnh đó.
- `CategoryIds`: 8785 = nhóm "Củ, quả" (trang `/cu`); thêm `CategoryId` của nhóm khác nếu muốn.
- `PriorityProductIdsByCategory`: với mỗi `CategoryId`, một chuỗi mã sản phẩm cách nhau bằng dấu phẩy, copy từ trường
  `PriorityProductIds` trong body yêu cầu `AjaxProduct` của **đúng nhóm hàng đó** (DevTools → Payload). Mỗi nhóm có danh sách
  riêng, không dùng chung được. Để trống/thiếu thì API có thể bỏ sót một số sản phẩm (vd. báo `total` 43 nhưng chỉ trả 37).
- `SubCategoryByCategory`: nhóm con (SubCategory) gán cho sản phẩm **mới** của từng `CategoryId`, vd. `{ "8820": "Dạng lá", "8788": "Trái cây" }`
  (mặc định "Rau, củ, quả khác"). Sản phẩm đã tồn tại cùng tên giữ nhóm con cũ — server chỉ gán nhóm con khi tạo mới sản phẩm.
- `UseListPrice`: `false` = giá bán thực tế (đã trừ khuyến mãi); `true` = giá niêm yết gốc (`sysPrice`).
- Đơn vị lưu: mặt hàng bán theo kg → "500g"/"1kg"/"1.5kg" theo `netUnitValue`; mặt hàng bán theo gói (Túi/Vỉ/Combo)
  → khối lượng thật của gói lấy từ `canonical` (vd "100g"), để giá so sánh được với nguồn khác.
- Giá là giá bán lẻ siêu thị theo từng cửa hàng, không phải giá thị trường.

### Chạy ở đâu

**BHX chặn IP của VPS Azure** (đã thử 04/11/2026: `curl` từ VPS tới `www`/`api.bachhoaxanh.com` bị đóng kết nối), nên nguồn này
**chạy từ máy bạn**: `dotnet run` trong thư mục crawler (với `appsettings.Local.json` như trên), dữ liệu vẫn đẩy lên VPS qua
`Api:BaseUrl`. Chạy BHX khoảng mỗi ngày hoặc vài ngày một lần. Workflow `crawl-bhx.yml` chỉ còn chạy tay; mẫu file cấu hình cho
VPS ở `crawler-config/appsettings.Local.example.json` (chưa dùng được vì lý do trên). Xem thêm mục 5 của
`docs/architecture/03-infrastructure-deployment.md`.
