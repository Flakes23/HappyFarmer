import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useCreateChatSession() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: aiAdvisoryApi.createSession,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat-sessions'] })
    },
  })
}
