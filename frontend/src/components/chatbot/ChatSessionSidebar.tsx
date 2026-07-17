import { Plus, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/shared/EmptyState'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { useChatSessions } from '@/hooks/queries/useChatSessions'
import { useCreateChatSession } from '@/hooks/mutations/useCreateChatSession'
import { useDeleteChatSession } from '@/hooks/mutations/useDeleteChatSession'
import { extractApiErrorMessage } from '@/api/authApi'
import { cn } from '@/lib/utils'

interface ChatSessionSidebarProps {
  selectedSessionId: number | undefined
  onSelect: (id: number | undefined) => void
}

export function ChatSessionSidebar({ selectedSessionId, onSelect }: ChatSessionSidebarProps) {
  const sessions = useChatSessions()
  const createSession = useCreateChatSession()
  const deleteSession = useDeleteChatSession()

  function handleCreate() {
    createSession.mutate(undefined, {
      onSuccess: (res) => onSelect(res.sessionId),
      onError: (err) => toast.error(extractApiErrorMessage(err, 'Không thể tạo cuộc trò chuyện mới.')),
    })
  }

  function handleDelete(id: number) {
    deleteSession.mutate(id, {
      onSuccess: () => {
        if (selectedSessionId === id) onSelect(undefined)
      },
      onError: (err) => toast.error(extractApiErrorMessage(err, 'Không thể xoá cuộc trò chuyện.')),
    })
  }

  return (
    <div className="flex h-full flex-col rounded-lg border border-border bg-surface">
      <div className="border-b border-border p-3">
        <Button className="w-full" onClick={handleCreate} disabled={createSession.isPending}>
          <Plus className="h-4 w-4" />
          Chat mới
        </Button>
      </div>

      <div className="flex-1 space-y-1 overflow-y-auto p-2">
        {sessions.isLoading ? (
          <div className="space-y-2 p-1">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
        ) : !sessions.data || sessions.data.length === 0 ? (
          <EmptyState title="Chưa có cuộc trò chuyện nào" description="Bấm 'Chat mới' để bắt đầu." />
        ) : (
          sessions.data.map((s) => (
            <button
              key={s.id}
              type="button"
              onClick={() => onSelect(s.id)}
              className={cn(
                'group flex w-full items-center justify-between gap-2 rounded-md px-3 py-2 text-left text-body-sm transition-colors hover:bg-secondary',
                s.id === selectedSessionId && 'bg-secondary'
              )}
            >
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium text-text">{s.title ?? 'Cuộc trò chuyện mới'}</p>
                <p className="text-xs text-text-muted">
                  {new Date(s.lastActivityAt).toLocaleString('vi-VN')}
                </p>
              </div>

              <ConfirmDialog
                title="Xoá cuộc trò chuyện?"
                description="Toàn bộ tin nhắn trong cuộc trò chuyện này sẽ bị xoá vĩnh viễn."
                confirmLabel="Xoá"
                variant="destructive"
                onConfirm={() => handleDelete(s.id)}
              >
                <span
                  role="button"
                  aria-label="Xoá cuộc trò chuyện"
                  onClick={(e) => e.stopPropagation()}
                  className="shrink-0 rounded-md p-1.5 text-text-muted opacity-0 hover:bg-error hover:text-white group-hover:opacity-100"
                >
                  <Trash2 className="h-4 w-4" />
                </span>
              </ConfirmDialog>
            </button>
          ))
        )}
      </div>
    </div>
  )
}
