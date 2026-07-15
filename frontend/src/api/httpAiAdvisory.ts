import axios from 'axios'
import { env } from '@/lib/env'
import { attachAuthInterceptors } from '@/api/authRefresh'

export const httpAiAdvisory = axios.create({
  baseURL: env.apiGatewayUrl,
})

attachAuthInterceptors(httpAiAdvisory)
