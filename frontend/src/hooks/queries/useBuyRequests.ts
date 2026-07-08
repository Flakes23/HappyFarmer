import { useQuery } from '@tanstack/react-query'
import { marketplaceApi, type BuyRequestFilters } from '@/api/marketplaceApi'

export function useBuyRequests(filters: BuyRequestFilters) {
  return useQuery({
    queryKey: ['buy-requests', filters],
    queryFn: () => marketplaceApi.searchBuyRequests(filters),
  })
}
