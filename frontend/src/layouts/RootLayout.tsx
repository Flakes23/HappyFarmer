import { Outlet } from 'react-router-dom'
import { Toaster } from '@/components/ui/sonner'
import { Navbar } from '@/components/layout/Navbar'

export function RootLayout() {
  return (
    <div className="min-h-screen bg-background text-text">
      <Navbar />
      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
      <Toaster />
    </div>
  )
}
