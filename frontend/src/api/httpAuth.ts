import axios from 'axios'
import { env } from '@/lib/env'
import { attachAuthInterceptors } from '@/api/authRefresh'

/** For AuthService endpoints that require a Bearer token (e.g. GET/PUT /me). */
export const httpAuth = axios.create({
  baseURL: env.authApiUrl,
})

attachAuthInterceptors(httpAuth)
