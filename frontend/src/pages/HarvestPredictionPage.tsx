import { useState } from 'react'
import { HarvestPredictionForm } from '@/components/harvest/HarvestPredictionForm'
import { HarvestPredictionResult } from '@/components/harvest/HarvestPredictionResult'
import { HarvestHistoryList } from '@/components/harvest/HarvestHistoryList'
import { EmptyState } from '@/components/shared/EmptyState'
import type { HarvestPredictionResponse } from '@/api/types'
import harvestIllustration from '@/assets/illustrations/illustration-harvest-result.webp'

export function HarvestPredictionPage() {
  const [result, setResult] = useState<HarvestPredictionResponse | null>(null)

  return (
    <div className="space-y-6">
      <HarvestPredictionForm onResult={setResult} />

      {result ? (
        <HarvestPredictionResult result={result} />
      ) : (
        <EmptyState
          illustration={harvestIllustration}
          title="Chưa có dự đoán nào"
          description="Điền thông tin cây trồng ở trên để xem thời điểm thu hoạch dự kiến."
        />
      )}

      <div>
        <h2 className="mb-2 text-sm font-medium text-text">Lịch sử dự đoán</h2>
        <HarvestHistoryList />
      </div>
    </div>
  )
}
