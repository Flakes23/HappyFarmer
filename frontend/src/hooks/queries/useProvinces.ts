import { useQuery } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'

export function useProvinces() {
  return useQuery({
    queryKey: ['provinces'],
    queryFn: authApi.getProvinces,
    staleTime: 60_000,
  })
}
