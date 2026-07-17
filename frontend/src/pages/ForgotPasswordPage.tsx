import { Link } from 'react-router-dom'
import { Mail } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { AuthLayout } from '@/layouts/AuthLayout'
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
            Tính năng tự đặt lại mật khẩu chưa khả dụng. Vui lòng liên hệ đội ngũ hỗ trợ HappyFarmer để được cấp
            lại mật khẩu.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-text-muted">
            Gửi yêu cầu qua email{' '}
            <a href="mailto:ho-tro@happyfarmer.vn" className="font-medium text-primary hover:underline">
              ho-tro@happyfarmer.vn
            </a>{' '}
            kèm số điện thoại đã đăng ký để được hỗ trợ nhanh nhất.
          </p>
          <Button variant="outline" className="w-full" asChild>
            <Link to="/login">Quay lại đăng nhập</Link>
          </Button>
        </CardContent>
      </Card>
    </AuthLayout>
  )
}
