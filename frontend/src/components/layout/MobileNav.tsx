import { useState } from 'react'
import { Link } from 'react-router-dom'
import { LogOut, Menu } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet'
import { ThemeToggle } from '@/components/layout/ThemeToggle'
import { useAuthStore } from '@/store/authStore'
import { useLogout } from '@/hooks/mutations/useLogout'
import { useUnreadInterestsCount } from '@/hooks/queries/useUnreadInterestsCount'

export function MobileNav() {
  const [open, setOpen] = useState(false)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()
  const unreadCount = useUnreadInterestsCount()

  function close() {
    setOpen(false)
  }

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" aria-label="Mở menu" className="md:hidden">
          <Menu className="h-5 w-5" />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="flex w-4/5 flex-col gap-6">
        <SheetHeader>
          <SheetTitle>HappyFarmer</SheetTitle>
        </SheetHeader>

        <nav className="flex flex-col gap-4">
          <Link to="/prices" className="text-base text-text hover:text-primary" onClick={close}>
            Giá nông sản
          </Link>
          <Link to="/marketplace" className="text-base text-text hover:text-primary" onClick={close}>
            Chợ nông sản
          </Link>
          {isAuthenticated ? (
            <Link
              to="/marketplace/my-interests"
              className="flex items-center gap-2 text-base text-text hover:text-primary"
              onClick={close}
            >
              Liên hệ của tôi
              {unreadCount.data && unreadCount.data.count > 0 ? (
                <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-error px-1 text-xs font-semibold text-white">
                  {unreadCount.data.count > 9 ? '9+' : unreadCount.data.count}
                </span>
              ) : null}
            </Link>
          ) : null}
        </nav>

        <div className="flex items-center justify-between">
          <span className="text-sm text-text-muted">Giao diện</span>
          <ThemeToggle />
        </div>

        <div className="mt-auto flex flex-col gap-3 border-t border-border pt-4">
          {isAuthenticated && user ? (
            <>
              <span className="text-sm text-text-muted">{user.fullName}</span>
              <Button variant="outline" asChild>
                <Link to="/profile" onClick={close}>
                  Hồ sơ của tôi
                </Link>
              </Button>
              <Button
                variant="outline"
                onClick={() => {
                  logout.mutate()
                  close()
                }}
                disabled={logout.isPending}
              >
                <LogOut className="h-4 w-4" />
                {logout.isPending ? 'Đang đăng xuất...' : 'Đăng xuất'}
              </Button>
            </>
          ) : (
            <>
              <Button variant="outline" asChild>
                <Link to="/login" onClick={close}>
                  Đăng nhập
                </Link>
              </Button>
              <Button asChild>
                <Link to="/register" onClick={close}>
                  Đăng ký
                </Link>
              </Button>
            </>
          )}
        </div>
      </SheetContent>
    </Sheet>
  )
}
