import { Link } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { SellerInfo } from '@/components/marketplace/SellerInfo'
import { PriceDeviationBadge } from '@/components/marketplace/PriceDeviationBadge'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { formatRelativeTime } from '@/lib/relativeTime'
import type { BuyRequestResponse } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function BuyRequestCard({ buyRequest }: { buyRequest: BuyRequestResponse }) {
  const products = useProducts()
  const regions = useRegions()

  const product = products.data?.find((p) => p.id === buyRequest.productId)
  const region = regions.data?.find((r) => r.id === buyRequest.regionId)

  const otherActiveCount =
    buyRequest.status === 'Active'
      ? Math.max(buyRequest.buyerActiveBuyRequestCount - 1, 0)
      : buyRequest.buyerActiveBuyRequestCount

  return (
    <Link to={`/marketplace/buy-requests/${buyRequest.id}`}>
      <Card className="relative h-full transition-colors hover:border-primary">
        {buyRequest.maxPricePerUnit ? (
          <PriceDeviationBadge
            productId={buyRequest.productId}
            regionId={buyRequest.regionId}
            pricePerUnit={buyRequest.maxPricePerUnit}
          />
        ) : null}
        <CardContent className="space-y-1 p-4">
          <div className="flex items-start justify-between gap-2">
            <p className="font-medium text-text">{product?.nameVi ?? `Sản phẩm #${buyRequest.productId}`}</p>
            <StatusBadge status={buyRequest.status} />
          </div>
          <p className="text-sm text-text-muted">
            {region ? region.provinceName : `Khu vực #${buyRequest.regionId}`}
          </p>
          <p className="text-sm text-text-muted">
            Cần mua: {buyRequest.desiredQuantity} {buyRequest.unit}
          </p>
          {buyRequest.maxPricePerUnit ? (
            <p className="font-semibold text-primary">
              Giá tối đa: {currencyFormatter.format(buyRequest.maxPricePerUnit)} / {buyRequest.unit}
            </p>
          ) : null}
          {buyRequest.description ? <p className="text-sm text-text-muted">{buyRequest.description}</p> : null}

          <SellerInfo
            name={buyRequest.buyerName}
            avatarUrl={buyRequest.buyerAvatarUrl}
            joinedAt={buyRequest.buyerJoinedAt}
            otherActiveCount={otherActiveCount}
            otherActiveLabel="yêu cầu khác"
            className="pt-1"
          />

          <p className="text-[11px] text-text-muted">{formatRelativeTime(buyRequest.createdAt)}</p>
        </CardContent>
      </Card>
    </Link>
  )
}
