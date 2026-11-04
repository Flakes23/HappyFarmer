import type { RegionResponse } from '@/api/types'

/**
 * Tên "nơi tham khảo" hiển thị cho người dùng. Crawler lưu thucphamnhanh.com dưới `MarketName = "Tổng hợp"`
 * (giá tổng hợp, không gắn 1 chợ cụ thể) — ghi thẳng "Tổng hợp" thì không biết lấy từ đâu, nên đổi sang tên nguồn.
 * Các nguồn khác (vd. "Bách Hóa Xanh") đã mang đúng tên nguồn trong `MarketName`.
 */
export function sourceLabel(marketName: string): string {
  return marketName === 'Tổng hợp' ? 'Thực Phẩm Nhanh' : marketName
}

/** Nhãn đầy đủ cho ô chọn khu vực: "Hà Nội – Bách Hóa Xanh" (cùng 1 tỉnh có thể có nhiều nguồn giá). */
export function regionLabel(region: Pick<RegionResponse, 'provinceName' | 'marketName'>): string {
  return `${region.provinceName} – ${sourceLabel(region.marketName)}`
}
