import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { useProducts } from '@/hooks/queries/useProducts'
import { useRegions } from '@/hooks/queries/useRegions'
import { useCreateBuyRequest } from '@/hooks/mutations/useCreateBuyRequest'
import { createBuyRequestSchema, type CreateBuyRequestFormValues } from '@/schemas/marketplaceSchemas'
import { extractApiErrorMessage } from '@/api/authApi'

export function BuyRequestForm() {
  const navigate = useNavigate()
  const products = useProducts()
  const regions = useRegions()
  const createBuyRequest = useCreateBuyRequest()

  const form = useForm<CreateBuyRequestFormValues>({
    resolver: zodResolver(createBuyRequestSchema),
    defaultValues: {
      productId: undefined,
      desiredQuantity: '',
      regionId: undefined,
      maxPricePerUnit: '',
      description: '',
    },
  })

  function onSubmit(values: CreateBuyRequestFormValues) {
    const parsed = createBuyRequestSchema.parse(values)
    createBuyRequest.mutate(parsed, {
      onSuccess: () => {
        toast.success('Đăng yêu cầu mua thành công!')
        navigate('/marketplace')
      },
    })
  }

  const errorMessage = createBuyRequest.isError
    ? extractApiErrorMessage(createBuyRequest.error, 'Đăng yêu cầu thất bại. Vui lòng thử lại.')
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

        <FormField
          control={form.control}
          name="desiredQuantity"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Số lượng cần mua</FormLabel>
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
          name="maxPricePerUnit"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Giá tối đa / đơn vị (tuỳ chọn)</FormLabel>
              <FormControl>
                <Input type="number" step="0.01" {...field} />
              </FormControl>
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
