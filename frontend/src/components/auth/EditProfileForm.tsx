import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
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
import { AvatarUploadField } from '@/components/auth/AvatarUploadField'
import { editProfileSchema, type EditProfileFormValues } from '@/schemas/authSchemas'
import { useUpdateProfile } from '@/hooks/mutations/useUpdateProfile'
import { extractApiErrorMessage } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'
import { getInitial } from '@/lib/utils'
import { useProvinces } from '@/hooks/queries/useProvinces'

export function EditProfileForm() {
  const user = useAuthStore((s) => s.user)
  const updateProfile = useUpdateProfile()
  const provinces = useProvinces()
  const [avatarUrl, setAvatarUrl] = useState<string | null>(user?.avatarUrl ?? null)

  const form = useForm<EditProfileFormValues>({
    resolver: zodResolver(editProfileSchema),
    defaultValues: {
      fullName: user?.fullName ?? '',
      email: user?.email ?? '',
      provinceId: user?.provinceId ?? undefined,
    },
    mode: 'onBlur',
  })

  function onSubmit(values: EditProfileFormValues) {
    const parsed = editProfileSchema.parse(values)
    updateProfile.mutate(
      { ...parsed, avatarUrl: avatarUrl ?? undefined },
      { onSuccess: () => toast.success('Đã cập nhật thông tin cá nhân.') }
    )
  }

  const errorMessage = updateProfile.isError
    ? extractApiErrorMessage(updateProfile.error, 'Cập nhật thất bại. Vui lòng thử lại.')
    : null

  if (!user) return null

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <AvatarUploadField value={avatarUrl} onChange={setAvatarUrl} fallbackText={getInitial(user.fullName)} />

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
                  {(provinces.data ?? []).map((p) => (
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

        <Button type="submit" className="w-full" disabled={updateProfile.isPending}>
          {updateProfile.isPending ? 'Đang lưu...' : 'Lưu thay đổi'}
        </Button>
      </form>
    </Form>
  )
}
