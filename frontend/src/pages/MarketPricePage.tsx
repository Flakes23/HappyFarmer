import { useState } from 'react'
import { usePrices } from '@/hooks/queries/usePrices'
import { PriceFilterBar } from '@/components/market-price/PriceFilterBar'
import { PriceTable } from '@/components/market-price/PriceTable'
import { TrendingList } from '@/components/market-price/TrendingList'
import { Pagination, PaginationContent, PaginationItem, PaginationNext, PaginationPrevious } from '@/components/ui/pagination'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

const PAGE_SIZE = 20

export function MarketPricePage() {
  useDocumentTitle('Giá nông sản — HappyFarmer')
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState<number | undefined>(undefined)
  const [subCategoryId, setSubCategoryId] = useState<number | undefined>(undefined)
  const [regionId, setRegionId] = useState<number | undefined>(undefined)
  const [page, setPage] = useState(1)

  const prices = usePrices({
    search: search || undefined,
    categoryId,
    subCategoryId,
    regionId,
    page,
    pageSize: PAGE_SIZE,
  })

  function resetPage() {
    setPage(1)
  }

  const totalCount = prices.data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-h1 text-text">Giá nông sản</h1>
        <p className="text-body text-text-muted">Tra cứu giá nông sản mới nhất theo sản phẩm và khu vực.</p>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-4 md:col-span-2">
          <PriceFilterBar
            search={search}
            categoryId={categoryId}
            subCategoryId={subCategoryId}
            regionId={regionId}
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
            onRegionChange={(v) => {
              setRegionId(v)
              resetPage()
            }}
          />

          <PriceTable prices={prices.data?.items} isLoading={prices.isLoading} />

          {totalPages > 1 ? (
            <Pagination>
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} />
                </PaginationItem>
                <PaginationItem className="px-3 text-sm text-text-muted">
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
