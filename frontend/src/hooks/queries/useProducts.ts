import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useProducts() {
  return useQuery({
    queryKey: ['products'],
    queryFn: () => marketPriceApi.getProducts(),
    staleTime: 60_000,
  })
}
