// Mirrors HappyFarmer_Backend DTOs exactly. Keep in sync with:
// - AuthService: Dtos/AuthDtos.cs
// - MarketPriceService: Dtos/MarketPriceDtos.cs
// - MarketplaceService: Dtos/MarketplaceDtos.cs, Services/CloudinarySignatureService.cs
// - AiAdvisoryService: Dtos/ChatDtos.cs

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
  avatarUrl: string | null
}

export interface UpdateProfileRequest {
  fullName?: string
  email?: string
  provinceId?: number
  avatarUrl?: string
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
  farmerAvatarUrl: string | null
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
  buyerAvatarUrl: string | null
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

export type ChatSender = 'User' | 'AI'
export type ChatSessionStatus = 'Active' | 'Ended'

export interface ChatSessionSummaryResponse {
  id: number
  title: string | null
  startedAt: string
  lastActivityAt: string
  status: ChatSessionStatus
}

export interface CreateChatSessionResponse {
  sessionId: number
  startedAt: string
}

export interface ChatMessageDto {
  id: number
  sender: ChatSender
  content: string
  createdAt: string
}

export interface SendChatMessageResponse {
  sessionId: number
  reply: string
  timestamp: string
}

export interface WeatherSummaryDto {
  avgTempC: number
  totalRainfallMm: number
}

export interface CreateHarvestPredictionRequest {
  cropType: string
  plantingDate: string
  location: string
}

export interface DailyForecastSummaryDto {
  date: string
  avgTempC: number
  minTempC: number
  maxTempC: number
  totalRainfallMm: number
  popPercent: number
  weatherId: number
  weatherDescription: string
}

export interface HarvestPredictionResponse {
  id: number
  cropType: string
  plantingDate: string
  location: string
  recommendedStartDate: string
  recommendedEndDate: string
  confidenceLevel: string
  riskFactors: string[]
  reasoning: string
  weatherSummary: WeatherSummaryDto | null
  usedVerifiedCropProfile: boolean
  weatherDataIncluded: boolean
  transparencyNote: string
  createdAt: string
}

export interface HarvestPredictionSummaryDto {
  id: number
  cropType: string
  location: string
  plantingDate: string
  recommendedStartDate: string
  recommendedEndDate: string
  confidenceLevel: string
  createdAt: string
}

export interface CreateDiseaseDetectionRequest {
  imageUrl: string
  cropTypeHint?: string
  note?: string
}

export interface DiseaseDetectionResponse {
  id: number
  imageUrl: string
  isHealthy: boolean
  identifiedCropType: string
  diseaseName: string | null
  confidenceScore: number
  severity: string | null
  description: string
  treatmentOrganic: string[]
  treatmentChemical: string[]
  preventionTips: string[]
  recommendedActions: string[]
  createdAt: string
}

export interface DiseaseDetectionSummaryDto {
  id: number
  imageUrl: string
  identifiedCropType: string
  isHealthy: boolean
  diseaseName: string | null
  severity: string | null
  createdAt: string
}
