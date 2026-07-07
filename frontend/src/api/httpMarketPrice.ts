import axios from 'axios'
import { env } from '@/lib/env'
import { attachAuthInterceptors } from '@/api/authRefresh'

export const httpMarketPrice = axios.create({
  baseURL: env.apiGatewayUrl,
})

attachAuthInterceptors(httpMarketPrice)
