import { useEffect, useState } from 'react'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useCategories } from '@/hooks/queries/useCategories'
import { useSubCategories } from '@/hooks/queries/useSubCategories'
import { useRegions } from '@/hooks/queries/useRegions'

interface PriceFilterBarProps {
  search: string
  categoryId: number | undefined
  subCategoryId: number | undefined
  regionId: number | undefined
  onSearchChange: (search: string) => void
  onCategoryChange: (categoryId: number | undefined) => void
  onSubCategoryChange: (subCategoryId: number | undefined) => void
  onRegionChange: (regionId: number | undefined) => void
}

const ALL = 'all'

export function PriceFilterBar({
  search,
  categoryId,
  subCategoryId,
  regionId,
  onSearchChange,
  onCategoryChange,
  onSubCategoryChange,
  onRegionChange,
}: PriceFilterBarProps) {
  const [searchInput, setSearchInput] = useState(search)
  const categories = useCategories()
  const subCategories = useSubCategories(categoryId)
  const regions = useRegions()

  useEffect(() => {
    const timeout = setTimeout(() => onSearchChange(searchInput.trim()), 400)
    return () => clearTimeout(timeout)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput])

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Input
        placeholder="Tìm theo tên nông sản..."
        value={searchInput}
        onChange={(e) => setSearchInput(e.target.value)}
        className="w-56"
      />

      <Select
        value={categoryId ? String(categoryId) : ALL}
        onValueChange={(v) => {
          onCategoryChange(v === ALL ? undefined : Number(v))
          onSubCategoryChange(undefined)
        }}
        disabled={categories.isLoading}
      >
        <SelectTrigger className="w-48">
          <SelectValue placeholder={categories.isLoading ? 'Đang tải...' : 'Tất cả danh mục'} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>Tất cả danh mục</SelectItem>
          {categories.data?.map((c) => (
            <SelectItem key={c.id} value={String(c.id)}>
              {c.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={subCategoryId ? String(subCategoryId) : ALL}
        onValueChange={(v) => onSubCategoryChange(v === ALL ? undefined : Number(v))}
        disabled={!categoryId || subCategories.isLoading}
      >
        <SelectTrigger className="w-48">
          <SelectValue placeholder={subCategories.isLoading ? 'Đang tải...' : 'Tất cả loại'} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>Tất cả loại</SelectItem>
          {subCategories.data?.map((sc) => (
            <SelectItem key={sc.id} value={String(sc.id)}>
              {sc.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select
        value={regionId ? String(regionId) : ALL}
        onValueChange={(v) => onRegionChange(v === ALL ? undefined : Number(v))}
        disabled={regions.isLoading}
      >
        <SelectTrigger className="w-56">
          <SelectValue placeholder={regions.isLoading ? 'Đang tải...' : 'Tất cả khu vực'} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>Tất cả khu vực</SelectItem>
          {regions.data?.map((r) => (
            <SelectItem key={r.id} value={String(r.id)}>
              {r.provinceName}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}
