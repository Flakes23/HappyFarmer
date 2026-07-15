import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useDeleteHarvestPrediction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => aiAdvisoryApi.deleteHarvestPrediction(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['harvest-history'] })
      queryClient.removeQueries({ queryKey: ['harvest-detail', id] })
    },
  })
}
