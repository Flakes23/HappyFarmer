import { useQuery } from '@tanstack/react-query'
import { marketPriceApi } from '@/api/marketPriceApi'

export function useSubCategories(categoryId: number | undefined) {
  return useQuery({
    queryKey: ['subCategories', categoryId],
    queryFn: () => marketPriceApi.getSubCategories(categoryId!),
    enabled: categoryId !== undefined,
    staleTime: 60_000,
  })
}
