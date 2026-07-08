import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useListing(id: number | undefined) {
  return useQuery({
    queryKey: ['listing', id],
    queryFn: () => marketplaceApi.getListing(id!),
    enabled: id !== undefined,
  })
}
