import { Cloud, CloudDrizzle, CloudFog, CloudLightning, CloudRain, CloudSnow, CloudSun, Droplets, Sun } from 'lucide-react'
import { format } from 'date-fns'
import { vi } from 'date-fns/locale'
import { WeatherForecastSkeleton } from '@/components/shared/Skeletons'
import { useWeatherForecast } from '@/hooks/queries/useWeatherForecast'
import { parseIsoDate } from '@/lib/utils'

function getWeatherIcon(weatherId: number) {
  if (weatherId >= 200 && weatherId < 300) return CloudLightning
  if (weatherId >= 300 && weatherId < 400) return CloudDrizzle
  if (weatherId >= 500 && weatherId < 600) return CloudRain
  if (weatherId >= 600 && weatherId < 700) return CloudSnow
  if (weatherId >= 700 && weatherId < 800) return CloudFog
  if (weatherId === 800) return Sun
  if (weatherId === 801) return CloudSun
  return Cloud
}

function capitalize(text: string) {
  return text.charAt(0).toUpperCase() + text.slice(1)
}

interface WeatherForecastPreviewProps {
  location: string | undefined
}

export function WeatherForecastPreview({ location }: WeatherForecastPreviewProps) {
  const forecast = useWeatherForecast(location)

  if (!location) return null

  if (forecast.isLoading) {
    return (
      <div className="rounded-lg border border-border bg-secondary p-4">
        <p className="mb-3 text-body-sm font-medium text-text">Dự báo thời tiết — {location}</p>
        <WeatherForecastSkeleton />
      </div>
    )
  }

  if (forecast.isError || !forecast.data || forecast.data.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-secondary p-4 text-body-sm text-text-muted">
        Chưa lấy được dự báo thời tiết cho {location}.
      </div>
    )
  }

  return (
    <div className="rounded-lg border border-border bg-gradient-to-br from-secondary to-background p-4">
      <p className="mb-3 text-body-sm font-medium text-text">Dự báo thời tiết — {location}</p>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
        {forecast.data.map((day) => {
          const Icon = getWeatherIcon(day.weatherId)
          return (
            <div
              key={day.date}
              className="flex flex-col items-center gap-1 rounded-md bg-surface px-2 py-3 text-center shadow-sm"
            >
              <p className="text-xs font-medium capitalize text-text-muted">
                {format(parseIsoDate(day.date), 'EEEEEE dd/MM', { locale: vi })}
              </p>
              <Icon className="h-7 w-7 text-primary" />
              <p className="text-xs text-text-muted">{capitalize(day.weatherDescription)}</p>
              <p className="text-body-sm font-semibold text-text">
                {Math.round(day.minTempC)}°–{Math.round(day.maxTempC)}°C
              </p>
              <p className="flex items-center gap-1 text-xs text-accent">
                <Droplets className="h-3 w-3" />
                {day.popPercent}%{day.totalRainfallMm > 0.5 ? ` · ${day.totalRainfallMm.toFixed(1)}mm` : ''}
              </p>
            </div>
          )
        })}
      </div>
    </div>
  )
}
