# Nhật ký ôn M2 — Market Price Service

> File học cá nhân, gitignored. Tài liệu chính thức: `docs/architecture/services/market-price-service.md`.

## Lịch crawler — 5h30 sáng, vì sao đúng giờ đó

`.github/workflows/crawl-daily.yml`: cron `22:30 UTC = 05:30 giờ VN`. Lý do (ghi trong comment file,
dễ quên): **sau khung giờ chốt giá chợ đầu mối (2-5h sáng)** — đợi giá thật "ổn định" trong ngày rồi
mới crawl, không phải chọn giờ ngẫu nhiên cho "sáng sớm nghe hợp lý".

Cơ chế chạy: GitHub Actions (cron) → SSH vào VPS → chạy container `market-price-crawler` qua
`docker-compose.prod.yml --profile cron run --rm` — là **job chạy 1 lần rồi thoát**, không phải
background service sống 24/7 (khác với 5 service chính luôn chạy qua `docker compose up -d`).

## Xử lý lỗi & trùng dữ liệu khi crawl

- Mỗi scraper (`IPriceSourceScraper`) có `try/catch` riêng trong `Program.cs` — 1 nguồn lỗi không
  làm sập cả lượt crawl (thiết kế cho tương lai nhiều nguồn, dù hiện tại chỉ có `ThucPhamNhanhScraper`
  — các nguồn khác như thucphamdongxanh.com/Bách hóa xanh/WinMart đã bị gỡ vì không ổn định).
- Trùng **trong cùng 1 lượt crawl** (trang bị tải lại giữa chừng do lỗi mạng): dedupe theo
  `(ProductName, Unit)` trước khi gửi API (`DistinctBy`, `Program.cs`).
- Bug thật đã gặp: sản phẩm hết hàng hiển thị "Liên hệ 090 1805550" thay vì giá số → đọc nhầm
  thành `901.805.550đ`. Fix: chỉ tin giá lấy từ đúng span CSS `woocommerce-Price-amount`, không có
  thì bỏ qua sản phẩm đó lần chạy này (`ThucPhamNhanhScraper.cs`).
- Server (`InternalController.CrawlIngest`) tự **find-or-create** Category→SubCategory→Product→
  Region theo tên — không seed catalog cố định. "Trùng" chỉ có ý nghĩa ở tầng `PriceEntries`: mỗi
  lần crawl luôn **insert** bản ghi giá mới (append-only), không update tại chỗ — giá hiện tại =
  bản ghi `Approved` mới nhất theo `(ProductId, RegionId, Unit)`.

## Giá cộng đồng (Community) — backend xong, frontend chưa có UI

Nhớ nhầm ban đầu: "chưa cho farmer nhập giá" — **không chính xác ở tầng backend**, dễ bị hỏi vặn.

- Backend **đã hoàn chỉnh**: `PricesController.SubmitPrice` (`POST /api/market-price/prices`,
  `[Authorize(Roles = "Farmer")]`) tạo `PriceEntry` với `Source=Community`, `Status=Pending`.
  `PricesController.ApprovePrice` (`POST /api/market-price/prices/{id}/approve`,
  `[Authorize(Roles = "Admin")]`) đổi status sang `Approved` + invalidate cache Redis.
- Frontend **chưa có** — check `frontend/src/api/marketPriceApi.ts` chỉ có các hàm GET (giá/lịch
  sử/trending/danh mục), không có hàm gọi 2 endpoint trên. Farmer chưa nhập được giá **qua giao
  diện thật**, nhưng đây là gap **frontend**, không phải backend chưa làm.
- Cách nói chính xác khi trình bày: "API + DB đã xong đầy đủ (submit + approve + phân quyền), còn
  thiếu form UI ở frontend để farmer/admin thao tác qua giao diện" — không nói chung chung "chưa
  làm giá cộng đồng".

## Ghi nhớ thêm

- `PriceEntries` là bảng append-only, KHÔNG update tại chỗ — mọi thay đổi giá đều insert dòng mới.
- Nhóm "giá hiện tại" theo `(ProductId, RegionId, Unit)` chứ không chỉ `(ProductId, RegionId)` —
  vì 1 sản phẩm có thể bán nhiều quy cách khối lượng khác nhau với giá khác hẳn (vd 150g vs 1kg).
