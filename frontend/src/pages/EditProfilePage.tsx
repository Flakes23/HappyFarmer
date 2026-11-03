import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { EditProfileForm } from '@/components/auth/EditProfileForm'
import { ChangePasswordForm } from '@/components/auth/ChangePasswordForm'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'

export function EditProfilePage() {
  useDocumentTitle('Hồ sơ của tôi — HappyFarmer')
  return (
    <div className="mx-auto max-w-sm space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Hồ sơ của tôi</CardTitle>
          <CardDescription>Cập nhật thông tin cá nhân và ảnh đại diện.</CardDescription>
        </CardHeader>
        <CardContent>
          <EditProfileForm />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Đổi mật khẩu</CardTitle>
          <CardDescription>Nhập mật khẩu hiện tại và chọn mật khẩu mới.</CardDescription>
        </CardHeader>
        <CardContent>
          <ChangePasswordForm />
        </CardContent>
      </Card>
    </div>
  )
}
