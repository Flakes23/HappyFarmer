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
      <span className="inline-flex items-center whitespace-nowrap rounded-md border border-accent/30 bg-accent/15 px-2 py-0.5 text-xs font-medium text-accent">
        Cộng đồng đóng góp
      </span>
    )
  }

  return (
    <span className="inline-flex items-center gap-1 rounded-md border border-primary/20 bg-primary/15 px-2 py-0.5 text-xs font-medium text-primary">
      <BadgeCheck className="h-3.5 w-3.5 fill-primary text-surface" />
      HappyFarmer
    </span>
  )
}
