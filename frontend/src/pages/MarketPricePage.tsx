import { useEffect, useState } from 'react'
import { usePriceGroups } from '@/hooks/queries/usePriceGroups'
import { useProvinces } from '@/hooks/queries/useProvinces'
import { useRegions } from '@/hooks/queries/useRegions'
import { useAuthStore } from '@/store/authStore'
import { findMatchingProvince } from '@/lib/provinceMatch'
import { PriceFilterBar } from '@/components/market-price/PriceFilterBar'
import { PriceTable } from '@/components/market-price/PriceTable'
import { TrendingList } from '@/components/market-price/TrendingList'
import { Pagination, PaginationContent, PaginationItem, PaginationNext, PaginationPrevious } from '@/components/ui/pagination'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

const PAGE_SIZE = 20
// Tỉnh mặc định khi người dùng chưa đăng nhập, chưa chọn tỉnh trong hồ sơ, hoặc tỉnh của họ chưa có dữ liệu giá.
const DEFAULT_PROVINCE = 'TP. Hồ Chí Minh'

export function MarketPricePage() {
  useDocumentTitle('Giá nông sản — HappyFarmer')
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState<number | undefined>(undefined)
  const [subCategoryId, setSubCategoryId] = useState<number | undefined>(undefined)
  const [province, setProvince] = useState<string>(DEFAULT_PROVINCE)
  // Mặc định chọn sẵn tỉnh trong hồ sơ người dùng nếu có dữ liệu giá ở tỉnh đó, còn lại dùng TP.HCM — chỉ làm 1 lần, và
  // không ghi đè lựa chọn của người dùng nếu họ đã tự đổi bộ lọc. Không có lựa chọn "tất cả tỉnh".
  const user = useAuthStore((s) => s.user)
  const userProvinces = useProvinces()
  const regions = useRegions()
  const [provinceTouched, setProvinceTouched] = useState(false)
  const [provinceResolved, setProvinceResolved] = useState(false)
  const needsProvinceDefault = Boolean(user?.provinceId) && !provinceTouched && !provinceResolved
  const [page, setPage] = useState(1)

  useEffect(() => {
    if (!needsProvinceDefault || !userProvinces.data || !regions.data) return
    const userProvinceName = userProvinces.data.find((p) => p.id === user?.provinceId)?.name
    const match = findMatchingProvince(userProvinceName, regions.data.map((r) => r.provinceName))
    if (match) setProvince(match)
    setProvinceResolved(true)
  }, [needsProvinceDefault, userProvinces.data, regions.data, user?.provinceId])

  // Chờ xác định tỉnh mặc định xong mới gọi API giá, tránh nháy TP.HCM rồi mới đổi sang tỉnh của người dùng
  // (nếu tải tỉnh/khu vực bị lỗi thì vẫn gọi bình thường với tỉnh mặc định).
  const prices = usePriceGroups(
    {
      search: search || undefined,
      categoryId,
      subCategoryId,
      province,
      page,
      pageSize: PAGE_SIZE,
    },
    { enabled: !needsProvinceDefault || userProvinces.isError || regions.isError },
  )

  function resetPage() {
    setPage(1)
  }

  const totalCount = prices.data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-h1 text-text">Giá nông sản</h1>
        <p className="text-body text-text-muted">Tra cứu giá nông sản mới nhất theo sản phẩm và tỉnh thành.</p>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-4 md:col-span-2">
          <PriceFilterBar
            search={search}
            categoryId={categoryId}
            subCategoryId={subCategoryId}
            province={province}
            onSearchChange={(v) => {
              setSearch(v)
              resetPage()
            }}
            onCategoryChange={(v) => {
              setCategoryId(v)
              resetPage()
            }}
            onSubCategoryChange={(v) => {
              setSubCategoryId(v)
              resetPage()
            }}
            onProvinceChange={(v) => {
              setProvinceTouched(true)
              setProvince(v)
              resetPage()
            }}
          />

          <PriceTable groups={prices.data?.items} isLoading={prices.isLoading} />

          {totalPages > 1 ? (
            <Pagination>
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} />
                </PaginationItem>
                <PaginationItem className="px-3 text-body-sm text-text-muted">
                  Trang {page} / {totalPages}
                </PaginationItem>
                <PaginationItem>
                  <PaginationNext
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page >= totalPages}
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          ) : null}
        </div>

        <TrendingList />
      </div>
    </div>
  )
}
