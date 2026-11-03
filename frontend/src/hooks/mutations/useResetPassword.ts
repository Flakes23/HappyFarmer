import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import type { ResetPasswordRequest } from '@/api/types'

export function useResetPassword() {
  return useMutation({
    mutationFn: (body: ResetPasswordRequest) => authApi.resetPassword(body),
  })
}
