import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'
import type { CreateDiseaseDetectionRequest } from '@/api/types'

export function useCreateDiseaseDetection() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateDiseaseDetectionRequest) => aiAdvisoryApi.detectDisease(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['disease-history'] })
    },
  })
}
