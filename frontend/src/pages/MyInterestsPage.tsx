import { Link } from 'react-router-dom'
import { MessageSquareOff } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { InterestSummary } from '@/components/marketplace/InterestSummary'
import { EmptyState } from '@/components/shared/EmptyState'
import { ListSkeleton } from '@/components/shared/Skeletons'
import { useMyInterests } from '@/hooks/queries/useMyInterests'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function MyInterestsPage() {
  useDocumentTitle('Liên hệ của tôi — HappyFarmer')
  const interests = useMyInterests()
  const userId = useAuthStore((s) => s.user?.id)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-text">Liên hệ của tôi</h1>
        <p className="text-text-muted">Các lượt liên hệ bạn đã gửi và nhận được trên Chợ nông sản.</p>
      </div>

      {interests.isLoading ? (
        <ListSkeleton count={3} />
      ) : interests.data && interests.data.length > 0 ? (
        <div className="space-y-3">
          {interests.data.map((interest) => {
            const isSent = interest.initiatorUserId === userId

            return (
              <Card key={interest.id} className="relative transition-colors hover:border-primary">
                {interest.hasUnread ? (
                  <span className="absolute -right-1 -top-1 h-3 w-3 rounded-full bg-error" aria-label="Chưa đọc" />
                ) : null}
                <Link to={`/marketplace/my-interests/${interest.id}`} className="block">
                  <CardContent className="space-y-2 p-4">
                    <div className="flex items-center justify-between gap-2">
                      <span className="text-sm font-medium text-text">
                        {isSent ? 'Bạn đã gửi liên hệ' : 'Bạn nhận được liên hệ'}
                      </span>
                      <StatusBadge status={interest.status} />
                    </div>

                    <InterestSummary interest={interest} />

                    {interest.lastMessage ? (
                      <p className="truncate text-sm text-text-muted">
                        {interest.lastMessage.senderUserId === userId ? 'Bạn: ' : ''}
                        {interest.lastMessage.body}
                      </p>
                    ) : null}
                    <p className="text-xs text-text-muted">
                      {new Date(interest.lastMessage?.createdAt ?? interest.createdAt).toLocaleString('vi-VN')}
                    </p>
                  </CardContent>
                </Link>
              </Card>
            )
          })}
        </div>
      ) : (
        <EmptyState
          icon={MessageSquareOff}
          title="Bạn chưa có liên hệ nào"
          description="Liên hệ với người bán/người mua trên Chợ nông sản sẽ hiển thị tại đây."
        />
      )}
    </div>
  )
}
