import { httpMarketPrice } from '@/api/httpMarketPrice'
import type {
  CategoryResponse,
  PagedResult,
  PriceHistoryPoint,
  PriceResponse,
  ProductResponse,
  RegionResponse,
  SubCategoryResponse,
  TrendingItem,
} from '@/api/types'

export interface PriceFilters {
  productId?: number
  regionId?: number
  date?: string
  search?: string
  categoryId?: number
  subCategoryId?: number
  page?: number
  pageSize?: number
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

  getCategories: () =>
    httpMarketPrice.get<CategoryResponse[]>('/api/market-price/categories').then((r) => r.data),

  getSubCategories: (categoryId: number) =>
    httpMarketPrice
      .get<SubCategoryResponse[]>(`/api/market-price/categories/${categoryId}/sub-categories`)
      .then((r) => r.data),

  getPrices: (filters: PriceFilters = {}) =>
    httpMarketPrice
      .get<PagedResult<PriceResponse>>('/api/market-price/prices', { params: filters })
      .then((r) => r.data),

  getPriceHistory: (productId: number, params: PriceHistoryParams = {}) =>
    httpMarketPrice
      .get<PriceHistoryPoint[]>(`/api/market-price/prices/${productId}/history`, { params })
      .then((r) => r.data),

  getTrending: () =>
    httpMarketPrice.get<TrendingItem[]>('/api/market-price/prices/trending').then((r) => r.data),
}
