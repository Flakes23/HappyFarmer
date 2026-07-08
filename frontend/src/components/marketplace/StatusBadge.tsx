import { Badge } from '@/components/ui/badge'
import type { BuyRequestStatus, InterestStatus, ListingStatus } from '@/api/types'

type Status = ListingStatus | BuyRequestStatus | InterestStatus

const LABELS: Record<Status, string> = {
  Active: 'Đang hoạt động',
  Sold: 'Đã bán',
  Closed: 'Đã đóng',
  Expired: 'Hết hạn',
  Pending: 'Chờ phản hồi',
  Responded: 'Đã phản hồi',
}

const VARIANTS: Record<Status, 'default' | 'secondary' | 'outline'> = {
  Active: 'default',
  Sold: 'secondary',
  Closed: 'outline',
  Expired: 'outline',
  Pending: 'secondary',
  Responded: 'default',
}

export function StatusBadge({ status }: { status: Status }) {
  return <Badge variant={VARIANTS[status]}>{LABELS[status]}</Badge>
}
