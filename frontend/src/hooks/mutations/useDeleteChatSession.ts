import { useMutation, useQueryClient } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useDeleteChatSession() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => aiAdvisoryApi.deleteSession(id),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['chat-sessions'] })
      queryClient.removeQueries({ queryKey: ['chat-history', id] })
    },
  })
}
