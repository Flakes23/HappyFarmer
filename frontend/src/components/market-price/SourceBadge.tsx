import { BadgeCheck } from 'lucide-react'
import type { PriceSource } from '@/api/types'

interface SourceBadgeProps {
  source: PriceSource
}

/**
 * Crawled (crawler tự động) và Admin (nhập tay đã xác nhận) đều hiển thị chung thương hiệu
 * "HappyFarmer" kèm tích xanh — phân biệt với giá Community do người dùng tự gửi, chưa qua
 * hệ thống xác nhận.
 */
export function SourceBadge({ source }: SourceBadgeProps) {
  if (source === 'Community') {
    return (
      <span className="inline-flex items-center rounded-md border border-border bg-secondary px-2 py-0.5 text-xs font-medium text-text-muted">
        Cộng đồng đóng góp
      </span>
    )
  }

  return (
    <span className="inline-flex items-center gap-1 rounded-md bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
      <BadgeCheck className="h-3.5 w-3.5 fill-primary text-surface" />
      HappyFarmer
    </span>
  )
}
