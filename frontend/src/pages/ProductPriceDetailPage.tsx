import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { usePriceHistory } from '@/hooks/queries/usePriceHistory'
import { PriceHistoryChart } from '@/components/market-price/PriceHistoryChart'

const ALL = 'all'

export function ProductPriceDetailPage() {
  const { productId } = useParams<{ productId: string }>()
  const productIdNum = productId ? Number(productId) : undefined
  const [regionId, setRegionId] = useState<number | undefined>(undefined)

  const products = useProducts()
  const regions = useRegions()
  const history = usePriceHistory(productIdNum, { regionId })

  const product = products.data?.find((p) => p.id === productIdNum)

  return (
    <div className="space-y-6">
      <Link to="/prices" className="flex items-center gap-1 text-sm text-primary hover:underline">
        <ArrowLeft className="h-4 w-4" />
        Quay lại danh sách giá
      </Link>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle>
            {product ? `Lịch sử giá — ${product.nameVi}` : 'Lịch sử giá'}
            {product?.unit ? <span className="ml-1 text-sm font-normal text-text-muted">({product.unit})</span> : null}
          </CardTitle>

          <Select
            value={regionId ? String(regionId) : ALL}
            onValueChange={(v) => setRegionId(v === ALL ? undefined : Number(v))}
          >
            <SelectTrigger className="w-56">
              <SelectValue placeholder="Tất cả khu vực" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL}>Tất cả khu vực</SelectItem>
              {regions.data?.map((r) => (
                <SelectItem key={r.id} value={String(r.id)}>
                  {r.marketName} — {r.provinceName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardHeader>
        <CardContent>
          <PriceHistoryChart data={history.data} isLoading={history.isLoading} />
        </CardContent>
      </Card>
    </div>
  )
}
