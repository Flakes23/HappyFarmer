import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { format } from 'date-fns'
import { vi } from 'date-fns/locale'
import { CalendarIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Calendar } from '@/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { WeatherForecastPreview } from '@/components/harvest/WeatherForecastPreview'
import { useCreateHarvestPrediction } from '@/hooks/mutations/useCreateHarvestPrediction'
import { harvestPredictionSchema, type HarvestPredictionFormValues } from '@/schemas/aiAdvisorySchemas'
import { extractApiErrorMessage } from '@/api/authApi'
import { useAuthStore } from '@/store/authStore'
import { useProvinces } from '@/hooks/queries/useProvinces'
import { cn, parseIsoDate } from '@/lib/utils'
import type { HarvestPredictionResponse } from '@/api/types'

interface HarvestPredictionFormProps {
  onResult: (result: HarvestPredictionResponse) => void
}

export function HarvestPredictionForm({ onResult }: HarvestPredictionFormProps) {
  const predict = useCreateHarvestPrediction()
  const [plantingDateOpen, setPlantingDateOpen] = useState(false)
  const user = useAuthStore((s) => s.user)
  const provinces = useProvinces()

  const form = useForm<HarvestPredictionFormValues>({
    resolver: zodResolver(harvestPredictionSchema),
    defaultValues: { cropType: '', plantingDate: '', location: '' },
  })

  // provinces tải bất đồng bộ nên không thể đưa thẳng vào defaultValues (chỉ áp dụng 1 lần lúc mount) —
  // set khi tải xong, chỉ nếu người dùng chưa tự chọn khu vực khác.
  useEffect(() => {
    if (!provinces.data || form.getValues('location')) return
    const match = provinces.data.find((p) => p.id === user?.provinceId)
    if (match) form.setValue('location', match.name)
  }, [provinces.data, user?.provinceId, form])

  const location = form.watch('location')

  function onSubmit(values: HarvestPredictionFormValues) {
    const parsed = harvestPredictionSchema.parse(values)
    predict.mutate(parsed, {
      onSuccess: (result) => {
        onResult(result)
        toast.success('Đã có kết quả dự đoán!')
      },
      onError: (err) => {
        toast.error(extractApiErrorMessage(err, 'Không thể dự đoán, vui lòng thử lại.'))
      },
    })
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <WeatherForecastPreview location={location || undefined} />

        <FormField
          control={form.control}
          name="location"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Khu vực</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value || undefined}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn tỉnh/thành" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {(provinces.data ?? []).map((p) => (
                    <SelectItem key={p.id} value={p.name}>
                      {p.name}
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
          name="cropType"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Cây trồng</FormLabel>
              <FormControl>
                <Input placeholder="Vd: Lúa" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="plantingDate"
          render={({ field }) => (
            <FormItem className="flex flex-col">
              <FormLabel>Ngày trồng</FormLabel>
              <Popover open={plantingDateOpen} onOpenChange={setPlantingDateOpen}>
                <PopoverTrigger asChild>
                  <FormControl>
                    <Button
                      type="button"
                      variant="outline"
                      className={cn('justify-start text-left font-normal', !field.value && 'text-text-muted')}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {field.value ? format(parseIsoDate(field.value), 'dd/MM/yyyy', { locale: vi }) : 'Chọn ngày trồng'}
                    </Button>
                  </FormControl>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    locale={vi}
                    className="bg-surface"
                    selected={field.value ? parseIsoDate(field.value) : undefined}
                    onSelect={(date) => {
                      field.onChange(date ? format(date, 'yyyy-MM-dd') : '')
                      setPlantingDateOpen(false)
                    }}
                  />
                </PopoverContent>
              </Popover>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={predict.isPending}>
          {predict.isPending ? 'Đang phân tích...' : 'Dự đoán thời điểm thu hoạch'}
        </Button>
      </form>
    </Form>
  )
}
