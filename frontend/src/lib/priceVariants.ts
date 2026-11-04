import type { PriceVariant } from '@/api/types'
import { sourceLabel } from '@/lib/regionLabels'

export interface VariantOption {
  key: string
  variant: PriceVariant
}

/** "500g" → 500, "1kg" → 1000, "1.5kg" → 1500; quy cách không phải khối lượng (vd. "3 trái", "Kg") → null. */
function toGrams(unit: string | null): number | null {
  const match = unit?.trim().toLowerCase().match(/^(\d+(?:[.,]\d+)?)\s*(kg|g)$/)
  if (!match) return null
  const value = Number(match[1].replace(',', '.'))
  return match[2] === 'kg' ? value * 1000 : value
}

/**
 * Các lựa chọn quy cách của một sản phẩm: mỗi (quy cách, nguồn) một lựa chọn, giữ giá mới nhất nếu trùng; xếp theo khối
 * lượng tăng dần (quy cách không phải khối lượng xếp cuối), cùng khối lượng thì theo tên nguồn.
 */
export function buildVariantOptions(variants: PriceVariant[]): VariantOption[] {
  const latest = new Map<string, PriceVariant>()
  for (const v of variants) {
    const key = `${v.unit ?? ''}|${v.marketName}`
    const current = latest.get(key)
    if (!current || v.effectiveDate > current.effectiveDate) latest.set(key, v)
  }

  return Array.from(latest, ([key, variant]) => ({ key, variant })).sort((a, b) => {
    const ga = toGrams(a.variant.unit)
    const gb = toGrams(b.variant.unit)
    if (ga !== gb) return (ga ?? Number.MAX_SAFE_INTEGER) - (gb ?? Number.MAX_SAFE_INTEGER)
    return sourceLabel(a.variant.marketName).localeCompare(sourceLabel(b.variant.marketName), 'vi')
  })
}

/** Nhãn lựa chọn: "500g" — kèm tên nguồn ("500g · Bách Hóa Xanh") khi sản phẩm có giá từ nhiều nguồn. */
export function variantLabel(option: VariantOption, showSource: boolean): string {
  const unit = option.variant.unit ?? 'Mặc định'
  return showSource ? `${unit} · ${sourceLabel(option.variant.marketName)}` : unit
}
