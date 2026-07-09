import { Link } from 'react-router-dom'
import { LineChart } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { SourceBadge } from '@/components/market-price/SourceBadge'
import { EmptyState } from '@/components/shared/EmptyState'
import { ListSkeleton } from '@/components/shared/Skeletons'
import type { PriceResponse } from '@/api/types'

interface PriceTableProps {
  prices: PriceResponse[] | undefined
  isLoading: boolean
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function PriceTable({ prices, isLoading }: PriceTableProps) {
  if (isLoading) {
    return <ListSkeleton count={5} rowHeight="h-10" />
  }

  if (!prices || prices.length === 0) {
    return <EmptyState icon={LineChart} title="Không có dữ liệu giá phù hợp" description="Thử thay đổi bộ lọc sản phẩm/khu vực." />
  }

  return (
    <>
      <div className="hidden rounded-lg border border-border sm:block">
        <Table>
          <TableHeader>
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
            {prices.map((p) => (
              <TableRow key={`${p.productId}-${p.regionId}-${p.unit ?? ''}`}>
                <TableCell>
                  <Link to={`/prices/${p.productId}`} className="text-primary hover:underline">
                    {p.productName}
                  </Link>
                </TableCell>
                <TableCell className="font-medium">{currencyFormatter.format(p.price)}</TableCell>
                <TableCell className="text-text-muted">{p.unit ?? '—'}</TableCell>
                <TableCell>
                  <SourceBadge source={p.source} />
                </TableCell>
                <TableCell>{p.regionName}</TableCell>
                <TableCell className="text-text-muted">{new Date(p.effectiveDate).toLocaleDateString('vi-VN')}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="space-y-3 sm:hidden">
        {prices.map((p) => (
          <Card key={`${p.productId}-${p.regionId}-${p.unit ?? ''}`}>
            <CardContent className="space-y-1 p-4">
              <div className="flex items-start justify-between gap-2">
                <Link to={`/prices/${p.productId}`} className="font-medium text-primary hover:underline">
                  {p.productName}
                </Link>
                <SourceBadge source={p.source} />
              </div>
              {p.unit ? <p className="text-sm text-text-muted">{p.unit}</p> : null}
              <p className="text-sm text-text-muted">{p.regionName}</p>
              <p className="font-semibold text-text">{currencyFormatter.format(p.price)}</p>
              <p className="text-xs text-text-muted">{new Date(p.effectiveDate).toLocaleDateString('vi-VN')}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </>
  )
}
