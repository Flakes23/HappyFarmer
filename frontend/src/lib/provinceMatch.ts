/**
 * Bỏ dấu, chữ thường, bỏ tiền tố "TP."/"Thành phố"/"Tỉnh" để so khớp tên tỉnh giữa các nguồn dữ liệu. Tên tỉnh trong
 * dữ liệu giá do crawler đặt (cấu hình `Province`) và phải đúng tên trong danh sách tỉnh của Auth Service — xem README
 * của crawler; hàm này chỉ chịu được khác biệt nhỏ về dấu/tiền tố, không nối được 2 tên khác hẳn nhau.
 */
function normalizeProvinceName(name: string): string {
  return name
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/gi, 'd')
    .toLowerCase()
    .replace(/[.,]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/^(tp|thanh pho|tinh) /, '')
}

/** Tìm trong `available` tỉnh trùng với `target` (so khớp không phân biệt dấu/tiền tố); trả undefined nếu không có. */
export function findMatchingProvince(target: string | undefined, available: string[]): string | undefined {
  if (!target) return undefined
  const key = normalizeProvinceName(target)
  return available.find((name) => normalizeProvinceName(name) === key)
}
