import { rawAuthClient } from '@/api/rawAuthClient'
import { httpAuth } from '@/api/httpAuth'
import type {
  AuthResponse,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
  UserResponse,
} from '@/api/types'

export const authApi = {
  register: (body: RegisterRequest) =>
    rawAuthClient.post<AuthResponse>('/api/auth/register', body).then((r) => r.data),

  login: (body: LoginRequest) =>
    rawAuthClient.post<AuthResponse>('/api/auth/login', body).then((r) => r.data),

  refreshToken: (body: RefreshTokenRequest) =>
    rawAuthClient.post<AuthResponse>('/api/auth/refresh-token', body).then((r) => r.data),

  logout: (body: RefreshTokenRequest) =>
    rawAuthClient.post<void>('/api/auth/logout', body).then((r) => r.data),

  getMe: () => httpAuth.get<UserResponse>('/api/auth/me').then((r) => r.data),
}

/** Backend duplicate-conflict bodies have been observed with inconsistent casing (`message`/`Message`). */
export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: Record<string, unknown> } }).response
    const data = response?.data
    if (data) {
      const message = data.message ?? data.Message
      if (typeof message === 'string') return message
    }
  }
  return fallback
}
