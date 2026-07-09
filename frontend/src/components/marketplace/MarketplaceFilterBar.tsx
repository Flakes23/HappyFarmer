import { useEffect, useState } from 'react'
import { SlidersHorizontal } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useRegions } from '@/hooks/queries/useRegions'
import type { MarketplaceSort } from '@/api/marketplaceApi'

interface MarketplaceFilterBarProps {
  regionId: number | undefined
  search: string
  minPrice: number | undefined
  maxPrice: number | undefined
  sort: MarketplaceSort
  onRegionChange: (regionId: number | undefined) => void
  onSearchChange: (search: string) => void
  onPriceRangeChange: (minPrice: number | undefined, maxPrice: number | undefined) => void
  onSortChange: (sort: MarketplaceSort) => void
}

const ALL_REGIONS = 'all'

export function MarketplaceFilterBar({
  regionId,
  search,
  minPrice,
  maxPrice,
  sort,
  onRegionChange,
  onSearchChange,
  onPriceRangeChange,
  onSortChange,
}: MarketplaceFilterBarProps) {
  const regions = useRegions()
  const [searchInput, setSearchInput] = useState(search)
  const [minInput, setMinInput] = useState(minPrice?.toString() ?? '')
  const [maxInput, setMaxInput] = useState(maxPrice?.toString() ?? '')

  useEffect(() => {
    const timeout = setTimeout(() => onSearchChange(searchInput.trim()), 400)
    return () => clearTimeout(timeout)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput])

  function applyPriceRange() {
    onPriceRangeChange(minInput ? Number(minInput) : undefined, maxInput ? Number(maxInput) : undefined)
  }

  const hasPriceFilter = minPrice !== undefined || maxPrice !== undefined

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Select
        value={regionId ? String(regionId) : ALL_REGIONS}
        onValueChange={(v) => onRegionChange(v === ALL_REGIONS ? undefined : Number(v))}
      >
        <SelectTrigger className="w-48">
          <SelectValue placeholder="Tất cả khu vực" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL_REGIONS}>Tất cả khu vực</SelectItem>
          {regions.data?.map((r) => (
            <SelectItem key={r.id} value={String(r.id)}>
              {r.provinceName}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Input
        placeholder="Tìm theo mô tả, đơn vị..."
        value={searchInput}
        onChange={(e) => setSearchInput(e.target.value)}
        className="w-56"
      />

      <Popover>
        <PopoverTrigger asChild>
          <Button variant="outline" size="sm">
            <SlidersHorizontal className="h-4 w-4" />
            Khoảng giá
            {hasPriceFilter ? <span className="ml-1 text-primary">•</span> : null}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-72 space-y-3">
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="text-xs text-text-muted">Giá từ</label>
              <Input
                type="number"
                min={0}
                placeholder="0"
                value={minInput}
                onChange={(e) => setMinInput(e.target.value)}
              />
            </div>
            <div>
              <label className="text-xs text-text-muted">Đến</label>
              <Input
                type="number"
                min={0}
                placeholder="Không giới hạn"
                value={maxInput}
                onChange={(e) => setMaxInput(e.target.value)}
              />
            </div>
          </div>
          <Button size="sm" className="w-full" onClick={applyPriceRange}>
            Áp dụng
          </Button>
        </PopoverContent>
      </Popover>

      <Select value={sort} onValueChange={(v) => onSortChange(v as MarketplaceSort)}>
        <SelectTrigger className="w-44">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="newest">Mới nhất</SelectItem>
          <SelectItem value="price_asc">Giá tăng dần</SelectItem>
          <SelectItem value="price_desc">Giá giảm dần</SelectItem>
        </SelectContent>
      </Select>
    </div>
  )
}
