import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { variantLabel, type VariantOption } from '@/lib/priceVariants'

interface VariantSelectProps {
  options: VariantOption[]
  selectedKey: string
  onChange: (key: string) => void
  className?: string
}

/**
 * Chọn quy cách (khối lượng) của sản phẩm. Chỉ có 1 quy cách thì hiện chữ tĩnh — không cần combobox; có nhiều quy cách
 * (vd. 500g/1kg, hoặc cùng quy cách nhưng khác nguồn) thì hiện combobox để người dùng xem giá từng loại.
 */
export function VariantSelect({ options, selectedKey, onChange, className }: VariantSelectProps) {
  const showSource = new Set(options.map((o) => o.variant.marketName)).size > 1

  if (options.length <= 1) {
    return <span className="text-text-muted">{options[0] ? variantLabel(options[0], false) : '—'}</span>
  }

  return (
    <Select value={selectedKey} onValueChange={onChange}>
      <SelectTrigger className={className ?? 'h-8 w-44'}>
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {options.map((o) => (
          <SelectItem key={o.key} value={o.key}>
            {variantLabel(o, showSource)}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
