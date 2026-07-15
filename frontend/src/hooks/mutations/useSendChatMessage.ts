import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'
import type { ChatMessageDto } from '@/api/types'

export function useSendChatMessage(sessionId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (message: string) => aiAdvisoryApi.sendMessage(sessionId, message),
    onSuccess: (res, message) => {
      const now = Date.now()
      const userMsg: ChatMessageDto = { id: now, sender: 'User', content: message, createdAt: res.timestamp }
      const aiMsg: ChatMessageDto = { id: now + 1, sender: 'AI', content: res.reply, createdAt: res.timestamp }
      queryClient.setQueryData<ChatMessageDto[]>(['chat-history', sessionId], (old) => [
        ...(old ?? []),
        userMsg,
        aiMsg,
      ])
      // LastActivityAt/Title đổi sau khi gửi tin -> re-sort/re-render sidebar
      queryClient.invalidateQueries({ queryKey: ['chat-sessions'] })
    },
  })
}
