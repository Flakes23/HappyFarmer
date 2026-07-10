import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { EditProfileForm } from '@/components/auth/EditProfileForm'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function EditProfilePage() {
  useDocumentTitle('Hồ sơ của tôi — HappyFarmer')
  return (
    <div className="mx-auto max-w-sm">
      <Card>
        <CardHeader>
          <CardTitle>Hồ sơ của tôi</CardTitle>
          <CardDescription>Cập nhật thông tin cá nhân và ảnh đại diện.</CardDescription>
        </CardHeader>
        <CardContent>
          <EditProfileForm />
        </CardContent>
      </Card>
    </div>
  )
}
