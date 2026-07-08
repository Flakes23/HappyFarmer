import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { ListingForm } from '@/components/marketplace/ListingForm'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function NewListingPage() {
  useDocumentTitle('Đăng tin bán — HappyFarmer')
  return (
    <div className="mx-auto max-w-xl">
      <Card>
        <CardHeader>
          <CardTitle>Đăng tin bán</CardTitle>
          <CardDescription>Điền thông tin nông sản bạn muốn bán.</CardDescription>
        </CardHeader>
        <CardContent>
          <ListingForm />
        </CardContent>
      </Card>
    </div>
  )
}
