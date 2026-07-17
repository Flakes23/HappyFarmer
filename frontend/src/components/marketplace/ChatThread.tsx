import { useEffect, useRef, useState } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import { Loader2, Send } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/shared/EmptyState'
import { useMessages } from '@/hooks/queries/useMessages'
import { useSendMessage } from '@/hooks/mutations/useSendMessage'
import { useMarkInterestRead } from '@/hooks/mutations/useMarkInterestRead'
import { useChatConnection } from '@/providers/ChatConnectionProvider'
import { useAuthStore } from '@/store/authStore'
import { cn } from '@/lib/utils'
import type { MessageResponse } from '@/api/types'

export function ChatThread({ interestId }: { interestId: number }) {
  const connection = useChatConnection()
  const messages = useMessages(interestId)
  const sendMessage = useSendMessage(interestId)
  const markRead = useMarkInterestRead()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const [draft, setDraft] = useState('')
  const listRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!connection) return

    function onReceiveMessage(msg: MessageResponse) {
      if (msg.interestId === interestId) markRead.mutate(interestId)
    }

    connection.on('ReceiveMessage', onReceiveMessage)
    return () => {
      connection.off('ReceiveMessage', onReceiveMessage)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [connection, interestId])

  useEffect(() => {
    if (!connection) return

    function join() {
      if (connection?.state === HubConnectionState.Connected) {
        connection.invoke('JoinConversation', interestId).catch(() => {})
      }
    }

    join()
    connection.onreconnected(join)

    return () => {
      connection.off('reconnected', join)
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveConversation', interestId).catch(() => {})
      }
    }
  }, [connection, interestId])

  useEffect(() => {
    listRef.current?.scrollTo({ top: listRef.current.scrollHeight })
  }, [messages.data?.messages.length])

  function handleSend() {
    const body = draft.trim()
    if (!body) return
    sendMessage.mutate(body, { onSuccess: () => setDraft('') })
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  return (
    <div className="flex h-[60vh] flex-col rounded-lg border border-border bg-surface">
      <div ref={listRef} className="flex-1 space-y-2 overflow-y-auto p-4">
        {messages.isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-10 w-2/3" />
            <Skeleton className="ml-auto h-10 w-2/3" />
            <Skeleton className="h-10 w-1/2" />
          </div>
        ) : !messages.data || messages.data.messages.length === 0 ? (
          <EmptyState title="Chưa có tin nhắn nào" description="Gửi lời nhắn đầu tiên để bắt đầu trò chuyện." />
        ) : (
          messages.data.messages.map((m) => {
            const isMine = m.senderUserId === currentUserId
            return (
              <div key={m.id} className={cn('flex', isMine ? 'justify-end' : 'justify-start')}>
                <div
                  className={cn(
                    'max-w-[75%] rounded-lg px-3 py-2 text-body-sm',
                    isMine ? 'bg-primary text-primary-foreground' : 'bg-secondary text-secondary-foreground'
                  )}
                >
                  <p className="whitespace-pre-wrap break-words">{m.body}</p>
                  <p
                    className={cn(
                      'mt-1 text-[10px]',
                      isMine ? 'text-primary-foreground/70' : 'text-text-muted'
                    )}
                  >
                    {new Date(m.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                  </p>
                </div>
              </div>
            )
          })
        )}
      </div>

      <div className="flex items-end gap-2 border-t border-border p-3">
        <Textarea
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Nhập tin nhắn..."
          rows={1}
          className="min-h-9 flex-1 resize-none"
        />
        <Button
          type="button"
          size="icon"
          aria-label="Gửi tin nhắn"
          onClick={handleSend}
          disabled={sendMessage.isPending || !draft.trim()}
        >
          {sendMessage.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
        </Button>
      </div>
    </div>
  )
}
