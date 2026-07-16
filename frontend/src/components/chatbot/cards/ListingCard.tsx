import { Link } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import type { ListingCard as ListingCardData } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function ListingCard({ card }: { card: ListingCardData }) {
  return (
    <Card className="w-full max-w-xs">
      <CardContent className="flex gap-3 p-3">
        <div className="h-16 w-16 shrink-0 overflow-hidden rounded-md bg-secondary">
          {card.imageUrl ? (
            <img src={card.imageUrl} alt={card.productName} className="h-full w-full object-cover" />
          ) : null}
        </div>
        <div className="min-w-0 flex-1 space-y-0.5">
          <p className="truncate text-sm font-medium text-text">{card.productName}</p>
          <p className="text-xs text-text-muted">{card.regionName}</p>
          <p className="text-xs text-text-muted">
            {card.quantity} {card.unit}
            {card.farmerName ? ` · ${card.farmerName}` : ''}
          </p>
          <p className="font-semibold text-primary">
            {currencyFormatter.format(card.pricePerUnit)} / {card.unit}
          </p>
          <Button asChild variant="outline" size="sm" className="mt-1 w-full">
            <Link to={buildPath(card.url)}>Xem chi tiết</Link>
          </Button>
        </div>
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
