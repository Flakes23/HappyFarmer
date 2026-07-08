import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'

export function useContactListing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, message }: { id: number; message: string }) =>
      marketplaceApi.contactListing(id, message),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-interests'] })
    },
  })
}
