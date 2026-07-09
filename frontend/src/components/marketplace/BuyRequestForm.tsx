import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { UnitSelect } from '@/components/marketplace/UnitSelect'
import { ProductSearchInput } from '@/components/marketplace/ProductSearchInput'
import { useRegions } from '@/hooks/queries/useRegions'
import { usePrices } from '@/hooks/queries/usePrices'
import { useCreateBuyRequest } from '@/hooks/mutations/useCreateBuyRequest'
import { createBuyRequestSchema, type CreateBuyRequestFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

export function BuyRequestForm() {
  const navigate = useNavigate()
  const regions = useRegions()
  const createBuyRequest = useCreateBuyRequest()

  const form = useForm<CreateBuyRequestFormValues>({
    resolver: zodResolver(createBuyRequestSchema),
    defaultValues: {
      productId: undefined,
      desiredQuantity: '',
      unit: 'kg',
      regionId: undefined,
      maxTotalPrice: '',
      description: '',
    },
  })

  function onSubmit(values: CreateBuyRequestFormValues) {
    const { maxTotalPrice, ...parsed } = createBuyRequestSchema.parse(values)
    createBuyRequest.mutate(
      { ...parsed, maxPricePerUnit: maxTotalPrice ? maxTotalPrice / parsed.desiredQuantity : undefined },
      {
        onSuccess: () => {
          toast.success('Đăng yêu cầu mua thành công!')
          navigate('/marketplace')
        },
      }
    )
  }

  const errorMessage = createBuyRequest.isError
    ? extractApiErrorMessage(createBuyRequest.error, 'Đăng yêu cầu thất bại. Vui lòng thử lại.')
    : null

  const productIdValue = form.watch('productId')
  const regionIdValue = form.watch('regionId')
  const desiredQuantityValue = Number(form.watch('desiredQuantity'))
  const maxTotalPriceValue = Number(form.watch('maxTotalPrice'))
  const unitValue = form.watch('unit') || 'kg'
  const derivedMaxPricePerUnit =
    Number.isFinite(desiredQuantityValue) &&
    desiredQuantityValue > 0 &&
    Number.isFinite(maxTotalPriceValue) &&
    maxTotalPriceValue > 0
      ? maxTotalPriceValue / desiredQuantityValue
      : null

  const prices = usePrices({ productId: productIdValue }, { enabled: Boolean(productIdValue) })
  const referencePrice = prices.data?.items.find((p) => p.regionId === regionIdValue)
  const deviationPercent =
    derivedMaxPricePerUnit !== null && referencePrice && referencePrice.price > 0
      ? ((derivedMaxPricePerUnit - referencePrice.price) / referencePrice.price) * 100
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
            name="desiredQuantity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Số lượng cần mua ({form.watch('unit') || 'kg'})</FormLabel>
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
                  <UnitSelect value={field.value} onChange={field.onChange} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

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
          name="maxTotalPrice"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tổng giá tối đa sẵn sàng trả (VNĐ, tuỳ chọn)</FormLabel>
              <FormControl>
                <Input type="number" step="1000" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {desiredQuantityValue > 0 ? (
          <p className="-mt-2 text-xs text-text-muted">
            Cần mua: <span className="font-medium text-text">{desiredQuantityValue}{unitValue}</span>.
            {derivedMaxPricePerUnit ? (
              <>
                {' '}
                Bạn đang muốn mua với giá{' '}
                <span className="font-medium text-primary">{currencyFormatter.format(derivedMaxPricePerUnit)}</span>/
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
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Mô tả (tuỳ chọn)</FormLabel>
              <FormControl>
                <Textarea placeholder="Mô tả thêm nhu cầu mua..." {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {errorMessage ? <p className="text-sm font-medium text-error">{errorMessage}</p> : null}

        <Button type="submit" className="w-full" disabled={createBuyRequest.isPending}>
          {createBuyRequest.isPending ? 'Đang đăng...' : 'Đăng yêu cầu mua'}
        </Button>
      </form>
    </Form>
  )
}
