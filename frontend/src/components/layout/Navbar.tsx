import { Link } from 'react-router-dom'
import { LogOut, User } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { MobileNav } from '@/components/layout/MobileNav'
import { NavLinks } from '@/components/layout/NavLinks'
import { ThemeToggle } from '@/components/layout/ThemeToggle'
import { useAuthStore } from '@/store/authStore'
import { useLogout } from '@/hooks/mutations/useLogout'
import { getInitial } from '@/lib/utils'
import logo from '@/assets/logo.svg'

export function Navbar() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()

  return (
    <header className="border-b border-border bg-surface">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
        <Link to="/" className="flex items-center gap-2 font-semibold text-primary">
          <img src={logo} alt="" className="h-6 w-6" />
          HappyFarmer
        </Link>

        <div className="hidden items-center gap-3 md:flex">
          <NavLinks orientation="horizontal" />

          <ThemeToggle />

          {isAuthenticated && user ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button className="flex items-center gap-2 rounded-full text-sm text-text-muted hover:text-primary">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={user.avatarUrl ?? undefined} alt={user.fullName} />
                    <AvatarFallback className="bg-primary-light text-white">
                      {getInitial(user.fullName)}
                    </AvatarFallback>
                  </Avatar>
                  {user.fullName}
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem asChild>
                  <Link to="/profile">
                    <User className="h-4 w-4" />
                    Hồ sơ của tôi
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => logout.mutate()} disabled={logout.isPending}>
                  <LogOut className="h-4 w-4" />
                  {logout.isPending ? 'Đang đăng xuất...' : 'Đăng xuất'}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
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
        </div>

        <MobileNav />
      </div>
    </header>
  )
}
