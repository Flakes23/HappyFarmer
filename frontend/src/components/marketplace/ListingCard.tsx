import { Link } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { SellerInfo } from '@/components/marketplace/SellerInfo'
import { PriceDeviationBadge } from '@/components/marketplace/PriceDeviationBadge'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { formatRelativeTime } from '@/lib/relativeTime'
import type { ListingResponse } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function ListingCard({ listing }: { listing: ListingResponse }) {
  const products = useProducts()
  const regions = useRegions()

  const product = products.data?.find((p) => p.id === listing.productId)
  const region = regions.data?.find((r) => r.id === listing.regionId)

  const otherActiveCount =
    listing.status === 'Active'
      ? Math.max(listing.farmerActiveListingCount - 1, 0)
      : listing.farmerActiveListingCount

  return (
    <Link to={`/marketplace/listings/${listing.id}`}>
      <Card className="relative h-full transition-colors hover:border-primary">
        <PriceDeviationBadge
          productId={listing.productId}
          regionId={listing.regionId}
          pricePerUnit={listing.pricePerUnit}
        />
        <CardContent className="flex gap-4 p-4">
          <div className="relative h-20 w-20 shrink-0 overflow-hidden rounded-md bg-secondary">
            {listing.imageUrls[0] ? (
              <img
                src={listing.imageUrls[0]}
                alt={product?.nameVi ?? 'Ảnh sản phẩm'}
                className="h-full w-full object-cover"
              />
            ) : null}
            {listing.imageUrls.length > 1 ? (
              <span className="absolute bottom-0.5 right-0.5 rounded bg-black/60 px-1 text-[10px] text-white">
                +{listing.imageUrls.length - 1} ảnh
              </span>
            ) : null}
          </div>

          <div className="min-w-0 flex-1 space-y-1">
            <div className="flex items-start justify-between gap-2">
              <p className="font-medium text-text">{product?.nameVi ?? `Sản phẩm #${listing.productId}`}</p>
              <StatusBadge status={listing.status} />
            </div>
            <p className="text-sm text-text-muted">
              {region ? region.provinceName : `Khu vực #${listing.regionId}`}
            </p>
            <p className="text-sm text-text-muted">
              Đang bán: {listing.quantity} {listing.unit}
            </p>
            <p className="font-semibold text-primary">
              {currencyFormatter.format(listing.pricePerUnit)} / {listing.unit}
            </p>

            <SellerInfo
              name={listing.farmerName}
              joinedAt={listing.farmerJoinedAt}
              otherActiveCount={otherActiveCount}
              otherActiveLabel="tin khác đang bán"
              className="pt-1"
            />

            <p className="text-[11px] text-text-muted">{formatRelativeTime(listing.createdAt)}</p>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}
