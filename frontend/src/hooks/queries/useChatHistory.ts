import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useChatHistory(sessionId: number | undefined) {
  return useQuery({
    queryKey: ['chat-history', sessionId],
    queryFn: () => aiAdvisoryApi.getHistory(sessionId!),
    enabled: sessionId !== undefined,
  })
}
