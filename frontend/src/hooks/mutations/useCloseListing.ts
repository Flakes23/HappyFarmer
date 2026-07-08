import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useCloseListing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => marketplaceApi.closeListing(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['listing', id] })
      queryClient.invalidateQueries({ queryKey: ['my-listings'] })
      queryClient.invalidateQueries({ queryKey: ['listings'] })
    },
  })
}
