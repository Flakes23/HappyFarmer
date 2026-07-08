import { useQuery } from '@tanstack/react-query'
import { marketplaceApi, type ListingFilters } from '@/api/marketplaceApi'

export function useListings(filters: ListingFilters) {
  return useQuery({
    queryKey: ['listings', filters],
    queryFn: () => marketplaceApi.searchListings(filters),
  })
}
