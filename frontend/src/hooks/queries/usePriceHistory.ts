import { useQuery } from '@tanstack/react-query'
import { marketPriceApi, type PriceHistoryParams } from '@/api/marketPriceApi'

export function usePriceHistory(productId: number | undefined, params: PriceHistoryParams = {}) {
  return useQuery({
    queryKey: ['price-history', productId, params],
    queryFn: () => marketPriceApi.getPriceHistory(productId as number, params),
    enabled: productId !== undefined,
  })
}
