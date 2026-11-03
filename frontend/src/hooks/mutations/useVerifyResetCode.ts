import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import type { VerifyResetCodeRequest } from '@/api/types'

export function useVerifyResetCode() {
  return useMutation({
    mutationFn: (body: VerifyResetCodeRequest) => authApi.verifyResetCode(body),
  })
}
