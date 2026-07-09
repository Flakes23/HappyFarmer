import { useProducts } from '@/hooks/queries/useProducts'
import type { InterestResponse } from '@/api/types'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function InterestSummary({ interest }: { interest: InterestResponse }) {
  const products = useProducts()
  const productId = interest.listing?.productId ?? interest.buyRequest?.productId
  const product = products.data?.find((p) => p.id === productId)

  if (interest.listing) {
    return (
      <div className="flex items-center gap-3">
        {interest.listing.imageUrl ? (
          <img
            src={interest.listing.imageUrl}
            alt={product?.nameVi}
            className="h-12 w-12 shrink-0 rounded-md object-cover"
          />
        ) : null}
        <div className="min-w-0">
          <p className="truncate font-medium text-text">
            {product?.nameVi ?? `Sản phẩm #${interest.listing.productId}`}
          </p>
          <p className="text-xs text-text-muted">
            {interest.listing.quantity} {interest.listing.unit} ·{' '}
            {currencyFormatter.format(interest.listing.pricePerUnit)}/{interest.listing.unit}
          </p>
        </div>
      </div>
    )
  }

  if (interest.buyRequest) {
    return (
      <div className="min-w-0">
        <p className="truncate font-medium text-text">
          {product?.nameVi ?? `Sản phẩm #${interest.buyRequest.productId}`}
        </p>
        <p className="text-xs text-text-muted">
          Cần mua: {interest.buyRequest.desiredQuantity} {interest.buyRequest.unit}
          {interest.buyRequest.maxPricePerUnit
            ? ` · Tối đa ${currencyFormatter.format(interest.buyRequest.maxPricePerUnit)}/${interest.buyRequest.unit}`
            : ''}
        </p>
      </div>
    )
  }

  return null
}
