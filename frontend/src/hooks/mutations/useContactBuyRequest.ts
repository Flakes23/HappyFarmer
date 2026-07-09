import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useContactBuyRequest() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, message }: { id: number; message: string }) =>
      marketplaceApi.contactBuyRequest(id, message),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-interests'] })
    },
  })
}
