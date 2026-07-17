import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Illustration } from '@/components/shared/Illustration'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import notFoundIllustration from '@/assets/illustrations/illustration-not-found.webp'

export function NotFoundPage() {
  useDocumentTitle('Không tìm thấy trang — HappyFarmer')
  return (
    <div className="flex flex-col items-center gap-4 py-16 text-center">
      <Illustration src={notFoundIllustration} className="h-40 w-40" />
      <h1 className="text-h1 text-text">404 — Không tìm thấy trang</h1>
      <p className="text-body text-text-muted">Trang bạn tìm kiếm không tồn tại.</p>
      <Button asChild>
        <Link to="/">Về trang chủ</Link>
      </Button>
    </div>
  )
}
