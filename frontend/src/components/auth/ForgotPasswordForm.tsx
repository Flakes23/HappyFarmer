import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PasswordInput } from '@/components/auth/PasswordInput'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import {
  forgotPasswordSchema,
  resetPasswordSchema,
  verifyCodeSchema,
  type ForgotPasswordFormValues,
  type ResetPasswordFormValues,
  type VerifyCodeFormValues,
} from '@/schemas/authSchemas'
import { useForgotPassword } from '@/hooks/mutations/useForgotPassword'
import { useVerifyResetCode } from '@/hooks/mutations/useVerifyResetCode'
import { useResetPassword } from '@/hooks/mutations/useResetPassword'
import { extractApiErrorMessage } from '@/api/authApi'

type Step = { name: 'email' } | { name: 'code'; email: string } | { name: 'password'; email: string; otp: string }

/** 3 bước: nhập email nhận mã → nhập mã xác nhận (đúng mới sang bước tiếp) → nhập mật khẩu mới. */
export function ForgotPasswordForm() {
  const [step, setStep] = useState<Step>({ name: 'email' })

  if (step.name === 'email') {
    return <EmailStep onSent={(email) => setStep({ name: 'code', email })} />
  }
  if (step.name === 'code') {
    return (
      <CodeStep
        email={step.email}
        onVerified={(otp) => setStep({ name: 'password', email: step.email, otp })}
        onChangeEmail={() => setStep({ name: 'email' })}
      />
    )
  }
  return <PasswordStep email={step.email} otp={step.otp} />
}

function EmailStep({ onSent }: { onSent: (email: string) => void }) {
  const forgot = useForgotPassword()
  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: '' },
    mode: 'onBlur',
  })

  function onSubmit(values: ForgotPasswordFormValues) {
    forgot.mutate(values, {
      onSuccess: () => {
        toast.success('Mã xác nhận đã được gửi nếu email tồn tại trong hệ thống.')
        onSent(values.email.trim())
      },
    })
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" placeholder="nguyenvana@gmail.com" autoComplete="email" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {forgot.isError ? (
          <p className="text-body-sm font-medium text-error">
            {extractApiErrorMessage(forgot.error, 'Không gửi được mã. Vui lòng thử lại sau.')}
          </p>
        ) : null}

        <Button type="submit" className="w-full" disabled={forgot.isPending}>
          {forgot.isPending ? 'Đang gửi mã...' : 'Gửi mã xác nhận'}
        </Button>
        <Button variant="outline" className="w-full" asChild>
          <Link to="/login">Quay lại đăng nhập</Link>
        </Button>
      </form>
    </Form>
  )
}

function CodeStep({
  email,
  onVerified,
  onChangeEmail,
}: {
  email: string
  onVerified: (otp: string) => void
  onChangeEmail: () => void
}) {
  const verify = useVerifyResetCode()
  const resend = useForgotPassword()

  const form = useForm<VerifyCodeFormValues>({
    resolver: zodResolver(verifyCodeSchema),
    defaultValues: { otp: '' },
    mode: 'onBlur',
  })

  function onSubmit(values: VerifyCodeFormValues) {
    verify.mutate({ email, otp: values.otp }, { onSuccess: () => onVerified(values.otp) })
  }

  function onResend() {
    resend.mutate(
      { email },
      {
        onSuccess: () => toast.success('Đã gửi lại mã xác nhận.'),
        onError: (error) => toast.error(extractApiErrorMessage(error, 'Không gửi lại được mã.')),
      },
    )
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <p className="text-body-sm text-text-muted">
          Mã xác nhận 6 chữ số đã được gửi tới <span className="font-medium text-foreground">{email}</span>. Mã có
          hiệu lực trong 10 phút.
        </p>

        <FormField
          control={form.control}
          name="otp"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mã xác nhận</FormLabel>
              <FormControl>
                <Input inputMode="numeric" maxLength={6} placeholder="123456" autoComplete="one-time-code" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {verify.isError ? (
          <p className="text-body-sm font-medium text-error">
            {extractApiErrorMessage(verify.error, 'Không xác nhận được mã. Vui lòng thử lại.')}
          </p>
        ) : null}

        <Button type="submit" className="w-full" disabled={verify.isPending}>
          {verify.isPending ? 'Đang kiểm tra...' : 'Xác nhận'}
        </Button>

        <div className="flex items-center justify-between text-xs">
          <button type="button" className="text-primary hover:underline" onClick={onChangeEmail}>
            Dùng email khác
          </button>
          <button
            type="button"
            className="text-primary hover:underline disabled:opacity-50"
            onClick={onResend}
            disabled={resend.isPending}
          >
            Gửi lại mã
          </button>
        </div>
      </form>
    </Form>
  )
}

function PasswordStep({ email, otp }: { email: string; otp: string }) {
  const navigate = useNavigate()
  const reset = useResetPassword()

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
    mode: 'onBlur',
  })

  function onSubmit(values: ResetPasswordFormValues) {
    reset.mutate(
      { email, otp, newPassword: values.newPassword },
      {
        onSuccess: () => {
          toast.success('Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.')
          navigate('/login')
        },
      },
    )
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <p className="text-body-sm text-text-muted">Mã đã được xác nhận. Hãy chọn mật khẩu mới cho tài khoản của bạn.</p>

        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mật khẩu mới</FormLabel>
              <FormControl>
                <PasswordInput autoComplete="new-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Nhập lại mật khẩu mới</FormLabel>
              <FormControl>
                <PasswordInput autoComplete="new-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {reset.isError ? (
          <p className="text-body-sm font-medium text-error">
            {extractApiErrorMessage(reset.error, 'Không đặt lại được mật khẩu. Vui lòng thử lại.')}
          </p>
        ) : null}

        <Button type="submit" className="w-full" disabled={reset.isPending}>
          {reset.isPending ? 'Đang xử lý...' : 'Đặt lại mật khẩu'}
        </Button>
      </form>
    </Form>
  )
}
