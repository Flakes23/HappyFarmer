import { Badge } from '@/components/ui/badge'
import { formatViDate } from '@/lib/utils'
import type { HarvestPredictionResponse } from '@/api/types'

const CONFIDENCE_STYLES: Record<string, string> = {
  Cao: 'bg-success text-success-foreground border-transparent hover:bg-success',
  'Trung bình': 'bg-accent text-accent-foreground border-transparent hover:bg-accent',
  Thấp: 'bg-error text-error-foreground border-transparent hover:bg-error',
}

function confidenceLabel(level: string) {
  return `Độ tin cậy ${level.toLowerCase()}`
}

interface HarvestPredictionResultProps {
  result: HarvestPredictionResponse
}

export function HarvestPredictionResult({ result }: HarvestPredictionResultProps) {
  return (
    <div className="space-y-4 rounded-lg border border-border bg-surface p-4">
      <div className="rounded-md bg-secondary px-3 py-2 text-sm text-text">{result.transparencyNote}</div>

      <p className="text-sm text-text-muted">Ngày trồng: {formatViDate(result.plantingDate)}</p>

      <div className="flex flex-wrap items-center gap-3">
        <p className="text-lg font-semibold text-text">
          Ngày nên thu hoạch: {formatViDate(result.recommendedStartDate)} — {formatViDate(result.recommendedEndDate)}
        </p>
        <Badge className={CONFIDENCE_STYLES[result.confidenceLevel] ?? ''}>
          {confidenceLabel(result.confidenceLevel)}
        </Badge>
      </div>

      {result.riskFactors.length > 0 ? (
        <div>
          <p className="mb-1 text-sm font-medium text-text">Rủi ro cần lưu ý</p>
          <ul className="list-disc space-y-1 pl-5 text-sm text-text-muted">
            {result.riskFactors.map((risk, i) => (
              <li key={i}>{risk}</li>
            ))}
          </ul>
        </div>
      ) : null}

      <p className="text-sm text-text-muted">{result.reasoning}</p>

      {result.weatherSummary ? (
        <p className="text-sm text-text-muted">
          Thời tiết dự báo: nhiệt độ TB {result.weatherSummary.avgTempC.toFixed(1)}°C, tổng lượng mưa{' '}
          {result.weatherSummary.totalRainfallMm.toFixed(1)}mm.
        </p>
      ) : null}
    </div>
  )
}
