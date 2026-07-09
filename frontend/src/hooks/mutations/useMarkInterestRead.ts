import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useMarkInterestRead() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (interestId: number) => marketplaceApi.markInterestRead(interestId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-interests'] })
    },
  })
}
