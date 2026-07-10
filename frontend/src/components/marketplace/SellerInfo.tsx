import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar'
import { cn, getInitial } from '@/lib/utils'
import { formatJoinedSince } from '@/lib/relativeTime'

export function SellerInfo({
  name,
  avatarUrl,
  joinedAt,
  otherActiveCount,
  otherActiveLabel,
  className,
}: {
  name: string | null
  avatarUrl?: string | null
  joinedAt: string | null
  otherActiveCount: number
  otherActiveLabel: string
  className?: string
}) {
  if (!name) return null

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Avatar className="h-5 w-5 shrink-0">
        <AvatarImage src={avatarUrl ?? undefined} alt={name} />
        <AvatarFallback className="bg-primary-light text-[10px] font-semibold text-white">
          {getInitial(name)}
        </AvatarFallback>
      </Avatar>
      <span className="min-w-0 truncate text-xs text-text-muted">
        <span className="font-medium text-text">{name}</span>
        {joinedAt ? <> · {formatJoinedSince(joinedAt)}</> : null}
      </span>
      {otherActiveCount > 0 ? (
        <span className="ml-auto shrink-0 whitespace-nowrap rounded-full bg-secondary px-2 py-0.5 text-[11px] text-text-muted">
          {otherActiveCount} {otherActiveLabel}
        </span>
      ) : null}
    </div>
  )
}
