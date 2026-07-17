import { Link } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { LoginForm } from '@/components/auth/LoginForm'
import { AuthLayout } from '@/layouts/AuthLayout'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import authIllustration from '@/assets/illustrations/illustration-auth.webp'

export function LoginPage() {
  useDocumentTitle('Đăng nhập — HappyFarmer')
  return (
    <AuthLayout illustration={authIllustration}>
      <Card>
        <CardHeader>
          <CardTitle>Đăng nhập</CardTitle>
          <CardDescription>Đăng nhập để tiếp tục sử dụng HappyFarmer.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <LoginForm />
          <p className="text-center text-body-sm text-text-muted">
            Chưa có tài khoản?{' '}
            <Link to="/register" className="font-medium text-primary hover:underline">
              Đăng ký
            </Link>
          </p>
        </CardContent>
      </Card>
    </AuthLayout>
  )
}
