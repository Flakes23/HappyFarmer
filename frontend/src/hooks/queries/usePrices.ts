import { useQuery } from '@tanstack/react-query'
import { marketPriceApi, type PriceFilters } from '@/api/marketPriceApi'

export function usePrices(filters: PriceFilters) {
  return useQuery({
    queryKey: ['prices', filters],
    queryFn: () => marketPriceApi.getPrices(filters),
  })
}
