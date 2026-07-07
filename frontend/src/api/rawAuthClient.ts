import axios from 'axios'
import { env } from '@/lib/env'

/**
 * Plain axios instance for AuthService endpoints that must never go through
 * the bearer/refresh interceptor: register, login, refresh-token, logout.
 * Used internally by authApi.ts and by the refresh logic in authRefresh.ts
 * (calling refresh-token through the interceptor-bearing client would recurse).
 */
export const rawAuthClient = axios.create({
  baseURL: env.apiGatewayUrl,
})
