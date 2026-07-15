import { useState } from 'react'
import { HarvestPredictionForm } from '@/components/harvest/HarvestPredictionForm'
import { HarvestPredictionResult } from '@/components/harvest/HarvestPredictionResult'
import { HarvestHistoryList } from '@/components/harvest/HarvestHistoryList'
import type { HarvestPredictionResponse } from '@/api/types'

export function HarvestPredictionPage() {
  const [result, setResult] = useState<HarvestPredictionResponse | null>(null)

  return (
    <div className="space-y-6">
      <HarvestPredictionForm onResult={setResult} />

      {result ? <HarvestPredictionResult result={result} /> : null}

      <div>
        <h2 className="mb-2 text-sm font-medium text-text">Lịch sử dự đoán</h2>
        <HarvestHistoryList />
      </div>
    </div>
  )
}
