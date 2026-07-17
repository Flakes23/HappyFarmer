import { useEffect, useRef } from 'react'
import Markdown from 'react-markdown'
import { Bot } from 'lucide-react'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Skeleton } from '@/components/ui/skeleton'
import { ChatCardList } from '@/components/chatbot/cards/ChatCardList'
import { useChatHistory } from '@/hooks/queries/useChatHistory'
import { cn } from '@/lib/utils'

const GREETING = 'Xin chào! Tôi có thể giúp gì cho việc canh tác của bạn?'

interface ChatMessageListProps {
  sessionId: number
  pendingMessage: string | null
}

export function ChatMessageList({ sessionId, pendingMessage }: ChatMessageListProps) {
  const history = useChatHistory(sessionId)
  const listRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    listRef.current?.scrollTo({ top: listRef.current.scrollHeight })
  }, [history.data?.length, pendingMessage])

  return (
    <div ref={listRef} className="flex-1 space-y-2 overflow-y-auto p-4">
      {history.isLoading ? (
        <div className="space-y-2">
          <Skeleton className="h-10 w-2/3" />
          <Skeleton className="ml-auto h-10 w-2/3" />
          <Skeleton className="h-10 w-1/2" />
        </div>
      ) : (!history.data || history.data.length === 0) && !pendingMessage ? (
        <div className="flex justify-start gap-2">
          <BotAvatar />
          <div className="max-w-[75%] rounded-lg bg-secondary px-3 py-2 text-sm text-secondary-foreground">
            {GREETING}
          </div>
        </div>
      ) : (
        <>
          {history.data?.map((m) => {
            const isMine = m.sender === 'User'
            return (
              <div key={m.id} className={cn('flex gap-2', isMine ? 'justify-end' : 'items-end justify-start')}>
                {isMine ? null : <BotAvatar />}
                <div
                  className={cn(
                    'max-w-[75%] rounded-lg px-3 py-2 text-sm',
                    isMine ? 'bg-primary text-primary-foreground' : 'bg-secondary text-secondary-foreground'
                  )}
                >
                  {isMine ? (
                    <p className="whitespace-pre-wrap break-words">{m.content}</p>
                  ) : (
                    <Markdown
                      components={{
                        p: (props) => <p className="mb-2 last:mb-0" {...props} />,
                        strong: (props) => <strong className="font-semibold" {...props} />,
                        ul: (props) => <ul className="mb-2 list-disc space-y-1 pl-4 last:mb-0" {...props} />,
                        ol: (props) => <ol className="mb-2 list-decimal space-y-1 pl-4 last:mb-0" {...props} />,
                        li: (props) => <li {...props} />,
                        a: (props) => (
                          <a
                            {...props}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-primary underline underline-offset-2 hover:text-primary-light"
                          />
                        ),
                      }}
                    >
                      {m.content}
                    </Markdown>
                  )}
                  <p
                    className={cn(
                      'mt-1 text-[10px]',
                      isMine ? 'text-primary-foreground/70' : 'text-text-muted'
                    )}
                  >
                    {new Date(m.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                  </p>

                  {!isMine && m.cards && m.cards.length > 0 ? <ChatCardList cards={m.cards} /> : null}
                </div>
              </div>
            )
          })}

          {pendingMessage ? (
            <>
              {/* Bong bóng tin nhắn vừa gửi — hiện ngay lập tức, không đợi phản hồi AI */}
              <div className="flex justify-end gap-2">
                <div className="max-w-[75%] rounded-lg bg-primary px-3 py-2 text-sm text-primary-foreground">
                  <p className="whitespace-pre-wrap break-words">{pendingMessage}</p>
                </div>
              </div>

              {/* Chỉ báo AI đang trả lời */}
              <div className="flex items-end justify-start gap-2">
                <BotAvatar />
                <div className="flex items-center gap-1 rounded-lg bg-secondary px-3 py-2.5">
                  <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-text-muted [animation-delay:-0.3s]" />
                  <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-text-muted [animation-delay:-0.15s]" />
                  <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-text-muted" />
                </div>
              </div>
            </>
          ) : null}
        </>
      )}
    </div>
  )
}

function BotAvatar() {
  return (
    <Avatar className="h-8 w-8">
      <AvatarFallback className="bg-primary-light text-white">
        <Bot className="h-4 w-4" />
      </AvatarFallback>
    </Avatar>
  )
}
