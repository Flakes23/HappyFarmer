// Mirrors HappyFarmer_Backend DTOs exactly. Keep in sync with:
// - AuthService: Dtos/AuthDtos.cs
// - MarketPriceService: Dtos/MarketPriceDtos.cs

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

export interface ProductResponse {
  id: number
  nameVi: string
  category: string | null
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
}

export interface PriceHistoryPoint {
  effectiveDate: string
  price: number
}

export interface TrendingItem {
  productId: number
  productName: string
  regionId: number
  regionName: string
  currentPrice: number
  previousPrice: number | null
  changePercent: number | null
}
