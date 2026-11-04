import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { EmptyState } from '@/components/shared/EmptyState'
import { TrendingRowSkeleton } from '@/components/shared/Skeletons'
import { PriceTrendBadge } from '@/components/market-price/PriceTrendBadge'
import { useTrending } from '@/hooks/queries/useTrending'
import { useRegions } from '@/hooks/queries/useRegions'
import { sourceLabel } from '@/lib/regionLabels'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function TrendingList() {
  const trending = useTrending()
  const regions = useRegions()
  const regionById = new Map(regions.data?.map((r) => [r.id, r]))

  return (
    <Card>
      <CardHeader>
        <CardTitle>Biến động giá nổi bật</CardTitle>
      </CardHeader>
      <CardContent>
        {trending.isLoading ? (
          <TrendingRowSkeleton />
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
                  <p className="text-xs text-text-muted">
                    {sourceLabel(item.regionName)}
                    {regionById.get(item.regionId) ? ` · ${regionById.get(item.regionId)!.provinceName}` : ''}
                  </p>
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
