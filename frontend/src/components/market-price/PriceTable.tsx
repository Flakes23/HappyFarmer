import { Link } from 'react-router-dom'
import { Skeleton } from '@/components/ui/skeleton'
import { SourceBadge } from '@/components/market-price/SourceBadge'
import type { PriceResponse } from '@/api/types'

interface PriceTableProps {
  prices: PriceResponse[] | undefined
  isLoading: boolean
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function PriceTable({ prices, isLoading }: PriceTableProps) {
  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    )
  }

  if (!prices || prices.length === 0) {
    return <p className="py-8 text-center text-text-muted">Không có dữ liệu giá phù hợp.</p>
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-left text-sm">
        <thead className="bg-secondary text-secondary-foreground">
          <tr>
            <th className="px-4 py-2 font-medium">Sản phẩm</th>
            <th className="px-4 py-2 font-medium">Khu vực</th>
            <th className="px-4 py-2 font-medium">Giá</th>
            <th className="px-4 py-2 font-medium">Nguồn</th>
            <th className="px-4 py-2 font-medium">Ngày</th>
          </tr>
        </thead>
        <tbody>
          {prices.map((p) => (
            <tr key={`${p.productId}-${p.regionId}`} className="border-t border-border">
              <td className="px-4 py-2">
                <Link to={`/prices/${p.productId}`} className="text-primary hover:underline">
                  {p.productName}
                </Link>
              </td>
              <td className="px-4 py-2">{p.regionName}</td>
              <td className="px-4 py-2 font-medium">{currencyFormatter.format(p.price)}</td>
              <td className="px-4 py-2">
                <SourceBadge source={p.source} />
              </td>
              <td className="px-4 py-2 text-text-muted">{p.effectiveDate}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
