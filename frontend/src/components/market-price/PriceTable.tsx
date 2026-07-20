import { Link } from 'react-router-dom'
import { LineChart } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { SourceBadge } from '@/components/market-price/SourceBadge'
import { PriceTrendBadge } from '@/components/market-price/PriceTrendBadge'
import { EmptyState } from '@/components/shared/EmptyState'
import { TableRowsSkeleton } from '@/components/shared/Skeletons'
import { useTrending } from '@/hooks/queries/useTrending'
import type { PriceResponse } from '@/api/types'
import emptyPricesIllustration from '@/assets/illustrations/illustration-empty-prices.webp'

interface PriceTableProps {
  prices: PriceResponse[] | undefined
  isLoading: boolean
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function PriceTable({ prices, isLoading }: PriceTableProps) {
  // Cùng queryKey ['trending'] với TrendingList — React Query dedupe, không gọi thêm
  // request nếu đã có cache. PriceResponse không có sẵn xu hướng giá (chỉ TrendingItem
  // có), nên join phía client theo productId/regionId.
  const trending = useTrending()

  if (isLoading) {
    return <TableRowsSkeleton count={10} />
  }

  if (!prices || prices.length === 0) {
    return (
      <EmptyState
        icon={LineChart}
        illustration={emptyPricesIllustration}
        title="Không có dữ liệu giá phù hợp"
        description="Thử thay đổi bộ lọc sản phẩm/khu vực."
      />
    )
  }

  const trendByKey = new Map(trending.data?.map((t) => [`${t.productId}-${t.regionId}`, t.changePercent]))

  return (
    <>
      <div className="hidden max-h-[560px] overflow-y-auto rounded-lg border border-border sm:block">
        <Table>
          <TableHeader className="sticky top-0 z-10 bg-card">
            <TableRow>
              <TableHead>Sản phẩm</TableHead>
              <TableHead>Giá</TableHead>
              <TableHead>Đơn vị tính</TableHead>
              <TableHead>Nguồn</TableHead>
              <TableHead>Nơi tham khảo</TableHead>
              <TableHead>Ngày</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {prices.map((p) => {
              const changePercent = trendByKey.get(`${p.productId}-${p.regionId}`) ?? null
              return (
                <TableRow key={`${p.productId}-${p.regionId}-${p.unit ?? ''}`} className="even:bg-muted/20">
                  <TableCell>
                    <Link to={`/prices/${p.productId}`} className="text-primary hover:underline">
                      {p.productName}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <p className="font-medium">{currencyFormatter.format(p.price)}</p>
                    <PriceTrendBadge changePercent={changePercent} />
                  </TableCell>
                  <TableCell className="text-text-muted">{p.unit ?? '—'}</TableCell>
                  <TableCell>
                    <SourceBadge source={p.source} />
                  </TableCell>
                  <TableCell>{p.regionName}</TableCell>
                  <TableCell className="text-text-muted">{new Date(p.effectiveDate).toLocaleDateString('vi-VN')}</TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </div>

      <div className="space-y-3 sm:hidden">
        {prices.map((p) => {
          const changePercent = trendByKey.get(`${p.productId}-${p.regionId}`) ?? null
          return (
            <Card key={`${p.productId}-${p.regionId}-${p.unit ?? ''}`} className="transition-shadow hover:shadow-raised">
              <CardContent className="space-y-1 p-4">
                <div className="flex items-start justify-between gap-2">
                  <Link to={`/prices/${p.productId}`} className="font-medium text-primary hover:underline">
                    {p.productName}
                  </Link>
                  <SourceBadge source={p.source} />
                </div>
                {p.unit ? <p className="text-body-sm text-text-muted">{p.unit}</p> : null}
                <p className="text-body-sm text-text-muted">{p.regionName}</p>
                <div className="flex items-center gap-2">
                  <p className="font-semibold text-text">{currencyFormatter.format(p.price)}</p>
                  <PriceTrendBadge changePercent={changePercent} />
                </div>
                <p className="text-xs text-text-muted">{new Date(p.effectiveDate).toLocaleDateString('vi-VN')}</p>
              </CardContent>
            </Card>
          )
        })}
      </div>
    </>
  )
}
