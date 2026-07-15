import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useDeleteDiseaseDetection() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => aiAdvisoryApi.deleteDiseaseDetection(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['disease-history'] })
      queryClient.removeQueries({ queryKey: ['disease-detail', id] })
    },
  })
}
