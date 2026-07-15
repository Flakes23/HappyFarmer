import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useDiseaseHistory() {
  return useQuery({
    queryKey: ['disease-history'],
    queryFn: aiAdvisoryApi.getDiseaseHistory,
  })
}
