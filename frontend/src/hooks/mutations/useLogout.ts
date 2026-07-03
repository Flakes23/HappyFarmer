import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'

export function useLogout() {
  const refreshToken = useAuthStore((s) => s.refreshToken)
  const clearSession = useAuthStore((s) => s.clearSession)

  return useMutation({
    mutationFn: async () => {
      if (refreshToken) {
        try {
          await authApi.logout({ refreshToken })
        } catch {
          // best-effort: always clear client-side session regardless of API result
        }
      }
    },
    onSettled: () => clearSession(),
  })
}
