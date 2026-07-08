import { Link } from 'react-router-dom'
import { Sprout } from 'lucide-react'

export function Footer() {
  const year = new Date().getFullYear()

  return (
    <footer className="border-t border-border bg-surface">
      <div className="mx-auto flex max-w-5xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-center sm:justify-between">
        <Link to="/" className="flex items-center gap-2 font-semibold text-primary">
          <Sprout className="h-5 w-5" />
          HappyFarmer
        </Link>

        <nav className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-text-muted">
          <Link to="/prices" className="hover:text-primary">
            Giá nông sản
          </Link>
          <Link to="/marketplace" className="hover:text-primary">
            Chợ nông sản
          </Link>
        </nav>

        <p className="text-xs text-text-muted">© {year} HappyFarmer. Đồng hành cùng nông dân Việt.</p>
      </div>
    </footer>
  )
}
