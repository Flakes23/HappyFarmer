# Tài liệu nguồn cho RAG

Thư mục này chứa file gốc (PDF/Word...) lấy từ [Thư viện sách Khuyến nông](https://khuyennongvn.gov.vn/thu-vien-khuyen-nong/thu-vien-sach-kn), dùng làm nguồn cho tính năng RAG của chatbot AI Advisory Service. **Đã có pipeline xử lý** — xem `src/Tools/HappyFarmer.RagIngestor/` (đọc PDF ở đây, chunk, gửi vào AI Advisory Service để embed + lưu Qdrant) và `docs/architecture/data-flows/ai-chatbot-flow.md#rag--tra-cứu-tài-liệu-nông-nghiệp`. Đã chạy ingest lần đầu thành công: 675 chunk từ 9/12 file (xem ghi chú từng file bên dưới).

File gốc (PDF/Word...) **không commit lên git** (xem `.gitignore`) — chỉ giữ ở máy local, vì đây là tài liệu của Trung tâm Khuyến nông Quốc gia, chưa xác nhận rõ điều khoản redistribute qua repo public. Bản đã qua xử lý/biên tập (chunk, tóm tắt) để nạp vào hệ thống sẽ nằm ở nơi khác khi pipeline được xây dựng.

## Danh sách + trạng thái ingest (chạy lần đầu, xem log đầy đủ trong PR/commit liên quan)

File đã đổi tên rõ ràng (không dấu, để tránh vấn đề encoding khi pipeline xử lý). ✅ = vào RAG tốt,
⚠️ = vào được nhưng rất ít nội dung, ❌ = không trích được chữ, bị bỏ qua hoàn toàn (không OCR):

- ✅ Tiếp cận thị trường và phương pháp tiêu thụ nông sản — `Tiep can thi truong va phuong phap tieu thu nong san.pdf` (244 chunk)
- ✅ Hướng dẫn kỹ thuật cho nông dân khảo sát thị trường — `Huong dan ky thuat cho nong dan khao sat thi truong.pdf` (32 chunk)
- ✅ Hướng dẫn kỹ thuật cho nông dân tiếp thị — `Huong dan ky thuat cho nong dan tiep thi.pdf` (35 chunk)
- ✅ Hướng dẫn kỹ thuật cho nông dân GAP/An toàn thực phẩm — `Huong dan ky thuat cho nong dan GAP - An toan thuc pham.pdf` (115 chunk)
- ✅ Canh tác lúa và cà phê giảm nhẹ tác động đến biến đổi khí hậu — `Canh tac lua va ca phe giam nhe tac dong bien doi khi hau.pdf` (207 chunk)
- ✅ Giải pháp kỹ thuật thích ứng thiên tai — Trồng trọt — `Giai phap ky thuat thich ung thien tai - Trong trot.pdf` (191MB, 39 chunk — không scan hoàn toàn như lo ngại ban đầu, vẫn trích được kha khá text)
- ⚠️ Hướng dẫn phục hồi vườn hồ tiêu sau bão lũ — `Huong dan phuc hoi vuon ho tieu sau bao lu.pdf` (chỉ 1 chunk, ~206 ký tự — tờ gấp dạng infographic, phần lớn nội dung nằm trong ảnh)
- ⚠️ Hướng dẫn phục hồi vườn cà phê sau bão lũ — `Huong dan phuc hoi vuon ca phe sau bao lu.pdf` (chỉ 1 chunk, ~210 ký tự — tương tự)
- ⚠️ Hướng dẫn phục hồi vườn sầu riêng sau bão lũ — `Huong dan phuc hoi vuon sau rieng sau bao lu.pdf` (chỉ 1 chunk, ~215 ký tự — tương tự)
- ❌ Biện pháp khắc phục úng ngập lúa sau mưa bão — `Bien phap khac phuc ung ngap lua sau mua bao.pdf` (0 ký tự trích được — bản scan/ảnh)
- ❌ Hướng dẫn xử lý các vùng đất bị vùi lấp sau bão, lũ — `Huong dan xu ly vung dat bi vui lap sau bao lu.pdf` (0 ký tự — bản scan/ảnh)
- ❌ `Cay an qua.pdf` (ngoài danh sách gốc, Cây ăn quả nói chung) — 0 ký tự — bản scan/ảnh

**Muốn khắc phục nhóm ❌/⚠️**: cần thêm bước OCR (vd. Tesseract) vào `HappyFarmer.RagIngestor` trước
khi chunk — chưa làm, ngoài phạm vi hiện tại.
