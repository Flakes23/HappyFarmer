import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useHarvestHistory() {
  return useQuery({
    queryKey: ['harvest-history'],
    queryFn: aiAdvisoryApi.getHarvestHistory,
  })
}
