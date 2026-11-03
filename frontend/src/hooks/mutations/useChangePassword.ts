import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import type { ChangePasswordRequest } from '@/api/types'

export function useChangePassword() {
  return useMutation({
    mutationFn: (body: ChangePasswordRequest) => authApi.changePassword(body),
  })
}
