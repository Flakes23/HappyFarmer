import { httpMarketplace } from '@/api/httpMarketplace'
import type {
  BuyRequestResponse,
  BuyRequestStatus,
  CreateBuyRequestRequest,
  CreateListingRequest,
  InterestResponse,
  ListingResponse,
  ListingStatus,
  MessageHistoryResponse,
  MessageResponse,
  PagedResult,
  UpdateListingRequest,
  UploadSignatureResponse,
} from '@/api/types'

export type MarketplaceSort = 'newest' | 'price_asc' | 'price_desc'

export interface ListingFilters {
  productId?: number
  regionId?: number
  status?: ListingStatus
  search?: string
  minPrice?: number
  maxPrice?: number
  sort?: MarketplaceSort
  page?: number
  pageSize?: number
}

export interface BuyRequestFilters {
  productId?: number
  regionId?: number
  status?: BuyRequestStatus
  search?: string
  minPrice?: number
  maxPrice?: number
  sort?: MarketplaceSort
  page?: number
  pageSize?: number
}

export const marketplaceApi = {
  searchListings: (filters: ListingFilters = {}) =>
    httpMarketplace
      .get<PagedResult<ListingResponse>>('/api/marketplace/listings', { params: filters })
      .then((r) => r.data),

  getListing: (id: number) =>
    httpMarketplace.get<ListingResponse>(`/api/marketplace/listings/${id}`).then((r) => r.data),

  getMyListings: () =>
    httpMarketplace.get<ListingResponse[]>('/api/marketplace/my-listings').then((r) => r.data),

  createListing: (body: CreateListingRequest) =>
    httpMarketplace.post<ListingResponse>('/api/marketplace/listings', body).then((r) => r.data),

  updateListing: (id: number, body: UpdateListingRequest) =>
    httpMarketplace.put<ListingResponse>(`/api/marketplace/listings/${id}`, body).then((r) => r.data),

  closeListing: (id: number) =>
    httpMarketplace.patch<ListingResponse>(`/api/marketplace/listings/${id}/close`).then((r) => r.data),

  contactListing: (id: number, message: string) =>
    httpMarketplace
      .post<InterestResponse>(`/api/marketplace/listings/${id}/contact`, { message })
      .then((r) => r.data),

  searchBuyRequests: (filters: BuyRequestFilters = {}) =>
    httpMarketplace
      .get<PagedResult<BuyRequestResponse>>('/api/marketplace/buy-requests', { params: filters })
      .then((r) => r.data),

  createBuyRequest: (body: CreateBuyRequestRequest) =>
    httpMarketplace.post<BuyRequestResponse>('/api/marketplace/buy-requests', body).then((r) => r.data),

  getMyInterests: () =>
    httpMarketplace.get<InterestResponse[]>('/api/marketplace/my-interests').then((r) => r.data),

  getMessages: (interestId: number, beforeId?: number) =>
    httpMarketplace
      .get<MessageHistoryResponse>(`/api/marketplace/my-interests/${interestId}/messages`, {
        params: beforeId ? { beforeId } : undefined,
      })
      .then((r) => r.data),

  sendMessage: (interestId: number, body: string) =>
    httpMarketplace
      .post<MessageResponse>(`/api/marketplace/my-interests/${interestId}/messages`, { body })
      .then((r) => r.data),

  getUploadSignature: () =>
    httpMarketplace
      .get<UploadSignatureResponse>('/api/marketplace/uploads/signature')
      .then((r) => r.data),
}
