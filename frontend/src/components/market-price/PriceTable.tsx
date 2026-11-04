import { useState } from 'react'
import { Link } from 'react-router-dom'
import { LineChart } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { SourceBadge } from '@/components/market-price/SourceBadge'
import { PriceTrendBadge } from '@/components/market-price/PriceTrendBadge'
import { VariantSelect } from '@/components/market-price/VariantSelect'
import { EmptyState } from '@/components/shared/EmptyState'
import { TableRowsSkeleton } from '@/components/shared/Skeletons'
import { useTrending } from '@/hooks/queries/useTrending'
import { buildVariantOptions } from '@/lib/priceVariants'
import { sourceLabel } from '@/lib/regionLabels'
import type { PriceGroup } from '@/api/types'
import emptyPricesIllustration from '@/assets/illustrations/illustration-empty-prices.webp'

interface PriceTableProps {
  groups: PriceGroup[] | undefined
  isLoading: boolean
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

type TrendLookup = (productId: number, regionId: number) => number | null

/** Quy cách đang chọn của một sản phẩm: mặc định quy cách đầu tiên; đổi bằng combobox khi có nhiều quy cách. */
function useSelectedVariant(group: PriceGroup) {
  const options = buildVariantOptions(group.variants)
  const [selectedKey, setSelectedKey] = useState<string | undefined>(undefined)
  const selected = options.find((o) => o.key === selectedKey) ?? options[0]
  return { options, selected, setSelectedKey }
}

export function PriceTable({ groups, isLoading }: PriceTableProps) {
  // Cùng queryKey ['trending'] với TrendingList — React Query dedupe, không gọi thêm request nếu đã có cache.
  // Dữ liệu giá không có sẵn xu hướng (chỉ TrendingItem có), nên join phía client theo productId/regionId.
  const trending = useTrending()

  if (isLoading) {
    return <TableRowsSkeleton count={10} />
  }

  if (!groups || groups.length === 0) {
    return (
      <EmptyState
        icon={LineChart}
        illustration={emptyPricesIllustration}
        title="Không có dữ liệu giá phù hợp"
        description="Thử thay đổi bộ lọc sản phẩm hoặc tỉnh thành."
      />
    )
  }

  const trendByKey = new Map(trending.data?.map((t) => [`${t.productId}-${t.regionId}`, t.changePercent]))
  const trendOf: TrendLookup = (productId, regionId) => trendByKey.get(`${productId}-${regionId}`) ?? null

  return (
    <>
      <div className="hidden max-h-[560px] overflow-y-auto rounded-lg border border-border sm:block">
        <Table>
          <TableHeader className="sticky top-0 z-10 bg-card">
            <TableRow>
              <TableHead>Sản phẩm</TableHead>
              <TableHead>Giá</TableHead>
              <TableHead>Đơn vị tính</TableHead>
              <TableHead>Nguồn</TableHead>
              <TableHead>Nơi tham khảo</TableHead>
              <TableHead>Ngày</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {groups.map((group) => (
              <DesktopRow key={group.productId} group={group} trendOf={trendOf} />
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="space-y-3 sm:hidden">
        {groups.map((group) => (
          <MobileCard key={group.productId} group={group} trendOf={trendOf} />
        ))}
      </div>
    </>
  )
}

function DesktopRow({ group, trendOf }: { group: PriceGroup; trendOf: TrendLookup }) {
  const { options, selected, setSelectedKey } = useSelectedVariant(group)
  if (!selected) return null
  const v = selected.variant

  return (
    <TableRow className="even:bg-muted/20">
      <TableCell>
        <Link to={`/prices/${group.productId}`} className="text-primary hover:underline">
          {group.productName}
        </Link>
      </TableCell>
      <TableCell>
        <p className="font-medium">{currencyFormatter.format(v.price)}</p>
        <PriceTrendBadge changePercent={trendOf(group.productId, v.regionId)} />
      </TableCell>
      <TableCell>
        <VariantSelect options={options} selectedKey={selected.key} onChange={setSelectedKey} />
      </TableCell>
      <TableCell>
        <SourceBadge source={v.source} />
      </TableCell>
      <TableCell>{sourceLabel(v.marketName)}</TableCell>
      <TableCell className="text-text-muted">{new Date(v.effectiveDate).toLocaleDateString('vi-VN')}</TableCell>
    </TableRow>
  )
}

function MobileCard({ group, trendOf }: { group: PriceGroup; trendOf: TrendLookup }) {
  const { options, selected, setSelectedKey } = useSelectedVariant(group)
  if (!selected) return null
  const v = selected.variant

  return (
    <Card className="transition-shadow hover:shadow-raised">
      <CardContent className="space-y-2 p-4">
        <div className="flex items-start justify-between gap-2">
          <Link to={`/prices/${group.productId}`} className="font-medium text-primary hover:underline">
            {group.productName}
          </Link>
          <SourceBadge source={v.source} />
        </div>
        <VariantSelect options={options} selectedKey={selected.key} onChange={setSelectedKey} className="h-8 w-full" />
        <p className="text-body-sm text-text-muted">{sourceLabel(v.marketName)}</p>
        <div className="flex items-center gap-2">
          <p className="font-semibold text-text">{currencyFormatter.format(v.price)}</p>
          <PriceTrendBadge changePercent={trendOf(group.productId, v.regionId)} />
        </div>
        <p className="text-xs text-text-muted">{new Date(v.effectiveDate).toLocaleDateString('vi-VN')}</p>
      </CardContent>
    </Card>
  )
}
