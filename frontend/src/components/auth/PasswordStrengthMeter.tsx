import { cn } from '@/lib/utils'

function computeStrength(value: string): number {
  if (!value) return 0
  let score = 0
  if (value.length >= 8) score++
  if (value.length >= 12) score++
  if (/[a-z]/.test(value) && /[A-Z]/.test(value)) score++
  if (/\d/.test(value)) score++
  if (/[^a-zA-Z0-9]/.test(value)) score++
  return Math.min(score, 4)
}

const LABELS = ['Rất yếu', 'Yếu', 'Trung bình', 'Khá', 'Mạnh']
const COLORS = ['bg-error', 'bg-error', 'bg-accent', 'bg-accent', 'bg-success']

export function PasswordStrengthMeter({ value }: { value: string }) {
  if (!value) return null

  const strength = computeStrength(value)

  return (
    <div className="space-y-1">
      <div className="flex gap-1">
        {Array.from({ length: 4 }).map((_, i) => (
          <span
            key={i}
            className={cn('h-1.5 flex-1 rounded-full bg-secondary', i < strength && COLORS[strength])}
          />
        ))}
      </div>
      <p className="text-xs text-text-muted">Độ mạnh mật khẩu: {LABELS[strength]}</p>
    </div>
  )
}
