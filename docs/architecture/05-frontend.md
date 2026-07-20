# Frontend — kiến trúc

React + TypeScript + Vite, TailwindCSS v3, shadcn/ui (Radix), TanStack Query + Zustand, React Hook Form + Zod, react-router-dom, SignalR client (`@microsoft/signalr`). Toàn bộ gọi API qua 1 biến duy nhất `VITE_API_GATEWAY_URL` — không có URL service lẻ nào trong code frontend, mọi request đi qua API Gateway.

## 1. Routing (`frontend/src/routes/router.tsx`)

| Path | Page component (`frontend/src/pages/...`) | Auth guard |
|---|---|---|
| `/` | `HomePage.tsx` | không |
| `/login`, `/register`, `/forgot-password` | `LoginPage`, `RegisterPage`, `ForgotPasswordPage` | không |
| `/profile` | `EditProfilePage.tsx` | `RequireAuth` |
| `/prices`, `/prices/:productId` | `MarketPricePage.tsx`, `ProductPriceDetailPage.tsx` | không |
| `/marketplace` | `MarketplacePage.tsx` | không |
| `/marketplace/listings/:id` | `ListingDetailPage.tsx` | không |
| `/marketplace/buy-requests/:id` | `BuyRequestDetailPage.tsx` | không |
| `/marketplace/new` | `NewListingPage.tsx` | `RequireAuth role="Farmer"` |
| `/marketplace/my-listings` | `MyListingsPage.tsx` | `RequireAuth role="Farmer"` |
| `/marketplace/buy-requests/new` | `NewBuyRequestPage.tsx` | `RequireAuth role="Buyer"` |
| `/marketplace/my-interests` | `MyInterestsPage.tsx` | `RequireAuth` |
| `/marketplace/my-interests/:id` | `InterestThreadPage.tsx` | `RequireAuth` |
| `/tu-van-ai` | `AiAdvisoryPage.tsx` (hub, xem mục 2) | `RequireAuth` |
| `*` | `NotFoundPage.tsx` | không |

Route guard `RequireAuth` (tuỳ chọn `role="Farmer"`/`"Buyer"`) đọc session từ `store/authStore.ts` (Zustand, persist localStorage).

## 2. Pattern hub tab — `/tu-van-ai`

`AiAdvisoryPage.tsx` không phải 3 route riêng mà là 1 trang duy nhất dùng component `Tabs` (shadcn/ui) với 3 tab: `chatbot`, `harvest`, `disease` — render inline `ChatbotPage`/`HarvestPredictionPage`/`DiseaseDetectionPage`. Lý do gộp: cả 3 tính năng đều thuộc AI Advisory Service và dùng chung ngữ cảnh "tư vấn AI", tránh rải route lẻ. Khi thêm tính năng AI mới, ưu tiên thêm tab vào hub này thay vì tạo route `/tu-van-ai/...` mới, trừ khi tính năng đó không còn thuộc nhóm "tư vấn AI".

## 3. Pattern Cloudinary upload dùng chung

Có 3 luồng upload ảnh độc lập, dùng chung 1 helper nhưng mỗi backend service tự có signature endpoint riêng (không dùng chung 1 Cloudinary config service):

| Luồng | Signature endpoint (backend) | Hook gọi (frontend) |
|---|---|---|
| Avatar profile | Auth Service — `GET /api/auth/.../cloudinary-signature` | `useUploadAvatar` (tương tự) |
| Ảnh tin đăng bán | Marketplace Service — `GET /api/marketplace/uploads/signature` | dùng trong `NewListingPage`/form đăng tin |
| Ảnh nhận diện bệnh cây | AI Advisory Service — `GET /api/ai-advisory/disease-detection/cloudinary-signature` | `useUploadDiseasePhoto` (`frontend/src/hooks/mutations/useUploadDiseasePhoto.ts`) |

Cả 3 hook đều gọi chung `frontend/src/lib/cloudinaryUpload.ts` (`uploadImageToCloudinary(file, signature)`) — hàm này POST thẳng file lên `api.cloudinary.com` kèm chữ ký lấy từ backend tương ứng, API Secret không rời server. Mỗi backend service có `CloudinarySignatureService.cs` riêng (cùng pattern, không share code giữa các service — đúng nguyên tắc mỗi microservice tự chủ). Ví dụ UI upload ảnh bệnh cây: `frontend/src/components/disease/DiseasePhotoUploadField.tsx`.

## 4. Chat real-time (SignalR) — Marketplace

- `frontend/src/providers/ChatConnectionProvider.tsx` — mở 1 kết nối SignalR duy nhất tới `${VITE_API_GATEWAY_URL}/api/marketplace/hubs/chat` (bearer token qua `accessTokenFactory`, tự reconnect). Đặt ở tầng cao trong cây component (gần root) để dùng chung cho toàn app, không tạo lại theo từng trang.
- Lắng nghe 2 event: `ReceiveMessage` (tin nhắn mới trong 1 Interest đang mở) và `UnreadCountChanged` (đẩy toàn cục, cập nhật badge số tin chưa đọc bất kể đang ở trang nào — invalidate query `['my-interests', 'unread-count']`).
- `frontend/src/components/marketplace/ChatThread.tsx` — UI thread cho 1 Interest: load lịch sử qua `useMessages(interestId)`, gửi tin qua `useSendMessage`, tự `invoke('JoinConversation'/'LeaveConversation', interestId)` khi vào/rời trang, animate tin nhắn mới (framer-motion), đánh dấu đã đọc qua `useMarkInterestRead`.
- Dùng ở `InterestThreadPage.tsx` (route `/marketplace/my-interests/:id`), kèm `InterestSummary` hiển thị ngữ cảnh tin đăng/yêu cầu mua phía trên thread.
- Chi tiết backend (Hub, DB schema `Messages`) xem [services/marketplace-service.md#chat-real-time-signalr](services/marketplace-service.md#chat-real-time-signalr).

## 5. State & data layer

- `store/` (Zustand) — session/auth state, persist vào localStorage. Hiện chỉ có `authStore.ts`; thêm store mới ở đây khi cần state toàn cục không phải server-state.
- `providers/` — context/provider dùng chung toàn app (hiện chỉ có `ChatConnectionProvider.tsx`).
- `hooks/queries/` vs `hooks/mutations/` (TanStack Query) — tách theo quy ước: đọc dữ liệu (GET) nằm ở `queries/`, ghi dữ liệu (POST/PUT/PATCH/DELETE) nằm ở `mutations/`. Giữ quy ước này khi thêm hook mới, không gộp chung 1 thư mục `hooks/api/`.
- `lib/` — helper thuần không phụ thuộc React: `cloudinaryUpload.ts`, `env.ts` (đọc `VITE_*`), `priceStats.ts`, `relativeTime.ts`, `units.ts`, `utils.ts` (cn/className helper).

## 6. Bảng màu

Xem `CLAUDE.md` ở root repo (bảng token màu "Đồng quê ấm áp", khai báo ở `frontend/src/index.css` + `frontend/tailwind.config.js`).
