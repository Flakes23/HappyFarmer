import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useHarvestPredictionDetail(id: number | null) {
  return useQuery({
    queryKey: ['harvest-detail', id],
    queryFn: () => aiAdvisoryApi.getHarvestPredictionDetail(id!),
    enabled: id !== null,
  })
}
