import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Skeleton } from '@/components/ui/skeleton'
import type { PriceHistoryPoint } from '@/api/types'

interface PriceHistoryChartProps {
  data: PriceHistoryPoint[] | undefined
  isLoading: boolean
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function PriceHistoryChart({ data, isLoading }: PriceHistoryChartProps) {
  if (isLoading) {
    return <Skeleton className="h-72 w-full" />
  }

  if (!data || data.length === 0) {
    return <p className="py-8 text-center text-text-muted">Chưa có dữ liệu lịch sử giá.</p>
  }

  return (
    <div className="h-72 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
          <XAxis dataKey="effectiveDate" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} />
          <YAxis
            tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }}
            tickFormatter={(v: number) => currencyFormatter.format(v)}
            width={90}
          />
          <Tooltip
            formatter={(value) => currencyFormatter.format(Number(value))}
            contentStyle={{ backgroundColor: 'var(--card)', borderColor: 'var(--border)' }}
          />
          <Line type="monotone" dataKey="price" stroke="var(--primary)" strokeWidth={2} dot={false} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  )
}
