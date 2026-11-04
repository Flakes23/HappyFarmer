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
  province: string
  onSearchChange: (search: string) => void
  onCategoryChange: (categoryId: number | undefined) => void
  onSubCategoryChange: (subCategoryId: number | undefined) => void
  onProvinceChange: (province: string) => void
}

const ALL = 'all'

export function PriceFilterBar({
  search,
  categoryId,
  subCategoryId,
  province,
  onSearchChange,
  onCategoryChange,
  onSubCategoryChange,
  onProvinceChange,
}: PriceFilterBarProps) {
  const [searchInput, setSearchInput] = useState(search)
  const categories = useCategories()
  const subCategories = useSubCategories(categoryId)
  const regions = useRegions()
  // Mỗi tỉnh có thể có nhiều khu vực (mỗi nguồn giá một khu vực) nên danh sách lọc chỉ liệt kê tên tỉnh, không lặp.
  const loadedProvinces = Array.from(new Set(regions.data?.map((r) => r.provinceName) ?? [])).sort((a, b) =>
    a.localeCompare(b, 'vi'),
  )
  // Luôn giữ tỉnh đang chọn trong danh sách (kể cả lúc danh sách khu vực chưa tải xong) để ô chọn không bị trống.
  const provinces = loadedProvinces.includes(province) ? loadedProvinces : [province, ...loadedProvinces]

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
        value={province}
        onValueChange={onProvinceChange}
        disabled={regions.isLoading}
      >
        <SelectTrigger className="w-56">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {provinces.map((name) => (
            <SelectItem key={name} value={name}>
              {name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}
