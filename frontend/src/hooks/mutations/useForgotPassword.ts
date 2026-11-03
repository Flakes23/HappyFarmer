import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import type { ForgotPasswordRequest } from '@/api/types'

export function useForgotPassword() {
  return useMutation({
    mutationFn: (body: ForgotPasswordRequest) => authApi.forgotPassword(body),
  })
}
