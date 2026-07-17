import type { LucideIcon } from 'lucide-react'
import { Inbox } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Illustration } from '@/components/shared/Illustration'
import { cn } from '@/lib/utils'

interface EmptyStateProps {
  icon?: LucideIcon
  /** Custom illustration (see frontend/src/assets/illustrations/) — takes over from `icon` when provided. */
  illustration?: string
  title: string
  description?: string
  actionLabel?: string
  onAction?: () => void
  className?: string
}

export function EmptyState({
  icon: Icon = Inbox,
  illustration,
  title,
  description,
  actionLabel,
  onAction,
  className,
}: EmptyStateProps) {
  return (
    <div className={cn('flex flex-col items-center gap-3 py-12 text-center', className)}>
      {illustration ? (
        <Illustration src={illustration} className="h-28 w-28" />
      ) : (
        <span className="flex h-12 w-12 items-center justify-center rounded-full bg-secondary text-text-muted">
          <Icon className="h-6 w-6" />
        </span>
      )}
      <p className="font-medium text-text">{title}</p>
      {description ? <p className="max-w-sm text-sm text-text-muted">{description}</p> : null}
      {actionLabel && onAction ? (
        <Button size="sm" onClick={onAction} className="mt-2">
          {actionLabel}
        </Button>
      ) : null}
    </div>
  )
}
