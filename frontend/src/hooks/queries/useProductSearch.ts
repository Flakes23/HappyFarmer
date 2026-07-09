import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useProductSearch(search: string) {
  const trimmed = search.trim()

  return useQuery({
    queryKey: ['products', 'search', trimmed],
    queryFn: () => marketPriceApi.getProducts({ search: trimmed }),
    enabled: trimmed.length > 0,
  })
}
