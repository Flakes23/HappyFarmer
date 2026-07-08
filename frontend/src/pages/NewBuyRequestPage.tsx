import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { BuyRequestForm } from '@/components/marketplace/BuyRequestForm'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function NewBuyRequestPage() {
  useDocumentTitle('Đăng yêu cầu mua — HappyFarmer')
  return (
    <div className="mx-auto max-w-xl">
      <Card>
        <CardHeader>
          <CardTitle>Đăng yêu cầu mua</CardTitle>
          <CardDescription>Cho nông dân biết bạn đang cần mua nông sản gì.</CardDescription>
        </CardHeader>
        <CardContent>
          <BuyRequestForm />
        </CardContent>
      </Card>
    </div>
  )
}
