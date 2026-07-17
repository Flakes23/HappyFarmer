import { Link, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { InterestSummary } from '@/components/marketplace/InterestSummary'
import { ChatThread } from '@/components/marketplace/ChatThread'
import { EmptyState } from '@/components/shared/EmptyState'
import { useMyInterests } from '@/hooks/queries/useMyInterests'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function InterestThreadPage() {
  const { id } = useParams<{ id: string }>()
  const interestId = id ? Number(id) : undefined

  const interests = useMyInterests()
  const userId = useAuthStore((s) => s.user?.id)
  const interest = interests.data?.find((i) => i.id === interestId)

  useDocumentTitle('Trò chuyện — HappyFarmer')

  return (
    <div className="space-y-4">
      <Link to="/marketplace/my-interests" className="flex items-center gap-1 text-body-sm text-primary hover:underline">
        <ArrowLeft className="h-4 w-4" />
        Quay lại liên hệ của tôi
      </Link>

      {interests.isLoading ? (
        <Skeleton className="h-[60vh] w-full" />
      ) : !interest || interestId === undefined ? (
        <EmptyState title="Không tìm thấy cuộc trò chuyện" description="Liên hệ này có thể đã bị xoá hoặc không tồn tại." />
      ) : (
        <>
          <div className="space-y-3 rounded-lg border border-border bg-surface p-3">
            <div className="flex items-center justify-between gap-2">
              <p className="text-body-sm font-medium text-text">
                {interest.initiatorUserId === userId ? 'Bạn đã gửi liên hệ' : 'Bạn nhận được liên hệ'}
              </p>
              <StatusBadge status={interest.status} />
            </div>

            <Link
              to={
                interest.listingId
                  ? `/marketplace/listings/${interest.listingId}`
                  : `/marketplace/buy-requests/${interest.buyRequestId}`
              }
              className="block rounded-md transition-colors hover:bg-secondary"
            >
              <InterestSummary interest={interest} />
            </Link>

            <p className="text-xs text-text-muted">{new Date(interest.createdAt).toLocaleString('vi-VN')}</p>
          </div>

          <ChatThread interestId={interestId} />
        </>
      )}
    </div>
  )
}
