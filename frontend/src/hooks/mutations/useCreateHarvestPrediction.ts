import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'
import type { CreateHarvestPredictionRequest } from '@/api/types'

export function useCreateHarvestPrediction() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateHarvestPredictionRequest) => aiAdvisoryApi.predictHarvest(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['harvest-history'] })
    },
  })
}
