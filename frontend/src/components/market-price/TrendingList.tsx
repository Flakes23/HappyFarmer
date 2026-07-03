import { Link } from 'react-router-dom'
import { TrendingUp, TrendingDown } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
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
          <p className="text-text-muted">Chưa có dữ liệu biến động.</p>
        ) : (
          <ul className="divide-y divide-border">
            {trending.data.map((item) => {
              const isUp = (item.changePercent ?? 0) >= 0
              return (
                <li key={`${item.productId}-${item.regionId}`} className="flex items-center justify-between py-3">
                  <div>
                    <Link
                      to={`/prices/${item.productId}`}
                      className="font-medium text-text hover:text-primary"
                    >
                      {item.productName}
                    </Link>
                    <p className="text-xs text-text-muted">{item.regionName}</p>
                  </div>
                  <div className="text-right">
                    <p className="font-medium">{currencyFormatter.format(item.currentPrice)}</p>
                    {item.changePercent !== null ? (
                      <p
                        className={`flex items-center justify-end gap-1 text-xs font-medium ${
                          isUp ? 'text-accent' : 'text-error'
                        }`}
                      >
                        {isUp ? <TrendingUp className="h-3 w-3" /> : <TrendingDown className="h-3 w-3" />}
                        {Math.abs(item.changePercent).toFixed(1)}%
                      </p>
                    ) : null}
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}
