import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import type { UpdateListingRequest } from '@/api/types'

export function useUpdateListing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, body }: { id: number; body: UpdateListingRequest }) =>
      marketplaceApi.updateListing(id, body),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['listing', id] })
      queryClient.invalidateQueries({ queryKey: ['my-listings'] })
    },
  })
}
