import { cn } from '@/lib/utils'

interface IllustrationProps {
  src: string
  alt?: string
  className?: string
}

/** AI-generated illustrations ship as single opaque PNGs (no per-theme recolor like the
 * old unDraw SVGs) — frame them in a fixed cream card so the baked-in background reads as
 * an intentional "illustration card" in both light and dark mode, instead of a mismatched
 * edge against the dark background. */
export function Illustration({ src, alt = '', className }: IllustrationProps) {
  return (
    <div className={cn('inline-flex rounded-2xl bg-[#f8ecd8] p-3 shadow-raised', className)}>
      <img src={src} alt={alt} className="w-full rounded-xl" />
    </div>
  )
}
