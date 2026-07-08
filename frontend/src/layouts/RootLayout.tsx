import { Outlet } from 'react-router-dom'
import { Toaster } from '@/components/ui/sonner'
import { Navbar } from '@/components/layout/Navbar'
import { Footer } from '@/components/layout/Footer'
import { ChatConnectionProvider } from '@/providers/ChatConnectionProvider'

export function RootLayout() {
  return (
    <ChatConnectionProvider>
      <div className="flex min-h-screen flex-col bg-background text-text">
        <Navbar />
        <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
          <Outlet />
        </main>
        <Footer />
        <Toaster />
      </div>
    </ChatConnectionProvider>
  )
}
