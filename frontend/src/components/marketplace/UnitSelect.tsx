import { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { UNIT_SYMBOLS } from '@/lib/units'

const CUSTOM_VALUE = '__custom__'

export function UnitSelect({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const [isCustom, setIsCustom] = useState(() => value !== '' && !UNIT_SYMBOLS.includes(value))

  if (isCustom) {
    return (
      <div className="space-y-1">
        <Input placeholder="vd: trái, mớ, chục" value={value} onChange={(e) => onChange(e.target.value)} autoFocus />
        <button
          type="button"
          className="text-xs text-primary hover:underline"
          onClick={() => {
            setIsCustom(false)
            onChange(UNIT_SYMBOLS[0])
          }}
        >
          ← Chọn từ danh sách có sẵn
        </button>
      </div>
    )
  }

  return (
    <Select
      value={value || undefined}
      onValueChange={(v) => {
        if (v === CUSTOM_VALUE) {
          setIsCustom(true)
          onChange('')
        } else {
          onChange(v)
        }
      }}
    >
      <SelectTrigger>
        <SelectValue placeholder="Chọn đơn vị" />
      </SelectTrigger>
      <SelectContent>
        {UNIT_SYMBOLS.map((u) => (
          <SelectItem key={u} value={u}>
            {u}
          </SelectItem>
        ))}
        <SelectItem value={CUSTOM_VALUE}>Tự nhập đơn vị khác...</SelectItem>
      </SelectContent>
    </Select>
  )
}
