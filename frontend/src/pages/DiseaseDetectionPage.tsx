import { useState } from 'react'
import { AlertTriangle } from 'lucide-react'
import { DiseaseDetectionForm } from '@/components/disease/DiseaseDetectionForm'
import { DiseaseDetectionResult } from '@/components/disease/DiseaseDetectionResult'
import { DiseaseDetectionHistoryList } from '@/components/disease/DiseaseDetectionHistoryList'
import { EmptyState } from '@/components/shared/EmptyState'
import type { InvalidPlantImageDetail } from '@/api/aiAdvisoryApi'
import type { DiseaseDetectionResponse } from '@/api/types'
import diseaseIllustration from '@/assets/illustrations/illustration-disease-result.webp'

export function DiseaseDetectionPage() {
  const [result, setResult] = useState<DiseaseDetectionResponse | null>(null)
  const [invalidImage, setInvalidImage] = useState<InvalidPlantImageDetail | null>(null)

  return (
    <div className="space-y-6">
      <DiseaseDetectionForm
        onResult={(r) => {
          setResult(r)
          setInvalidImage(null)
        }}
        onInvalidImage={setInvalidImage}
      />

      {invalidImage ? (
        <div className="flex items-start gap-3 rounded-lg border border-accent bg-secondary p-4 text-sm text-text">
          <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-accent" />
          <div>
            <p className="font-medium">{invalidImage.message}</p>
            {invalidImage.description ? <p className="mt-1 text-text-muted">{invalidImage.description}</p> : null}
          </div>
        </div>
      ) : null}

      {result ? (
        <DiseaseDetectionResult result={result} />
      ) : !invalidImage ? (
        <EmptyState
          illustration={diseaseIllustration}
          title="Chưa có chẩn đoán nào"
          description="Tải ảnh cây trồng ở trên để nhận diện bệnh."
        />
      ) : null}

      <div>
        <h2 className="mb-2 text-sm font-medium text-text">Lịch sử chẩn đoán</h2>
        <DiseaseDetectionHistoryList />
      </div>
    </div>
  )
}
