import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Label } from '@/components/ui/label'
import { ImageUploadField } from '@/components/marketplace/ImageUploadField'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { useCreateListing } from '@/hooks/mutations/useCreateListing'
import { createListingSchema, type CreateListingFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

export function ListingForm() {
  const navigate = useNavigate()
  const products = useProducts()
  const regions = useRegions()
  const createListing = useCreateListing()
  const [images, setImages] = useState<string[]>([])

  const form = useForm<CreateListingFormValues>({
    resolver: zodResolver(createListingSchema),
    defaultValues: {
      productId: undefined,
      quantity: '',
      unit: 'kg',
      pricePerUnit: '',
      regionId: undefined,
      description: '',
    },
  })

  function onSubmit(values: CreateListingFormValues) {
    const parsed = createListingSchema.parse(values)
    createListing.mutate(
      { ...parsed, imageUrls: images },
      {
        onSuccess: (listing) => {
          toast.success('Đăng tin bán thành công!')
          navigate(`/marketplace/listings/${listing.id}`)
        },
      }
    )
  }

  const errorMessage = createListing.isError
    ? extractApiErrorMessage(createListing.error, 'Đăng tin thất bại. Vui lòng thử lại.')
    : null

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="productId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Sản phẩm</FormLabel>
              <Select
                onValueChange={(v) => field.onChange(Number(v))}
                defaultValue={field.value ? String(field.value) : undefined}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn sản phẩm" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {products.data?.map((p) => (
                    <SelectItem key={p.id} value={String(p.id)}>
                      {p.nameVi}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="quantity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Số lượng</FormLabel>
                <FormControl>
                  <Input type="number" step="0.01" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="unit"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Đơn vị</FormLabel>
                <FormControl>
                  <Input placeholder="kg" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="pricePerUnit"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Giá / đơn vị (VNĐ)</FormLabel>
              <FormControl>
                <Input type="number" step="0.01" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="regionId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Khu vực</FormLabel>
              <Select
                onValueChange={(v) => field.onChange(Number(v))}
                defaultValue={field.value ? String(field.value) : undefined}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn khu vực" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {regions.data?.map((r) => (
                    <SelectItem key={r.id} value={String(r.id)}>
                      {r.marketName} — {r.provinceName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mô tả (tuỳ chọn)</FormLabel>
              <FormControl>
                <Textarea placeholder="Mô tả thêm về nông sản..." {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="space-y-2">
          <Label>Hình ảnh (tuỳ chọn)</Label>
          <ImageUploadField value={images} onChange={setImages} />
        </div>

        {errorMessage ? <p className="text-sm font-medium text-error">{errorMessage}</p> : null}

        <Button type="submit" className="w-full" disabled={createListing.isPending}>
          {createListing.isPending ? 'Đang đăng tin...' : 'Đăng tin bán'}
        </Button>
      </form>
    </Form>
  )
}
