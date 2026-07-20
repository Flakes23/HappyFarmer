import { cn } from "@/lib/utils"

function Skeleton({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        "relative overflow-hidden rounded-md bg-[var(--skeleton-base)]",
        "after:absolute after:inset-0 after:animate-shimmer after:bg-[linear-gradient(90deg,transparent,var(--skeleton-highlight),transparent)] after:content-['']",
        className
      )}
      {...props}
    />
  )
}

export { Skeleton }
