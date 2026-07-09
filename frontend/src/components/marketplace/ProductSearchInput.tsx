import { useEffect, useState } from 'react'
import { Input } from '@/components/ui/input'
import { useProductSearch } from '@/hooks/queries/useProductSearch'

export function ProductSearchInput({
  onChange,
  placeholder = 'Gõ để tìm sản phẩm...',
}: {
  onChange: (id: number) => void
  placeholder?: string
}) {
  const [query, setQuery] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [selectedName, setSelectedName] = useState<string | null>(null)

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedQuery(query), 300)
    return () => clearTimeout(timeout)
  }, [query])

  const results = useProductSearch(debouncedQuery)
  const showDropdown = query.length > 0 && !selectedName

  return (
    <div className="relative">
      <Input
        placeholder={placeholder}
        value={selectedName ?? query}
        onChange={(e) => {
          setSelectedName(null)
          setQuery(e.target.value)
        }}
      />
      {showDropdown ? (
        <div className="absolute z-10 mt-1 max-h-64 w-full overflow-y-auto rounded-md border border-border bg-card p-1 shadow-md">
          {results.isFetching ? (
            <p className="px-2 py-2 text-sm text-text-muted">Đang tìm...</p>
          ) : !results.data || results.data.length === 0 ? (
            <p className="px-2 py-2 text-sm text-text-muted">Không tìm thấy sản phẩm.</p>
          ) : (
            results.data.map((p) => (
              <button
                key={p.id}
                type="button"
                className="flex w-full items-center rounded-sm px-2 py-1.5 text-left text-sm hover:bg-secondary"
                onClick={() => {
                  onChange(p.id)
                  setSelectedName(p.nameVi)
                  setQuery('')
                }}
              >
                {p.nameVi}
              </button>
            ))
          )}
        </div>
      ) : null}
    </div>
  )
}
