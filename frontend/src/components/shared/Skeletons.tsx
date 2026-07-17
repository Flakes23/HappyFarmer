import { Skeleton } from '@/components/ui/skeleton'

export function CardGridSkeleton({ count = 4 }: { count?: number }) {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {Array.from({ length: count }).map((_, i) => (
        <Skeleton key={i} className="h-28 w-full" />
      ))}
    </div>
  )
}

export function ListSkeleton({ count = 3, rowHeight = 'h-20' }: { count?: number; rowHeight?: string }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <Skeleton key={i} className={`w-full ${rowHeight}`} />
      ))}
    </div>
  )
}

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
