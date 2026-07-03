import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios'
import { rawAuthClient } from '@/api/rawAuthClient'
import { useAuthStore } from '@/store/authStore'
import type { AuthResponse } from '@/api/types'

interface RetryableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

let refreshPromise: Promise<AuthResponse> | null = null

function redirectToLogin() {
  const { pathname } = window.location
  if (pathname !== '/login' && pathname !== '/register') {
    window.location.assign('/login')
  }
}

function refreshAccessToken(): Promise<AuthResponse> {
  if (refreshPromise) return refreshPromise

  const refreshToken = useAuthStore.getState().refreshToken
  if (!refreshToken) {
    return Promise.reject(new Error('Không có refresh token.'))
  }

  refreshPromise = rawAuthClient
    .post<AuthResponse>('/api/auth/refresh-token', { refreshToken })
    .then((res) => {
      useAuthStore.getState().setSession(res.data)
      return res.data
    })
    .catch((err) => {
      useAuthStore.getState().clearSession()
      redirectToLogin()
      throw err
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

/** Attaches Authorization header + 401-refresh-and-retry-once behavior to an axios instance. */
export function attachAuthInterceptors(client: AxiosInstance) {
  client.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken
    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`)
    }
    return config
  })

  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as RetryableConfig | undefined
      if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
        originalRequest._retry = true
        const auth = await refreshAccessToken()
        originalRequest.headers.set('Authorization', `Bearer ${auth.accessToken}`)
        return client(originalRequest)
      }
      return Promise.reject(error)
    }
  )
}
