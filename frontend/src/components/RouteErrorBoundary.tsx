import { Link, isRouteErrorResponse, useRouteError } from 'react-router-dom'
import { AlertTriangle } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function RouteErrorBoundary() {
  const error = useRouteError()

  const message = isRouteErrorResponse(error)
    ? error.statusText || 'Đã có lỗi xảy ra.'
    : error instanceof Error
      ? error.message
      : 'Đã có lỗi không xác định xảy ra.'

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-background px-4 text-center">
      <span className="flex h-14 w-14 items-center justify-center rounded-full bg-secondary text-error">
        <AlertTriangle className="h-7 w-7" />
      </span>
      <h1 className="text-2xl font-semibold text-text">Đã có lỗi xảy ra</h1>
      <p className="max-w-sm text-text-muted">{message}</p>
      <div className="flex gap-3">
        <Button variant="outline" onClick={() => window.location.reload()}>
          Thử lại
        </Button>
        <Button asChild>
          <Link to="/">Về trang chủ</Link>
        </Button>
      </div>
    </div>
  )
}
