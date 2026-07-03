import { useState } from 'react'
import { usePrices } from '@/hooks/queries/usePrices'
import { ProductRegionFilterBar } from '@/components/market-price/ProductRegionFilterBar'
import { PriceTable } from '@/components/market-price/PriceTable'
import { TrendingList } from '@/components/market-price/TrendingList'

export function MarketPricePage() {
  const [productId, setProductId] = useState<number | undefined>(undefined)
  const [regionId, setRegionId] = useState<number | undefined>(undefined)

  const prices = usePrices({ productId, regionId })

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-semibold text-text">Giá nông sản</h1>
        <p className="text-text-muted">Tra cứu giá nông sản mới nhất theo sản phẩm và khu vực.</p>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-4 md:col-span-2">
          <ProductRegionFilterBar
            productId={productId}
            regionId={regionId}
            onProductChange={setProductId}
            onRegionChange={setRegionId}
          />
          <PriceTable prices={prices.data} isLoading={prices.isLoading} />
        </div>

        <TrendingList />
      </div>
    </div>
  )
}
