import { useState } from 'react'
import { Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { format } from 'date-fns'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { EmptyState } from '@/components/shared/EmptyState'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { HistoryRowSkeleton } from '@/components/shared/Skeletons'
import { DiseaseDetectionResult } from '@/components/disease/DiseaseDetectionResult'
import { useDiseaseHistory } from '@/hooks/queries/useDiseaseHistory'
import { useDiseaseDetectionDetail } from '@/hooks/queries/useDiseaseDetectionDetail'
import { useDeleteDiseaseDetection } from '@/hooks/mutations/useDeleteDiseaseDetection'
import { extractApiErrorMessage } from '@/api/authApi'

const SEVERITY_STYLES: Record<string, string> = {
  Nhẹ: 'bg-success text-success-foreground border-transparent hover:bg-success',
  'Trung bình': 'bg-accent text-accent-foreground border-transparent hover:bg-accent',
  Nặng: 'bg-error text-error-foreground border-transparent hover:bg-error',
}

export function DiseaseDetectionHistoryList() {
  const history = useDiseaseHistory()
  const deleteDetection = useDeleteDiseaseDetection()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const detail = useDiseaseDetectionDetail(selectedId)

  function handleDelete(id: number) {
    deleteDetection.mutate(id, {
      onError: (err) => toast.error(extractApiErrorMessage(err, 'Không thể xoá.')),
    })
  }

  if (history.isLoading) {
    return <HistoryRowSkeleton count={2} />
  }

  if (!history.data || history.data.length === 0) {
    return (
      <EmptyState title="Chưa có lượt chẩn đoán nào" description="Kết quả nhận diện bệnh cây của bạn sẽ hiện ở đây." />
    )
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
            <div className="flex items-center gap-3">
              <img
                src={item.imageUrl}
                alt={item.identifiedCropType}
                className="h-12 w-12 shrink-0 rounded-md border border-border object-cover"
              />
              <div>
                <div className="flex items-center gap-3">
                  <p className="font-medium text-text">{item.identifiedCropType}</p>
                  <p className="text-xs text-text-muted">{format(new Date(item.createdAt), 'dd/MM/yyyy HH:mm')}</p>
                </div>
                {!item.isHealthy && item.diseaseName ? (
                  <p className="text-body-sm text-text-muted">{item.diseaseName}</p>
                ) : null}
              </div>
            </div>

            <div className="flex items-center gap-2">
              <Badge
                className={
                  item.isHealthy
                    ? 'border-transparent bg-success text-success-foreground hover:bg-success'
                    : 'border-transparent bg-error text-error-foreground hover:bg-error'
                }
              >
                {item.isHealthy ? 'Khỏe mạnh' : 'Có bệnh'}
              </Badge>
              {!item.isHealthy && item.severity ? (
                <Badge className={SEVERITY_STYLES[item.severity] ?? ''}>{item.severity}</Badge>
              ) : null}

              <ConfirmDialog
                title="Xoá lượt chẩn đoán này?"
                description="Kết quả chẩn đoán này sẽ bị xoá vĩnh viễn."
                confirmLabel="Xoá"
                variant="destructive"
                onConfirm={() => handleDelete(item.id)}
              >
                <span
                  role="button"
                  aria-label="Xoá lượt chẩn đoán"
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
            <DialogTitle>Chi tiết chẩn đoán bệnh cây</DialogTitle>
          </DialogHeader>

          {detail.isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-40 w-full" />
              <Skeleton className="h-16 w-full" />
            </div>
          ) : detail.data ? (
            <DiseaseDetectionResult result={detail.data} />
          ) : null}
        </DialogContent>
      </Dialog>
    </>
  )
}
