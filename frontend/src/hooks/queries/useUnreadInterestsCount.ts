import { useQuery } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import { useAuthStore } from '@/store/authStore'

export function useUnreadInterestsCount() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  return useQuery({
    queryKey: ['my-interests', 'unread-count'],
    queryFn: marketplaceApi.getUnreadInterestsCount,
    enabled: isAuthenticated,
  })
}
