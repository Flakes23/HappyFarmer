import { z } from 'zod'

export const loginSchema = z.object({
  phoneNumber: z.string().min(1, 'Số điện thoại là bắt buộc'),
  password: z.string().min(1, 'Mật khẩu là bắt buộc'),
})

export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z.object({
  phoneNumber: z.string().min(1, 'Số điện thoại là bắt buộc'),
  email: z
    .string()
    .email('Email không hợp lệ')
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
  password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
  fullName: z.string().min(1, 'Họ tên là bắt buộc'),
  role: z.enum(['Farmer', 'Buyer'], { message: 'Vui lòng chọn vai trò' }),
  provinceId: z.coerce.number().optional(),
})

export type RegisterFormValues = z.input<typeof registerSchema>
export type RegisterFormOutput = z.output<typeof registerSchema>

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email là bắt buộc').email('Email không hợp lệ'),
})

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>

export const resetPasswordSchema = z
  .object({
    otp: z.string().regex(/^\d{6}$/, 'Mã xác nhận gồm 6 chữ số'),
    newPassword: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
    confirmPassword: z.string().min(1, 'Vui lòng nhập lại mật khẩu mới'),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu nhập lại không khớp',
  })

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Vui lòng nhập mật khẩu hiện tại'),
    newPassword: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
    confirmPassword: z.string().min(1, 'Vui lòng nhập lại mật khẩu mới'),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu nhập lại không khớp',
  })
  .refine((v) => v.newPassword !== v.currentPassword, {
    path: ['newPassword'],
    message: 'Mật khẩu mới phải khác mật khẩu hiện tại',
  })

export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>

export const editProfileSchema = z.object({
  fullName: z.string().min(1, 'Họ tên là bắt buộc'),
  email: z
    .string()
    .email('Email không hợp lệ')
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
  provinceId: z.coerce.number().optional(),
})

export type EditProfileFormValues = z.input<typeof editProfileSchema>
export type EditProfileFormOutput = z.output<typeof editProfileSchema>
