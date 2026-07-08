import { Link } from 'react-router-dom'
import { MessageSquareOff } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
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
              <Card key={interest.id} className="transition-colors hover:border-primary">
                <Link to={`/marketplace/my-interests/${interest.id}`} className="block">
                  <CardContent className="space-y-1 p-4">
                    <div className="flex items-center justify-between gap-2">
                      <span className="text-sm font-medium text-text">
                        {isSent ? 'Bạn đã gửi liên hệ' : 'Bạn nhận được liên hệ'}
                        {interest.listingId ? ` cho tin #${interest.listingId}` : null}
                      </span>
                      <StatusBadge status={interest.status} />
                    </div>
                    {interest.message ? <p className="text-sm text-text-muted">{interest.message}</p> : null}
                    <p className="text-xs text-text-muted">
                      {new Date(interest.createdAt).toLocaleString('vi-VN')}
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
