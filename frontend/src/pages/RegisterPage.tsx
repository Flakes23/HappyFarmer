import { Link } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { RegisterForm } from '@/components/auth/RegisterForm'
import { AuthLayout } from '@/layouts/AuthLayout'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import authIllustration from '@/assets/illustrations/illustration-auth.webp'

export function RegisterPage() {
  useDocumentTitle('Đăng ký — HappyFarmer')
  return (
    <AuthLayout illustration={authIllustration}>
      <Card>
        <CardHeader>
          <CardTitle>Đăng ký</CardTitle>
          <CardDescription>Tạo tài khoản HappyFarmer mới.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <RegisterForm />
          <p className="text-center text-body-sm text-text-muted">
            Đã có tài khoản?{' '}
            <Link to="/login" className="font-medium text-primary hover:underline">
              Đăng nhập
            </Link>
          </p>
        </CardContent>
      </Card>
    </AuthLayout>
  )
}
