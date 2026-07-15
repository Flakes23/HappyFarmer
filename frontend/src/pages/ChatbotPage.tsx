import { useEffect, useState } from 'react'
import { PanelLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet'
import { EmptyState } from '@/components/shared/EmptyState'
import { ChatSessionSidebar } from '@/components/chatbot/ChatSessionSidebar'
import { ChatMessageList } from '@/components/chatbot/ChatMessageList'
import { ChatInput } from '@/components/chatbot/ChatInput'

export function ChatbotPage() {
  const [selectedSessionId, setSelectedSessionId] = useState<number | undefined>(undefined)
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [pendingMessage, setPendingMessage] = useState<string | null>(null)

  // Đổi phiên chat thì bong bóng "đang gửi" của phiên cũ (nếu còn) không được rơi rớt sang phiên mới.
  useEffect(() => {
    setPendingMessage(null)
  }, [selectedSessionId])

  function selectAndClose(id: number | undefined) {
    setSelectedSessionId(id)
    setSidebarOpen(false)
  }

  return (
    <div className="flex h-[75vh] gap-4">
      <aside className="hidden w-72 flex-col md:flex">
        <ChatSessionSidebar selectedSessionId={selectedSessionId} onSelect={setSelectedSessionId} />
      </aside>

      <Sheet open={sidebarOpen} onOpenChange={setSidebarOpen}>
        <SheetTrigger asChild>
          <Button variant="outline" size="icon" className="md:hidden" aria-label="Danh sách hội thoại">
            <PanelLeft className="h-4 w-4" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="flex w-4/5 flex-col">
          <SheetHeader>
            <SheetTitle>Lịch sử trò chuyện</SheetTitle>
          </SheetHeader>
          <div className="flex-1 overflow-hidden py-2">
            <ChatSessionSidebar selectedSessionId={selectedSessionId} onSelect={selectAndClose} />
          </div>
        </SheetContent>
      </Sheet>

      <section className="flex flex-1 flex-col">
        {selectedSessionId === undefined ? (
          <div className="flex h-full items-center justify-center rounded-lg border border-border bg-surface">
            <EmptyState
              title="Chưa chọn cuộc trò chuyện"
              description="Chọn một cuộc trò chuyện ở danh sách bên trái, hoặc bấm 'Chat mới' để bắt đầu."
            />
          </div>
        ) : (
          <div className="flex h-full flex-col rounded-lg border border-border bg-surface">
            <ChatMessageList sessionId={selectedSessionId} pendingMessage={pendingMessage} />
            <ChatInput sessionId={selectedSessionId} onPendingChange={setPendingMessage} />
          </div>
        )}
      </section>
    </div>
  )
}
