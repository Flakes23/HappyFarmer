import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useMyInterests() {
  return useQuery({
    queryKey: ['my-interests'],
    queryFn: marketplaceApi.getMyInterests,
  })
}
