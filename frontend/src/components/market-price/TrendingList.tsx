import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/shared/EmptyState'
import { PriceTrendBadge } from '@/components/market-price/PriceTrendBadge'
import { useTrending } from '@/hooks/queries/useTrending'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function TrendingList() {
  const trending = useTrending()

  return (
    <Card>
      <CardHeader>
        <CardTitle>Biến động giá nổi bật</CardTitle>
      </CardHeader>
      <CardContent>
        {trending.isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </div>
        ) : !trending.data || trending.data.length === 0 ? (
          <EmptyState title="Chưa có dữ liệu biến động" />
        ) : (
          <ul className="divide-y divide-border">
            {trending.data.map((item) => (
              <li key={`${item.productId}-${item.regionId}`} className="flex items-center justify-between py-3">
                <div>
                  <Link to={`/prices/${item.productId}`} className="font-medium text-text hover:text-primary">
                    {item.productName}
                  </Link>
                  <p className="text-xs text-text-muted">{item.regionName}</p>
                </div>
                <div className="text-right">
                  <p className="font-medium">{currencyFormatter.format(item.currentPrice)}</p>
                  <PriceTrendBadge changePercent={item.changePercent} className="justify-end" />
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}
