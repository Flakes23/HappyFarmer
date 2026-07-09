import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: marketPriceApi.getCategories,
    staleTime: 60_000,
  })
}
