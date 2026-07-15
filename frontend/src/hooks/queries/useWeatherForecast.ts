import { useQuery } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'

export function useWeatherForecast(location: string | undefined) {
  return useQuery({
    queryKey: ['weather-forecast', location],
    queryFn: () => aiAdvisoryApi.getWeatherForecast(location!),
    enabled: Boolean(location),
    retry: false,
  })
}
