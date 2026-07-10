import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { computePriceStats } from '@/lib/priceStats'
import type { PriceHistoryPoint } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

interface PriceStatCardsProps {
  data: PriceHistoryPoint[] | undefined
  isLoading: boolean
}

export function PriceStatCards({ data, isLoading }: PriceStatCardsProps) {
  if (isLoading) {
    return (
      <div className="grid gap-3 sm:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-[76px] w-full" />
        ))}
      </div>
    )
  }

  const stats = computePriceStats(data)
  if (!stats) return null

  const items = [
    { label: 'Giá trung bình', value: stats.avg },
    { label: 'Giá cao nhất', value: stats.high },
    { label: 'Giá thấp nhất', value: stats.low },
  ]

  return (
    <div className="grid gap-3 sm:grid-cols-3">
      {items.map((item) => (
        <Card key={item.label}>
          <CardContent className="p-4">
            <p className="text-sm text-text-muted">{item.label}</p>
            <p className="text-xl font-semibold text-text">{currencyFormatter.format(item.value)}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
