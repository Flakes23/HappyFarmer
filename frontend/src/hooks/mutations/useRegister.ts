import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'
import type { RegisterRequest } from '@/api/types'

export function useRegister() {
  const setSession = useAuthStore((s) => s.setSession)

  return useMutation({
    mutationFn: (body: RegisterRequest) => authApi.register(body),
    onSuccess: (auth) => setSession(auth),
  })
}
