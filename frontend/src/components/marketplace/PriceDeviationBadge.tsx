import { usePrices } from '@/hooks/queries/usePrices'

interface PriceDeviationBadgeProps {
  productId: number
  regionId: number
  pricePerUnit: number
}

const THRESHOLD_PERCENT = 10

export function PriceDeviationBadge({ productId, regionId, pricePerUnit }: PriceDeviationBadgeProps) {
  const prices = usePrices({ productId })
  const reference = prices.data?.items.find((p) => p.regionId === regionId)

  if (!reference || reference.price <= 0) return null

  const deviationPercent = ((pricePerUnit - reference.price) / reference.price) * 100
  const isEqual = deviationPercent === 0
  if (!isEqual && Math.abs(deviationPercent) < THRESHOLD_PERCENT) return null

  const isHigher = deviationPercent > 0

  return (
    <span
      className={`absolute -top-2 left-3 rounded-full px-2 py-0.5 text-[11px] font-semibold shadow-sm ${
        isEqual
          ? 'bg-muted text-text-muted'
          : isHigher
            ? 'bg-accent text-accent-foreground'
            : 'bg-success text-success-foreground'
      }`}
    >
      {isEqual
        ? 'Giá bằng giá thị trường'
        : `Giá ${isHigher ? 'cao hơn' : 'thấp hơn'} ${Math.abs(deviationPercent).toFixed(0)}% so với giá thị trường`}
    </span>
  )
}
