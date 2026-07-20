import { Skeleton } from '@/components/ui/skeleton'

/** Mirrors PriceTable's row columns (product + price + unit + source + region + date) so real data doesn't reflow the layout on arrival. */
export function TableRowsSkeleton({ count = 5 }: { count?: number }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-center gap-4 rounded-lg border border-border p-3">
          <Skeleton className="h-4 w-4 shrink-0 rounded-full" />
          <Skeleton className="h-4 w-28" />
          <Skeleton className="h-4 w-14" />
          <Skeleton className="ml-auto h-4 w-20" />
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-16" />
        </div>
      ))}
    </div>
  )
}

/** Mirrors WeatherForecastPreview's 6-day success-state grid — replaces a single generic blob so the loading state doesn't read as a mysterious empty box. */
export function WeatherForecastSkeleton() {
  return (
    <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
      {Array.from({ length: 6 }).map((_, i) => (
        <div key={i} className="flex flex-col items-center gap-2 rounded-md bg-surface px-2 py-3 shadow-sm">
          <Skeleton className="h-3 w-12" />
          <Skeleton className="h-7 w-7 rounded-full" />
          <Skeleton className="h-3 w-14" />
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-3 w-10" />
        </div>
      ))}
    </div>
  )
}

/** Alternating-alignment chat bubble shapes for chat-history loading. */
export function ChatMessageSkeleton() {
  return (
    <div className="space-y-2">
      <Skeleton className="h-10 w-2/3" />
      <Skeleton className="ml-auto h-10 w-2/3" />
      <Skeleton className="h-10 w-1/2" />
    </div>
  )
}

/** Mirrors MyListingsPage's card row (title + qty/price line, status badge + action buttons). */
export function ListingRowSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border p-4">
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-40" />
          </div>
          <div className="flex items-center gap-2">
            <Skeleton className="h-6 w-16 rounded-full" />
            <Skeleton className="h-9 w-16" />
            <Skeleton className="h-9 w-16" />
          </div>
        </div>
      ))}
    </div>
  )
}

/** Mirrors MyInterestsPage's card row (label + badge, thumbnail + summary, last message, timestamp). */
export function InterestRowSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="space-y-2 rounded-xl border border-border p-4">
          <div className="flex items-center justify-between gap-2">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-6 w-16 rounded-full" />
          </div>
          <div className="flex items-center gap-3">
            <Skeleton className="h-12 w-12 shrink-0 rounded-md" />
            <div className="min-w-0 flex-1 space-y-1">
              <Skeleton className="h-4 w-2/3" />
              <Skeleton className="h-3 w-1/3" />
            </div>
          </div>
          <Skeleton className="h-3 w-24" />
        </div>
      ))}
    </div>
  )
}

/** Same card shape as InterestRowSkeleton, single instance — for InterestThreadPage's summary header. */
export function ThreadSummarySkeleton() {
  return <InterestRowSkeleton count={1} />
}

/** Mirrors harvest/disease history rows (title + date line, secondary line, status badge). */
export function HistoryRowSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-center justify-between gap-3 rounded-lg border border-border p-3">
          <div className="space-y-2">
            <Skeleton className="h-4 w-36" />
            <Skeleton className="h-3 w-24" />
          </div>
          <Skeleton className="h-6 w-16 rounded-full" />
        </div>
      ))}
    </div>
  )
}

/** Mirrors ListingCard (thumbnail + title/status row + region/quantity/price lines + seller row). */
export function ListingCardSkeleton({ count = 4 }: { count?: number }) {
  return (
    <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex gap-4 rounded-lg border border-border p-4">
          <Skeleton className="h-20 w-20 shrink-0 rounded-md" />
          <div className="min-w-0 flex-1 space-y-2">
            <div className="flex items-center justify-between gap-2">
              <Skeleton className="h-4 w-28" />
              <Skeleton className="h-5 w-14 rounded-full" />
            </div>
            <Skeleton className="h-3 w-20" />
            <Skeleton className="h-3 w-24" />
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-3 w-32" />
          </div>
        </div>
      ))}
    </div>
  )
}

/** Mirrors BuyRequestCard (no thumbnail — title/status row + region/quantity/price/seller lines). */
export function BuyRequestCardSkeleton({ count = 4 }: { count?: number }) {
  return (
    <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="space-y-2 rounded-lg border border-border p-4">
          <div className="flex items-start justify-between gap-2">
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-5 w-14 rounded-full" />
          </div>
          <Skeleton className="h-3 w-20" />
          <Skeleton className="h-3 w-24" />
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-3 w-28" />
        </div>
      ))}
    </div>
  )
}

/** Mirrors TrendingList row (product/region left, price/trend badge right). */
export function TrendingRowSkeleton({ count = 10 }: { count?: number }) {
  return (
    <div className="divide-y divide-border">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-center justify-between py-3">
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-3 w-16" />
          </div>
          <div className="space-y-2 text-right">
            <Skeleton className="ml-auto h-4 w-20" />
            <Skeleton className="ml-auto h-3 w-14" />
          </div>
        </div>
      ))}
    </div>
  )
}
