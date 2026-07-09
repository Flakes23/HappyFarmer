// Mirrors HappyFarmer_Backend DTOs exactly. Keep in sync with:
// - AuthService: Dtos/AuthDtos.cs
// - MarketPriceService: Dtos/MarketPriceDtos.cs
// - MarketplaceService: Dtos/MarketplaceDtos.cs, Services/CloudinarySignatureService.cs

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export type UserRole = 'Farmer' | 'Buyer' | 'Admin'

export interface UserResponse {
  id: number
  phoneNumber: string | null
  email: string | null
  fullName: string
  role: UserRole
  provinceId: number | null
  isActive: boolean
  createdAt: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  user: UserResponse
}

export interface RegisterRequest {
  phoneNumber: string
  email?: string
  password: string
  fullName: string
  role: 'Farmer' | 'Buyer'
  provinceId?: number
}

export interface LoginRequest {
  phoneNumber: string
  password: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export type PriceSource = 'Crawled' | 'Community' | 'Admin'

export interface CategoryResponse {
  id: number
  name: string
}

export interface SubCategoryResponse {
  id: number
  categoryId: number
  categoryName: string
  name: string
}

export interface ProductResponse {
  id: number
  nameVi: string
  subCategoryId: number
  subCategoryName: string
  categoryId: number
  categoryName: string
  unit: string
  imageUrl: string | null
}

export interface RegionResponse {
  id: number
  provinceName: string
  marketName: string
  lat: number | null
  lon: number | null
}

export interface PriceResponse {
  productId: number
  productName: string
  regionId: number
  regionName: string
  price: number
  source: PriceSource
  effectiveDate: string
  unit: string | null
}

export interface PriceHistoryPoint {
  effectiveDate: string
  price: number
  unit: string | null
}

export interface TrendingItem {
  productId: number
  productName: string
  regionId: number
  regionName: string
  currentPrice: number
  previousPrice: number | null
  changePercent: number | null
  unit: string | null
}

export type ListingStatus = 'Active' | 'Sold' | 'Closed' | 'Expired'
export type BuyRequestStatus = 'Active' | 'Closed'
export type InterestStatus = 'Pending' | 'Responded'

export interface ListingResponse {
  id: number
  farmerId: number
  farmerName: string | null
  farmerJoinedAt: string | null
  farmerActiveListingCount: number
  productId: number
  quantity: number
  unit: string
  pricePerUnit: number
  regionId: number
  description: string | null
  status: ListingStatus
  createdAt: string
  expiresAt: string | null
  imageUrls: string[]
}

export interface CreateListingRequest {
  productId: number
  quantity: number
  unit: string
  pricePerUnit: number
  regionId: number
  description?: string
  expiresAt?: string
  imageUrls?: string[]
}

export interface UpdateListingRequest {
  quantity?: number
  pricePerUnit?: number
  description?: string
  expiresAt?: string
}

export interface BuyRequestResponse {
  id: number
  buyerId: number
  buyerName: string | null
  buyerJoinedAt: string | null
  buyerActiveBuyRequestCount: number
  productId: number
  desiredQuantity: number
  unit: string
  regionId: number
  maxPricePerUnit: number | null
  description: string | null
  status: BuyRequestStatus
  createdAt: string
}

export interface CreateBuyRequestRequest {
  productId: number
  desiredQuantity: number
  unit: string
  regionId: number
  maxPricePerUnit?: number
  description?: string
}

export interface InterestListingSummary {
  productId: number
  quantity: number
  unit: string
  pricePerUnit: number
  status: ListingStatus
  imageUrl: string | null
}

export interface InterestBuyRequestSummary {
  productId: number
  desiredQuantity: number
  unit: string
  maxPricePerUnit: number | null
  status: BuyRequestStatus
}

export interface InterestLastMessage {
  body: string
  senderUserId: number
  createdAt: string
}

export interface InterestResponse {
  id: number
  listingId: number | null
  listing: InterestListingSummary | null
  buyRequestId: number | null
  buyRequest: InterestBuyRequestSummary | null
  initiatorUserId: number
  targetUserId: number
  message: string | null
  status: InterestStatus
  createdAt: string
  hasUnread: boolean
  lastMessage: InterestLastMessage | null
}

export interface UnreadCountResponse {
  count: number
}

export interface MessageResponse {
  id: number
  interestId: number
  senderUserId: number
  body: string
  createdAt: string
  readAt: string | null
}

export interface MessageHistoryResponse {
  messages: MessageResponse[]
  hasMore: boolean
}

export interface UploadSignatureResponse {
  signature: string
  timestamp: number
  apiKey: string
  cloudName: string
  folder: string
}
