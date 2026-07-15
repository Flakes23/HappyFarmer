import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useChatSessions() {
  return useQuery({
    queryKey: ['chat-sessions'],
    queryFn: aiAdvisoryApi.listSessions,
  })
}
