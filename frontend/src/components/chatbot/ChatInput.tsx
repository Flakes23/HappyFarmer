import { useState } from 'react'
import { Loader2, Send } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { useSendChatMessage } from '@/hooks/mutations/useSendChatMessage'
import { extractApiErrorMessage } from '@/api/authApi'

interface ChatInputProps {
  sessionId: number
  onPendingChange: (message: string | null) => void
}

export function ChatInput({ sessionId, onPendingChange }: ChatInputProps) {
  const [draft, setDraft] = useState('')
  const sendMessage = useSendChatMessage(sessionId)

  function handleSend() {
    const body = draft.trim()
    if (!body) return
    // Báo ngay cho ChatMessageList hiển thị bong bóng "đang gửi" + "AI đang trả lời", và clear
    // input ngay — thấy bong bóng "..." tức là tin nhắn đã đi, không cần đợi phản hồi mới coi là
    // gửi xong. Nếu lỗi, khôi phục lại nội dung vào ô input để không mất chữ đã gõ.
    onPendingChange(body)
    setDraft('')
    sendMessage.mutate(body, {
      onSuccess: () => onPendingChange(null),
      onError: (err) => {
        onPendingChange(null)
        setDraft(body)
        toast.error(extractApiErrorMessage(err, 'Không thể gửi tin nhắn. Vui lòng thử lại.'))
      },
    })
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  return (
    <div className="flex items-end gap-2 border-t border-border p-3">
      <Textarea
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Hỏi về canh tác, sâu bệnh, giá nông sản..."
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
  )
}
