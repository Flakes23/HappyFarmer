import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useMyListings() {
  return useQuery({
    queryKey: ['my-listings'],
    queryFn: marketplaceApi.getMyListings,
  })
}
