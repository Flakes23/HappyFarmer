import { Link, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb'
import { StatusBadge } from '@/components/marketplace/StatusBadge'
import { ContactDialog } from '@/components/marketplace/ContactDialog'
import { SellerInfo } from '@/components/marketplace/SellerInfo'
import { EmptyState } from '@/components/shared/EmptyState'
import { useBuyRequest } from '@/hooks/queries/useBuyRequest'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { useAuthStore } from '@/store/authStore'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { formatRelativeTime } from '@/lib/relativeTime'
import notFoundIllustration from '@/assets/illustrations/illustration-not-found.webp'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function BuyRequestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const buyRequestId = id ? Number(id) : undefined

  const buyRequest = useBuyRequest(buyRequestId)
  const products = useProducts()
  const regions = useRegions()
  const user = useAuthStore((s) => s.user)

  const product = products.data?.find((p) => p.id === buyRequest.data?.productId)
  const region = regions.data?.find((r) => r.id === buyRequest.data?.regionId)
  useDocumentTitle(product ? `${product.nameVi} — Chợ nông sản` : 'Chợ nông sản — HappyFarmer')

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Link to="/marketplace" className="flex items-center gap-1 text-body-sm text-primary hover:underline">
          <ArrowLeft className="h-4 w-4" />
          Quay lại chợ nông sản
        </Link>

        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink asChild>
                <Link to="/marketplace">Chợ nông sản</Link>
              </BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>
                {products.isLoading ? (
                  <Skeleton className="inline-block h-4 w-24 align-middle" />
                ) : (
                  (product?.nameVi ?? `Sản phẩm #${buyRequest.data?.productId ?? ''}`)
                )}
              </BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>
      </div>

      {buyRequest.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : !buyRequest.data ? (
        <EmptyState illustration={notFoundIllustration} title="Không tìm thấy yêu cầu mua" />
      ) : (
        <Card>
          <CardHeader className="flex flex-row items-start justify-between gap-4">
            <CardTitle>{product?.nameVi ?? `Sản phẩm #${buyRequest.data.productId}`}</CardTitle>
            <StatusBadge status={buyRequest.data.status} />
          </CardHeader>
          <CardContent className="space-y-4">
            <dl className="grid grid-cols-2 gap-3 text-body-sm">
              <div>
                <dt className="text-text-muted">Cần mua</dt>
                <dd className="font-medium">
                  {buyRequest.data.desiredQuantity} {buyRequest.data.unit}
                </dd>
              </div>
              {buyRequest.data.maxPricePerUnit ? (
                <div>
                  <dt className="text-text-muted">Giá tối đa</dt>
                  <dd className="font-medium text-primary">
                    {currencyFormatter.format(buyRequest.data.maxPricePerUnit)} / {buyRequest.data.unit}
                  </dd>
                </div>
              ) : null}
              <div>
                <dt className="text-text-muted">Khu vực</dt>
                <dd className="font-medium">
                  {regions.isLoading ? (
                    <Skeleton className="inline-block h-4 w-20 align-middle" />
                  ) : (
                    (region ? region.provinceName : `#${buyRequest.data.regionId}`)
                  )}
                </dd>
              </div>
              <div>
                <dt className="text-text-muted">Ngày đăng</dt>
                <dd className="font-medium">{new Date(buyRequest.data.createdAt).toLocaleDateString('vi-VN')}</dd>
              </div>
            </dl>

            {buyRequest.data.description ? <p className="text-text">{buyRequest.data.description}</p> : null}

            <div className="space-y-1 border-t border-border pt-4">
              <SellerInfo
                name={buyRequest.data.buyerName}
                avatarUrl={buyRequest.data.buyerAvatarUrl}
                joinedAt={buyRequest.data.buyerJoinedAt}
                otherActiveCount={
                  buyRequest.data.status === 'Active'
                    ? Math.max(buyRequest.data.buyerActiveBuyRequestCount - 1, 0)
                    : buyRequest.data.buyerActiveBuyRequestCount
                }
                otherActiveLabel="yêu cầu khác"
              />
              <p className="text-[11px] text-text-muted">{formatRelativeTime(buyRequest.data.createdAt)}</p>
            </div>

            {user?.role === 'Farmer' && buyRequest.data.status === 'Active' ? (
              <ContactDialog id={buyRequest.data.id} kind="buyRequest" />
            ) : null}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
