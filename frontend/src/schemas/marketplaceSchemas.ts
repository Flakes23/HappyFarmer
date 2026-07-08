import { z } from 'zod'

export const createListingSchema = z.object({
  productId: z.coerce.number().min(1, 'Vui lòng chọn sản phẩm'),
  quantity: z.coerce.number().positive('Số lượng phải lớn hơn 0'),
  unit: z.string().min(1, 'Đơn vị là bắt buộc'),
  pricePerUnit: z.coerce.number().positive('Giá phải lớn hơn 0'),
  regionId: z.coerce.number().min(1, 'Vui lòng chọn khu vực'),
  description: z
    .string()
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
})

export type CreateListingFormValues = z.input<typeof createListingSchema>
export type CreateListingFormOutput = z.output<typeof createListingSchema>

export const updateListingSchema = z.object({
  quantity: z.coerce.number().positive('Số lượng phải lớn hơn 0'),
  pricePerUnit: z.coerce.number().positive('Giá phải lớn hơn 0'),
  description: z
    .string()
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
})

export type UpdateListingFormValues = z.input<typeof updateListingSchema>
export type UpdateListingFormOutput = z.output<typeof updateListingSchema>

export const createBuyRequestSchema = z.object({
  productId: z.coerce.number().min(1, 'Vui lòng chọn sản phẩm'),
  desiredQuantity: z.coerce.number().positive('Số lượng phải lớn hơn 0'),
  regionId: z.coerce.number().min(1, 'Vui lòng chọn khu vực'),
  maxPricePerUnit: z.coerce
    .number()
    .positive('Giá phải lớn hơn 0')
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
  description: z
    .string()
    .optional()
    .or(z.literal(''))
    .transform((v) => (v ? v : undefined)),
})

export type CreateBuyRequestFormValues = z.input<typeof createBuyRequestSchema>
export type CreateBuyRequestFormOutput = z.output<typeof createBuyRequestSchema>

export const contactListingSchema = z.object({
  message: z.string().min(1, 'Vui lòng nhập lời nhắn').max(1000, 'Lời nhắn tối đa 1000 ký tự'),
})

export type ContactListingFormValues = z.input<typeof contactListingSchema>
