import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { isAxiosError } from 'axios'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { loginSchema, type LoginFormValues } from '@/schemas/authSchemas'
import { useLogin } from '@/hooks/mutations/useLogin'
import { extractApiErrorMessage } from '@/api/authApi'

export function LoginForm() {
  const navigate = useNavigate()
  const login = useLogin()

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { phoneNumber: '', password: '' },
  })

  function onSubmit(values: LoginFormValues) {
    login.mutate(values, {
      onSuccess: () => navigate('/'),
    })
  }

  const errorMessage = login.isError
    ? extractApiErrorMessage(
        login.error,
        isAxiosError(login.error) && login.error.response?.status === 429
          ? 'Bạn đã thử đăng nhập quá nhiều lần. Vui lòng thử lại sau.'
          : 'Số điện thoại hoặc mật khẩu không đúng.'
      )
    : null

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="phoneNumber"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Số điện thoại</FormLabel>
              <FormControl>
                <Input placeholder="09xxxxxxxx" autoComplete="tel" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mật khẩu</FormLabel>
              <FormControl>
                <Input type="password" autoComplete="current-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {errorMessage ? <p className="text-sm font-medium text-error">{errorMessage}</p> : null}

        <Button type="submit" className="w-full" disabled={login.isPending}>
          {login.isPending ? 'Đang đăng nhập...' : 'Đăng nhập'}
        </Button>
      </form>
    </Form>
  )
}
