import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useTrending() {
  return useQuery({
    queryKey: ['trending'],
    queryFn: marketPriceApi.getTrending,
    staleTime: 15 * 60_000,
  })
}
