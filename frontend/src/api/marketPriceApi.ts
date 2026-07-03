import { httpMarketPrice } from '@/api/httpMarketPrice'
import type {
  PriceHistoryPoint,
  PriceResponse,
  ProductResponse,
  RegionResponse,
  TrendingItem,
} from '@/api/types'

export interface PriceFilters {
  productId?: number
  regionId?: number
  date?: string
}

export interface PriceHistoryParams {
  regionId?: number
  from?: string
  to?: string
}

export const marketPriceApi = {
  getProducts: () =>
    httpMarketPrice.get<ProductResponse[]>('/api/market-price/products').then((r) => r.data),

  getRegions: () =>
    httpMarketPrice.get<RegionResponse[]>('/api/market-price/regions').then((r) => r.data),

  getPrices: (filters: PriceFilters = {}) =>
    httpMarketPrice
      .get<PriceResponse[]>('/api/market-price/prices', { params: filters })
      .then((r) => r.data),

  getPriceHistory: (productId: number, params: PriceHistoryParams = {}) =>
    httpMarketPrice
      .get<PriceHistoryPoint[]>(`/api/market-price/prices/${productId}/history`, { params })
      .then((r) => r.data),

  getTrending: () =>
    httpMarketPrice.get<TrendingItem[]>('/api/market-price/prices/trending').then((r) => r.data),
}
