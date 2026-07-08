import type { ReactNode } from 'react'
import { useEffect } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { toast } from 'sonner'
import { useAuthStore } from '@/store/authStore'
import type { UserRole } from '@/api/types'

interface RequireAuthProps {
  children: ReactNode
  role?: UserRole
}

/** Chặn route theo đăng nhập (bắt buộc luôn) và role (tuỳ chọn) — chưa đăng nhập redirect sang /login. */
export function RequireAuth({ children, role }: RequireAuthProps) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)
  const location = useLocation()

  useEffect(() => {
    if (!isAuthenticated) {
      toast.info('Vui lòng đăng nhập để tiếp tục.')
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticated])

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (role && user?.role !== role) {
    return (
      <div className="rounded-lg border border-border bg-surface p-8 text-center text-text-muted">
        Bạn không có quyền truy cập trang này.
      </div>
    )
  }

  return <>{children}</>
}
