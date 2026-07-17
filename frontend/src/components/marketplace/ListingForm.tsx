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
import { UnitSelect } from '@/components/marketplace/UnitSelect'
import { ProductSearchInput } from '@/components/marketplace/ProductSearchInput'
import { useRegions } from '@/hooks/queries/useRegions'
import { usePrices } from '@/hooks/queries/usePrices'
import { useCreateListing } from '@/hooks/mutations/useCreateListing'
import { createListingSchema, type CreateListingFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function ListingForm() {
  const navigate = useNavigate()
  const regions = useRegions()
  const createListing = useCreateListing()
  const [images, setImages] = useState<string[]>([])

  const form = useForm<CreateListingFormValues>({
    resolver: zodResolver(createListingSchema),
    defaultValues: {
      productId: undefined,
      quantity: '',
      unit: 'kg',
      totalPrice: '',
      regionId: undefined,
      description: '',
    },
  })

  function onSubmit(values: CreateListingFormValues) {
    const { totalPrice, ...parsed } = createListingSchema.parse(values)
    createListing.mutate(
      { ...parsed, pricePerUnit: totalPrice / parsed.quantity, imageUrls: images },
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

  const productIdValue = form.watch('productId') as number | undefined
  const regionIdValue = form.watch('regionId')
  const quantityValue = Number(form.watch('quantity'))
  const totalPriceValue = Number(form.watch('totalPrice'))
  const unitValue = form.watch('unit') || 'kg'

  const derivedPricePerUnit =
    Number.isFinite(quantityValue) && quantityValue > 0 && Number.isFinite(totalPriceValue) && totalPriceValue > 0
      ? totalPriceValue / quantityValue
      : null

  const prices = usePrices({ productId: productIdValue }, { enabled: Boolean(productIdValue) })
  const referencePrice = prices.data?.items.find((p) => p.regionId === regionIdValue)
  const deviationPercent =
    derivedPricePerUnit !== null && referencePrice && referencePrice.price > 0
      ? ((derivedPricePerUnit - referencePrice.price) / referencePrice.price) * 100
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
              <FormControl>
                <ProductSearchInput onChange={field.onChange} />
              </FormControl>
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
                <FormLabel>Số lượng ({form.watch('unit') || 'kg'})</FormLabel>
                <FormControl>
                  <Input type="number" step="0.01" {...field} value={field.value as number | string} />
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
                  <UnitSelect value={field.value} onChange={field.onChange} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="totalPrice"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tổng giá bán (VNĐ)</FormLabel>
              <FormControl>
                <Input type="number" step="1000" {...field} value={field.value as number | string} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {quantityValue > 0 ? (
          <p className="-mt-2 text-xs text-text-muted">
            Số lượng đang bán: <span className="font-medium text-text">{quantityValue}{unitValue}</span>.
            {derivedPricePerUnit ? (
              <>
                {' '}
                Bạn đang bán với giá{' '}
                <span className="font-medium text-primary">{currencyFormatter.format(derivedPricePerUnit)}</span>/
                {unitValue}
                {deviationPercent !== null && Math.abs(deviationPercent) >= 1
                  ? `, ${deviationPercent > 0 ? 'cao hơn' : 'thấp hơn'} giá thị trường ${Math.abs(deviationPercent).toFixed(0)}%`
                  : ''}
                .
              </>
            ) : null}
          </p>
        ) : null}

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
                      {r.provinceName}
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

        {errorMessage ? <p className="text-body-sm font-medium text-error">{errorMessage}</p> : null}

        <Button type="submit" className="w-full" disabled={createListing.isPending}>
          {createListing.isPending ? 'Đang đăng tin...' : 'Đăng tin bán'}
        </Button>
      </form>
    </Form>
  )
}
