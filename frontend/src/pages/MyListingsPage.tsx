import { toast } from 'sonner'
import { Link } from 'react-router-dom'
import { PackageOpen } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { EditListingDialog } from '@/components/marketplace/EditListingDialog'
import { EmptyState } from '@/components/shared/EmptyState'
import { ListSkeleton } from '@/components/shared/Skeletons'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { useMyListings } from '@/hooks/queries/useMyListings'
import { useCloseListing } from '@/hooks/mutations/useCloseListing'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function MyListingsPage() {
  useDocumentTitle('Tin đăng của tôi — HappyFarmer')
  const myListings = useMyListings()
  const closeListing = useCloseListing()

  function handleClose(id: number) {
    closeListing.mutate(id, {
      onSuccess: () => toast.success('Đã đóng tin đăng.'),
      onError: () => toast.error('Đóng tin thất bại. Vui lòng thử lại.'),
    })
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-h1 text-text">Tin đăng của tôi</h1>
        <p className="text-text-muted">Quản lý các tin bán nông sản bạn đã đăng.</p>
      </div>

      {myListings.isLoading ? (
        <ListSkeleton count={3} />
      ) : myListings.data && myListings.data.length > 0 ? (
        <div className="space-y-3">
          {myListings.data.map((listing) => (
            <Card key={listing.id}>
              <CardContent className="flex flex-wrap items-center justify-between gap-3 p-4">
                <div>
                  <Link
                    to={`/marketplace/listings/${listing.id}`}
                    className="font-medium text-primary hover:underline"
                  >
                    Tin #{listing.id}
                  </Link>
                  <p className="text-sm text-text-muted">
                    {listing.quantity} {listing.unit} · {currencyFormatter.format(listing.pricePerUnit)}/
                    {listing.unit}
                  </p>
                </div>

                <div className="flex items-center gap-2">
                  <StatusBadge status={listing.status} />
                  {listing.status === 'Active' ? (
                    <>
                      <EditListingDialog listing={listing} />
                      <ConfirmDialog
                        title="Đóng tin đăng?"
                        description="Tin sẽ không còn hiển thị trên chợ nông sản. Bạn có thể đăng tin mới sau."
                        confirmLabel="Đóng tin"
                        variant="destructive"
                        onConfirm={() => handleClose(listing.id)}
                      >
                        <Button variant="outline" size="sm" disabled={closeListing.isPending}>
                          Đóng tin
                        </Button>
                      </ConfirmDialog>
                    </>
                  ) : null}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={PackageOpen}
          title="Bạn chưa có tin đăng nào"
          description="Đăng tin bán nông sản để người mua trên khắp cả nước có thể tìm thấy bạn."
        />
      )}
    </div>
  )
}
