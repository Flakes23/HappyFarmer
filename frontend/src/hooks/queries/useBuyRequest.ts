import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useBuyRequest(id: number | undefined) {
  return useQuery({
    queryKey: ['buy-request', id],
    queryFn: () => marketplaceApi.getBuyRequest(id!),
    enabled: id !== undefined,
  })
}
