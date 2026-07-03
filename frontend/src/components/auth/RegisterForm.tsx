import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { registerSchema, type RegisterFormValues } from '@/schemas/authSchemas'
import { useRegister } from '@/hooks/mutations/useRegister'
import { extractApiErrorMessage } from '@/api/authApi'
import { PROVINCES } from '@/lib/provinces'

export function RegisterForm() {
  const navigate = useNavigate()
  const register = useRegister()

  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      phoneNumber: '',
      email: '',
      password: '',
      fullName: '',
      role: 'Farmer',
      provinceId: undefined,
    },
  })

  function onSubmit(values: RegisterFormValues) {
    const parsed = registerSchema.parse(values)
    register.mutate(parsed, {
      onSuccess: () => navigate('/'),
    })
  }

  const errorMessage = register.isError
    ? extractApiErrorMessage(register.error, 'Đăng ký thất bại. Vui lòng thử lại.')
    : null

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="fullName"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Họ và tên</FormLabel>
              <FormControl>
                <Input autoComplete="name" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

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
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email (tùy chọn)</FormLabel>
              <FormControl>
                <Input type="email" autoComplete="email" {...field} />
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
                <Input type="password" autoComplete="new-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="role"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Bạn là</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn vai trò" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="Farmer">Nông dân</SelectItem>
                  <SelectItem value="Buyer">Người mua</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="provinceId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tỉnh/thành (tùy chọn)</FormLabel>
              <Select
                onValueChange={(v) => field.onChange(Number(v))}
                defaultValue={field.value ? String(field.value) : undefined}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn tỉnh/thành" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {PROVINCES.map((p) => (
                    <SelectItem key={p.id} value={String(p.id)}>
                      {p.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        {errorMessage ? <p className="text-sm font-medium text-error">{errorMessage}</p> : null}

        <Button type="submit" className="w-full" disabled={register.isPending}>
          {register.isPending ? 'Đang đăng ký...' : 'Đăng ký'}
        </Button>
      </form>
    </Form>
  )
}
