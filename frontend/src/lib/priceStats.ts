import type { PriceHistoryPoint } from '@/api/types'

export interface PriceStats {
  avg: number
  high: number
  low: number
}

export function computePriceStats(points: PriceHistoryPoint[] | undefined): PriceStats | null {
  if (!points || points.length === 0) return null

  const prices = points.map((p) => p.price)
  const avg = prices.reduce((sum, p) => sum + p, 0) / prices.length

  return {
    avg,
    high: Math.max(...prices),
    low: Math.min(...prices),
  }
}
