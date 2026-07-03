import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'

export function NotFoundPage() {
  return (
    <div className="flex flex-col items-center gap-4 py-16 text-center">
      <h1 className="text-3xl font-semibold text-text">404 — Không tìm thấy trang</h1>
      <p className="text-text-muted">Trang bạn tìm kiếm không tồn tại.</p>
      <Button asChild>
        <Link to="/">Về trang chủ</Link>
      </Button>
    </div>
  )
}
