import { Badge } from '@/components/ui/badge'
import type { DiseaseDetectionResponse } from '@/api/types'

const SEVERITY_STYLES: Record<string, string> = {
  Nhẹ: 'bg-success text-success-foreground border-transparent hover:bg-success',
  'Trung bình': 'bg-accent text-accent-foreground border-transparent hover:bg-accent',
  Nặng: 'bg-error text-error-foreground border-transparent hover:bg-error',
}

interface DiseaseDetectionResultProps {
  result: DiseaseDetectionResponse
}

function ListSection({ title, items }: { title: string; items: string[] }) {
  if (items.length === 0) return null
  return (
    <div>
      <p className="mb-1 text-body-sm font-medium text-text">{title}</p>
      <ul className="list-disc space-y-1 pl-5 text-body-sm text-text-muted">
        {items.map((item, i) => (
          <li key={i}>{item}</li>
        ))}
      </ul>
    </div>
  )
}

export function DiseaseDetectionResult({ result }: DiseaseDetectionResultProps) {
  return (
    <div className="space-y-4 rounded-lg border border-border bg-surface p-4">
      <img
        src={result.imageUrl}
        alt={result.identifiedCropType}
        className="h-56 w-full max-w-sm rounded-md object-cover"
      />

      <div className="flex flex-wrap items-center gap-3">
        <p className="text-lg font-semibold text-text">{result.identifiedCropType}</p>
        <Badge
          className={
            result.isHealthy
              ? 'border-transparent bg-success text-success-foreground hover:bg-success'
              : 'border-transparent bg-error text-error-foreground hover:bg-error'
          }
        >
          {result.isHealthy ? 'Cây khỏe mạnh' : 'Phát hiện bệnh'}
        </Badge>
        {!result.isHealthy && result.severity ? (
          <Badge className={SEVERITY_STYLES[result.severity] ?? ''}>Mức độ: {result.severity}</Badge>
        ) : null}
        <span className="text-xs text-text-muted">Độ tin cậy: {Math.round(result.confidenceScore * 100)}%</span>
      </div>

      {!result.isHealthy && result.diseaseName ? (
        <p className="text-body-sm font-medium text-text">Bệnh/sâu hại: {result.diseaseName}</p>
      ) : null}

      <p className="text-body-sm text-text-muted">{result.description}</p>

      <ListSection title="Biện pháp hữu cơ" items={result.treatmentOrganic} />
      <ListSection title="Biện pháp hóa học" items={result.treatmentChemical} />
      <ListSection title="Phòng ngừa" items={result.preventionTips} />
      <ListSection title="Hành động ngay" items={result.recommendedActions} />
    </div>
  )
}
