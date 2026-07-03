import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'
import type { LoginRequest } from '@/api/types'

export function useLogin() {
  const setSession = useAuthStore((s) => s.setSession)

  return useMutation({
    mutationFn: (body: LoginRequest) => authApi.login(body),
    onSuccess: (auth) => setSession(auth),
  })
}
