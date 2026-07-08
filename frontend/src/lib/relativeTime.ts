const UNITS: [Intl.RelativeTimeFormatUnit, number][] = [
  ['year', 365 * 24 * 60 * 60],
  ['month', 30 * 24 * 60 * 60],
  ['day', 24 * 60 * 60],
  ['hour', 60 * 60],
  ['minute', 60],
]

const relativeFormatter = new Intl.RelativeTimeFormat('vi-VN', { numeric: 'auto' })

export function formatRelativeTime(isoDate: string): string {
  const seconds = Math.round((new Date(isoDate).getTime() - Date.now()) / 1000)
  const abs = Math.abs(seconds)

  if (abs < 60) return 'Vừa đăng'

  for (const [unit, unitSeconds] of UNITS) {
    if (abs >= unitSeconds) {
      return relativeFormatter.format(Math.round(seconds / unitSeconds), unit)
    }
  }

  return relativeFormatter.format(Math.round(seconds / 60), 'minute')
}

const joinDateFormatter = new Intl.DateTimeFormat('vi-VN', { month: 'long', year: 'numeric' })

export function formatJoinedSince(isoDate: string): string {
  return `Tham gia từ ${joinDateFormatter.format(new Date(isoDate))}`
}
