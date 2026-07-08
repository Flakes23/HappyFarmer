import { useMutation, useQueryClient } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import type { MessageHistoryResponse, MessageResponse } from '@/api/types'

export function appendIfNew(old: MessageHistoryResponse | undefined, msg: MessageResponse): MessageHistoryResponse {
  const messages = old?.messages ?? []
  if (messages.some((m) => m.id === msg.id)) return old ?? { messages, hasMore: false }
  return { messages: [...messages, msg], hasMore: old?.hasMore ?? false }
}

export function useSendMessage(interestId: number) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (body: string) => marketplaceApi.sendMessage(interestId, body),
    onSuccess: (msg) => {
      queryClient.setQueryData<MessageHistoryResponse>(['messages', interestId], (old) => appendIfNew(old, msg))
    },
  })
}
