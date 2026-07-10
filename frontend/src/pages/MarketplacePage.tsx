import { useState } from 'react'
import { Link } from 'react-router-dom'
import { MessageSquare, PackageOpen, Plus, ShoppingBasket } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'
import { MarketplaceFilterBar } from '@/components/marketplace/MarketplaceFilterBar'
import { ListingCard } from '@/components/marketplace/ListingCard'
import { BuyRequestCard } from '@/components/marketplace/BuyRequestCard'
import { EmptyState } from '@/components/shared/EmptyState'
import { CardGridSkeleton } from '@/components/shared/Skeletons'
import { useListings } from '@/hooks/queries/useListings'
import { useBuyRequests } from '@/hooks/queries/useBuyRequests'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import type { MarketplaceSort } from '@/api/marketplaceApi'

const PAGE_SIZE = 20

export function MarketplacePage() {
  useDocumentTitle('Chợ nông sản — HappyFarmer')
  const [regionId, setRegionId] = useState<number | undefined>(undefined)
  const [search, setSearch] = useState('')
  const [minPrice, setMinPrice] = useState<number | undefined>(undefined)
  const [maxPrice, setMaxPrice] = useState<number | undefined>(undefined)
  const [sort, setSort] = useState<MarketplaceSort>('newest')
  const [listingsPage, setListingsPage] = useState(1)
  const [buyRequestsPage, setBuyRequestsPage] = useState(1)

  const filters = { regionId, search: search || undefined, minPrice, maxPrice, sort }

  const listings = useListings({ ...filters, page: listingsPage, pageSize: PAGE_SIZE })
  const buyRequests = useBuyRequests({ ...filters, page: buyRequestsPage, pageSize: PAGE_SIZE })

  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)

  function resetPages() {
    setListingsPage(1)
    setBuyRequestsPage(1)
  }

  function totalPages(totalCount: number | undefined) {
    return Math.max(1, Math.ceil((totalCount ?? 0) / PAGE_SIZE))
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-text">Chợ nông sản</h1>
          <p className="text-text-muted">Đăng bán, tìm mua nông sản trực tiếp giữa nông dân và người mua.</p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {isAuthenticated ? (
            <Button variant="outline" size="sm" asChild>
              <Link to="/marketplace/my-interests">
                <MessageSquare className="h-4 w-4" />
                Liên hệ của tôi
              </Link>
            </Button>
          ) : null}

          {user?.role === 'Farmer' ? (
            <>
              <Button variant="outline" size="sm" asChild>
                <Link to="/marketplace/my-listings">Tin của tôi</Link>
              </Button>
              <Button size="sm" asChild>
                <Link to="/marketplace/new">
                  <Plus className="h-4 w-4" />
                  Đăng tin bán
                </Link>
              </Button>
            </>
          ) : null}

          {user?.role === 'Buyer' ? (
            <Button size="sm" asChild>
              <Link to="/marketplace/buy-requests/new">
                <Plus className="h-4 w-4" />
                Đăng yêu cầu mua
              </Link>
            </Button>
          ) : null}
        </div>
      </div>

      <MarketplaceFilterBar
        regionId={regionId}
        search={search}
        minPrice={minPrice}
        maxPrice={maxPrice}
        sort={sort}
        onRegionChange={(v) => {
          setRegionId(v)
          resetPages()
        }}
        onSearchChange={(v) => {
          setSearch(v)
          resetPages()
        }}
        onPriceRangeChange={(min, max) => {
          setMinPrice(min)
          setMaxPrice(max)
          resetPages()
        }}
        onSortChange={(v) => {
          setSort(v)
          resetPages()
        }}
      />

      <Tabs defaultValue="listings">
        <TabsList>
          <TabsTrigger value="listings">Tin bán</TabsTrigger>
          <TabsTrigger value="buy-requests">Yêu cầu mua</TabsTrigger>
        </TabsList>

        <TabsContent value="listings" className="space-y-4">
          {listings.isLoading ? (
            <CardGridSkeleton count={8} />
          ) : listings.data && listings.data.items.length > 0 ? (
            <>
              <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
                {listings.data.items.map((l) => (
                  <ListingCard key={l.id} listing={l} />
                ))}
              </div>
              {totalPages(listings.data.totalCount) > 1 ? (
                <Pagination>
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious
                        onClick={() => setListingsPage((p) => Math.max(1, p - 1))}
                        disabled={listingsPage === 1}
                      />
                    </PaginationItem>
                    <PaginationItem className="px-3 text-sm text-text-muted">
                      Trang {listingsPage} / {totalPages(listings.data.totalCount)}
                    </PaginationItem>
                    <PaginationItem>
                      <PaginationNext
                        onClick={() => setListingsPage((p) => Math.min(totalPages(listings.data?.totalCount), p + 1))}
                        disabled={listingsPage >= totalPages(listings.data.totalCount)}
                      />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              ) : null}
            </>
          ) : (
            <EmptyState
              icon={PackageOpen}
              title="Chưa có tin đăng bán phù hợp"
              description="Thử thay đổi bộ lọc, hoặc quay lại sau khi có tin đăng mới."
            />
          )}
        </TabsContent>

        <TabsContent value="buy-requests" className="space-y-4">
          {buyRequests.isLoading ? (
            <CardGridSkeleton count={8} />
          ) : buyRequests.data && buyRequests.data.items.length > 0 ? (
            <>
              <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
                {buyRequests.data.items.map((br) => (
                  <BuyRequestCard key={br.id} buyRequest={br} />
                ))}
              </div>
              {totalPages(buyRequests.data.totalCount) > 1 ? (
                <Pagination>
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious
                        onClick={() => setBuyRequestsPage((p) => Math.max(1, p - 1))}
                        disabled={buyRequestsPage === 1}
                      />
                    </PaginationItem>
                    <PaginationItem className="px-3 text-sm text-text-muted">
                      Trang {buyRequestsPage} / {totalPages(buyRequests.data.totalCount)}
                    </PaginationItem>
                    <PaginationItem>
                      <PaginationNext
                        onClick={() =>
                          setBuyRequestsPage((p) => Math.min(totalPages(buyRequests.data?.totalCount), p + 1))
                        }
                        disabled={buyRequestsPage >= totalPages(buyRequests.data.totalCount)}
                      />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              ) : null}
            </>
          ) : (
            <EmptyState
              icon={ShoppingBasket}
              title="Chưa có yêu cầu mua phù hợp"
              description="Thử thay đổi bộ lọc, hoặc quay lại sau khi có yêu cầu mới."
            />
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}
