import { Link } from 'react-router-dom'
import { Sprout, LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/store/authStore'
import { useLogout } from '@/hooks/mutations/useLogout'

export function Navbar() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()

  return (
    <header className="border-b border-border bg-surface">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
        <Link to="/" className="flex items-center gap-2 font-semibold text-primary">
          <Sprout className="h-5 w-5" />
          HappyFarmer
        </Link>

        <nav className="flex items-center gap-3">
          <Link to="/prices" className="text-sm text-text hover:text-primary">
            Giá nông sản
          </Link>

          {isAuthenticated && user ? (
            <div className="flex items-center gap-3">
              <span className="text-sm text-text-muted">{user.fullName}</span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => logout.mutate()}
                disabled={logout.isPending}
              >
                <LogOut className="h-4 w-4" />
                Đăng xuất
              </Button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Button variant="ghost" size="sm" asChild>
                <Link to="/login">Đăng nhập</Link>
              </Button>
              <Button size="sm" asChild>
                <Link to="/register">Đăng ký</Link>
              </Button>
            </div>
          )}
        </nav>
      </div>
    </header>
  )
}
