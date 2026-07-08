import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useMessages(interestId: number | undefined) {
  return useQuery({
    queryKey: ['messages', interestId],
    queryFn: () => marketplaceApi.getMessages(interestId!),
    enabled: interestId !== undefined,
  })
}
