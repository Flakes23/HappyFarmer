import { Minus, TrendingDown, TrendingUp } from 'lucide-react'
import { cn } from '@/lib/utils'

interface PriceTrendBadgeProps {
  changePercent: number | null
  className?: string
}

/** Shared up/down/flat indicator — used by TrendingList and PriceTable so the same product shows the same trend styling everywhere. */
export function PriceTrendBadge({ changePercent, className }: PriceTrendBadgeProps) {
  if (changePercent === null) return null

  const isUp = changePercent > 0
  const isDown = changePercent < 0
  const Icon = isUp ? TrendingUp : isDown ? TrendingDown : Minus

  return (
    <p
      className={cn(
        'flex items-center gap-1 text-xs font-medium',
        isUp ? 'text-success' : isDown ? 'text-error' : 'text-text-muted',
        className,
      )}
    >
      <Icon className="h-3 w-3" />
      {Math.abs(changePercent).toFixed(1)}%
    </p>
  )
}
