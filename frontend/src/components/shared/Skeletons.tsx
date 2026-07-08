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
