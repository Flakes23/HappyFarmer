import { Link } from 'react-router-dom'
import { TrendingDown, TrendingUp } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import type { PriceCard as PriceCardData } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function PriceCard({ card }: { card: PriceCardData }) {
  const changePercent = card.changePercent
  const isUp = changePercent !== null && changePercent > 0
  const isDown = changePercent !== null && changePercent < 0

  return (
    <Card className="w-full max-w-xs">
      <CardContent className="space-y-1 p-3">
        <p className="text-body-sm font-medium text-text">{card.productName}</p>
        <p className="text-xs text-text-muted">{card.regionName}</p>
        <div className="flex items-baseline justify-between gap-2 pt-1">
          <p className="font-semibold text-primary">
            {currencyFormatter.format(card.currentPrice)}
            {card.unit ? ` / ${card.unit}` : ''}
          </p>
          {changePercent !== null ? (
            <span
              className={`flex items-center gap-0.5 text-xs font-semibold ${
                isUp ? 'text-accent' : isDown ? 'text-success' : 'text-text-muted'
              }`}
            >
              {isUp ? <TrendingUp className="h-3 w-3" /> : isDown ? <TrendingDown className="h-3 w-3" /> : null}
              {Math.abs(changePercent).toFixed(1)}%
            </span>
          ) : null}
        </div>
        <Button asChild variant="outline" size="sm" className="mt-2 w-full">
          <Link to={buildPath(card.url)}>Xem lịch sử giá</Link>
        </Button>
      </CardContent>
    </Card>
  )
}

function buildPath(url: string): string {
  try {
    return new URL(url).pathname
  } catch {
    return url
  }
}
