import { Link } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { useUnreadInterestsCount } from '@/hooks/queries/useUnreadInterestsCount'
import { cn } from '@/lib/utils'

interface NavLinksProps {
  orientation?: 'horizontal' | 'vertical'
  onNavigate?: () => void
}

/** Shared link list for Navbar (desktop, horizontal) and MobileNav (Sheet, vertical) — same links, different container/badge styling per orientation. */
export function NavLinks({ orientation = 'horizontal', onNavigate }: NavLinksProps) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const unreadCount = useUnreadInterestsCount()
  const isVertical = orientation === 'vertical'

  const linkClassName = isVertical ? 'text-body text-text hover:text-primary' : 'text-body-sm text-text hover:text-primary'

  return (
    <nav className={isVertical ? 'flex flex-col gap-4' : 'flex items-center gap-3'}>
      <Link to="/prices" className={linkClassName} onClick={onNavigate}>
        Giá nông sản
      </Link>
      <Link to="/marketplace" className={linkClassName} onClick={onNavigate}>
        Chợ nông sản
      </Link>

      {isAuthenticated ? (
        <Link
          to="/marketplace/my-interests"
          className={cn(linkClassName, isVertical ? 'flex items-center gap-2' : 'relative')}
          onClick={onNavigate}
        >
          Liên hệ của tôi
          {unreadCount.data && unreadCount.data.count > 0 ? (
            <span
              className={cn(
                'flex items-center justify-center rounded-full bg-error font-semibold text-white',
                isVertical ? 'h-5 min-w-5 px-1 text-xs' : 'absolute -right-3 -top-2 h-4 min-w-4 px-1 text-[10px]',
              )}
            >
              {unreadCount.data.count > 9 ? '9+' : unreadCount.data.count}
            </span>
          ) : null}
        </Link>
      ) : null}

      {isAuthenticated ? (
        <Link to="/tu-van-ai" className={linkClassName} onClick={onNavigate}>
          Tư vấn AI
        </Link>
      ) : null}
    </nav>
  )
}
