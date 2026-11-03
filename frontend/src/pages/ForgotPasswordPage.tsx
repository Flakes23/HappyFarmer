import { Mail } from 'lucide-react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { AuthLayout } from '@/layouts/AuthLayout'
import { ForgotPasswordForm } from '@/components/auth/ForgotPasswordForm'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import authIllustration from '@/assets/illustrations/illustration-auth.webp'

export function ForgotPasswordPage() {
  useDocumentTitle('Quên mật khẩu — HappyFarmer')
  return (
    <AuthLayout illustration={authIllustration}>
      <Card>
        <CardHeader>
          <span className="flex h-10 w-10 items-center justify-center rounded-full bg-secondary text-primary">
            <Mail className="h-5 w-5" />
          </span>
          <CardTitle className="pt-2">Quên mật khẩu</CardTitle>
          <CardDescription>
            Nhập email đã đăng ký, chúng tôi sẽ gửi mã xác nhận để bạn đặt lại mật khẩu. Tài khoản chưa cập nhật
            email chưa dùng được tính năng này.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ForgotPasswordForm />
        </CardContent>
      </Card>
    </AuthLayout>
  )
}
