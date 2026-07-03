import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useRegions() {
  return useQuery({
    queryKey: ['regions'],
    queryFn: marketPriceApi.getRegions,
    staleTime: 60_000,
  })
}
