import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useDiseaseDetectionDetail(id: number | null) {
  return useQuery({
    queryKey: ['disease-detail', id],
    queryFn: () => aiAdvisoryApi.getDiseaseDetectionDetail(id!),
    enabled: id !== null,
  })
}
