import { useState } from 'react'
import { Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { differenceInCalendarDays, format } from 'date-fns'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { EmptyState } from '@/components/shared/EmptyState'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { HarvestPredictionResult } from '@/components/harvest/HarvestPredictionResult'
import { useHarvestHistory } from '@/hooks/queries/useHarvestHistory'
import { useHarvestPredictionDetail } from '@/hooks/queries/useHarvestPredictionDetail'
import { useDeleteHarvestPrediction } from '@/hooks/mutations/useDeleteHarvestPrediction'
import { extractApiErrorMessage } from '@/api/authApi'
import { formatViDate, parseIsoDate } from '@/lib/utils'

const CONFIDENCE_STYLES: Record<string, string> = {
  Cao: 'bg-success text-success-foreground border-transparent hover:bg-success',
  'Trung bình': 'bg-accent text-accent-foreground border-transparent hover:bg-accent',
  Thấp: 'bg-error text-error-foreground border-transparent hover:bg-error',
}

function confidenceLabel(level: string) {
  return `Độ tin cậy ${level.toLowerCase()}`
}

export function HarvestHistoryList() {
  const history = useHarvestHistory()
  const deletePrediction = useDeleteHarvestPrediction()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const detail = useHarvestPredictionDetail(selectedId)

  function handleDelete(id: number) {
    deletePrediction.mutate(id, {
      onError: (err) => toast.error(extractApiErrorMessage(err, 'Không thể xoá.')),
    })
  }

  if (history.isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-14 w-full" />
        <Skeleton className="h-14 w-full" />
      </div>
    )
  }

  if (!history.data || history.data.length === 0) {
    return <EmptyState title="Chưa có lượt dự đoán nào" description="Kết quả dự đoán thu hoạch của bạn sẽ hiện ở đây." />
  }

  return (
    <>
      <div className="space-y-2">
        {history.data.map((item) => (
          <button
            key={item.id}
            type="button"
            onClick={() => setSelectedId(item.id)}
            className="group flex w-full flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-surface px-4 py-3 text-left transition-colors hover:bg-secondary"
          >
            <div>
              <div className="flex items-center gap-3">
                <p className="font-medium text-text">
                  {item.cropType} — {item.location}
                </p>
                <p className="text-xs text-text-muted">
                  {format(new Date(item.createdAt), 'dd/MM/yyyy HH:mm')}
                </p>
              </div>
              <p className="text-sm text-text-muted">Ngày trồng: {formatViDate(item.plantingDate)}</p>
              <p className="text-sm text-text-muted">
                Ngày nên thu hoạch: {formatViDate(item.recommendedStartDate)} — {formatViDate(item.recommendedEndDate)}
              </p>
            </div>

            <div className="flex items-center gap-2">
              <Badge variant="secondary">
                {differenceInCalendarDays(parseIsoDate(item.recommendedStartDate), parseIsoDate(item.plantingDate))} ngày sinh trưởng
              </Badge>
              <Badge className={CONFIDENCE_STYLES[item.confidenceLevel] ?? ''}>
                {confidenceLabel(item.confidenceLevel)}
              </Badge>

              <ConfirmDialog
                title="Xoá lượt dự đoán này?"
                description="Kết quả dự đoán này sẽ bị xoá vĩnh viễn."
                confirmLabel="Xoá"
                variant="destructive"
                onConfirm={() => handleDelete(item.id)}
              >
                <span
                  role="button"
                  aria-label="Xoá lượt dự đoán"
                  onClick={(e) => e.stopPropagation()}
                  className="shrink-0 rounded-md p-1.5 text-text-muted hover:bg-error hover:text-white"
                >
                  <Trash2 className="h-4 w-4" />
                </span>
              </ConfirmDialog>
            </div>
          </button>
        ))}
      </div>

      <Dialog open={selectedId !== null} onOpenChange={(open) => !open && setSelectedId(null)}>
        <DialogContent className="max-h-[85vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Chi tiết dự đoán thu hoạch</DialogTitle>
          </DialogHeader>

          {detail.isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-24 w-full" />
              <Skeleton className="h-16 w-full" />
            </div>
          ) : detail.data ? (
            <HarvestPredictionResult result={detail.data} />
          ) : null}
        </DialogContent>
      </Dialog>
    </>
  )
}
