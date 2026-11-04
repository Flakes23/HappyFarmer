import { useQuery } from '@tanstack/react-query'
import { marketPriceApi, type PriceFilters } from '@/api/marketPriceApi'

export function usePriceGroups(filters: Omit<PriceFilters, 'productId'>, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['price-groups', filters],
    queryFn: () => marketPriceApi.getPriceGroups(filters),
    enabled: options?.enabled,
  })
}
