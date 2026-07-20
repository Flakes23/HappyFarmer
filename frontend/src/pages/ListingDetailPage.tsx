import { Link, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { ContactDialog } from '@/components/marketplace/ContactDialog'
import { ImageGallery } from '@/components/marketplace/ImageGallery'
import { SellerInfo } from '@/components/marketplace/SellerInfo'
import { EmptyState } from '@/components/shared/EmptyState'
import { useListing } from '@/hooks/queries/useListing'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { formatRelativeTime } from '@/lib/relativeTime'
import notFoundIllustration from '@/assets/illustrations/illustration-not-found.webp'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function ListingDetailPage() {
  const { id } = useParams<{ id: string }>()
  const listingId = id ? Number(id) : undefined

  const listing = useListing(listingId)
  const products = useProducts()
  const regions = useRegions()
  const user = useAuthStore((s) => s.user)

  const product = products.data?.find((p) => p.id === listing.data?.productId)
  const region = regions.data?.find((r) => r.id === listing.data?.regionId)
  useDocumentTitle(product ? `${product.nameVi} — Chợ nông sản` : 'Chợ nông sản — HappyFarmer')

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Link to="/marketplace" className="flex items-center gap-1 text-body-sm text-primary hover:underline">
          <ArrowLeft className="h-4 w-4" />
          Quay lại chợ nông sản
        </Link>

        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink asChild>
                <Link to="/marketplace">Chợ nông sản</Link>
              </BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>
                {products.isLoading ? (
                  <Skeleton className="inline-block h-4 w-24 align-middle" />
                ) : (
                  (product?.nameVi ?? `Sản phẩm #${listing.data?.productId ?? ''}`)
                )}
              </BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>
      </div>

      {listing.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : !listing.data ? (
        <EmptyState illustration={notFoundIllustration} title="Không tìm thấy tin đăng" />
      ) : (
        <Card>
          <CardHeader className="flex flex-row items-start justify-between gap-4">
            <CardTitle>{product?.nameVi ?? `Sản phẩm #${listing.data.productId}`}</CardTitle>
            <StatusBadge status={listing.data.status} />
          </CardHeader>
          <CardContent className="space-y-4">
            <ImageGallery images={listing.data.imageUrls} alt={product?.nameVi} />

            <dl className="grid grid-cols-2 gap-3 text-body-sm">
              <div>
                <dt className="text-text-muted">Số lượng</dt>
                <dd className="font-medium">
                  {listing.data.quantity} {listing.data.unit}
                </dd>
              </div>
              <div>
                <dt className="text-text-muted">Giá</dt>
                <dd className="font-medium text-primary">
                  {currencyFormatter.format(listing.data.pricePerUnit)} / {listing.data.unit}
                </dd>
              </div>
              <div>
                <dt className="text-text-muted">Khu vực</dt>
                <dd className="font-medium">
                  {regions.isLoading ? (
                    <Skeleton className="inline-block h-4 w-20 align-middle" />
                  ) : (
                    (region ? region.provinceName : `#${listing.data.regionId}`)
                  )}
                </dd>
              </div>
              <div>
                <dt className="text-text-muted">Ngày đăng</dt>
                <dd className="font-medium">{new Date(listing.data.createdAt).toLocaleDateString('vi-VN')}</dd>
              </div>
            </dl>

            {listing.data.description ? <p className="text-text">{listing.data.description}</p> : null}

            <div className="space-y-1 border-t border-border pt-4">
              <SellerInfo
                name={listing.data.farmerName}
                avatarUrl={listing.data.farmerAvatarUrl}
                joinedAt={listing.data.farmerJoinedAt}
                otherActiveCount={
                  listing.data.status === 'Active'
                    ? Math.max(listing.data.farmerActiveListingCount - 1, 0)
                    : listing.data.farmerActiveListingCount
                }
                otherActiveLabel="tin khác đang bán"
              />
              <p className="text-[11px] text-text-muted">{formatRelativeTime(listing.data.createdAt)}</p>
            </div>

            {user?.role === 'Buyer' && listing.data.status === 'Active' ? (
              <ContactDialog id={listing.data.id} kind="listing" />
            ) : null}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
