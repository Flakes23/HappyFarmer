import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import type { CreateListingRequest } from '@/api/types'

export function useCreateListing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (body: CreateListingRequest) => marketplaceApi.createListing(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['listings'] })
      queryClient.invalidateQueries({ queryKey: ['my-listings'] })
    },
  })
}
