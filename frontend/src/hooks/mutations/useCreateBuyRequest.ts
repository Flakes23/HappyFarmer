import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import type { CreateBuyRequestRequest } from '@/api/types'

export function useCreateBuyRequest() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (body: CreateBuyRequestRequest) => marketplaceApi.createBuyRequest(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['buy-requests'] })
    },
  })
}
