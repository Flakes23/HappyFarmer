import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'

interface ProductRegionFilterBarProps {
  productId: number | undefined
  regionId: number | undefined
  onProductChange: (productId: number | undefined) => void
  onRegionChange: (regionId: number | undefined) => void
}

const ALL = 'all'

export function ProductRegionFilterBar({
  productId,
  regionId,
  onProductChange,
  onRegionChange,
}: ProductRegionFilterBarProps) {
  const products = useProducts()
  const regions = useRegions()

  if (products.isLoading || regions.isLoading) {
    return (
      <div className="flex gap-3">
        <Skeleton className="h-9 w-48" />
        <Skeleton className="h-9 w-48" />
      </div>
    )
  }

  return (
    <div className="flex flex-wrap gap-3">
      <Select
        value={productId ? String(productId) : ALL}
        onValueChange={(v) => onProductChange(v === ALL ? undefined : Number(v))}
      >
        <SelectTrigger className="w-56">
          <SelectValue placeholder="Tất cả sản phẩm" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>Tất cả sản phẩm</SelectItem>
          {products.data?.map((p) => (
            <SelectItem key={p.id} value={String(p.id)}>
              {p.nameVi}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={regionId ? String(regionId) : ALL}
        onValueChange={(v) => onRegionChange(v === ALL ? undefined : Number(v))}
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
    </div>
  )
}
