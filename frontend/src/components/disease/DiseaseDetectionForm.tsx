import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { DiseasePhotoUploadField } from '@/components/disease/DiseasePhotoUploadField'
import { useCreateDiseaseDetection } from '@/hooks/mutations/useCreateDiseaseDetection'
import { diseaseDetectionSchema, type DiseaseDetectionFormValues } from '@/schemas/aiAdvisorySchemas'
import { extractApiErrorMessage } from '@/api/authApi'
import { extractInvalidPlantImage, type InvalidPlantImageDetail } from '@/api/aiAdvisoryApi'
import type { DiseaseDetectionResponse } from '@/api/types'

interface DiseaseDetectionFormProps {
  onResult: (result: DiseaseDetectionResponse) => void
  onInvalidImage: (invalid: InvalidPlantImageDetail | null) => void
}

export function DiseaseDetectionForm({ onResult, onInvalidImage }: DiseaseDetectionFormProps) {
  const detect = useCreateDiseaseDetection()
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [isUploadingPhoto, setIsUploadingPhoto] = useState(false)

  const form = useForm<DiseaseDetectionFormValues>({
    resolver: zodResolver(diseaseDetectionSchema),
    defaultValues: { cropTypeHint: '', note: '' },
  })

  function onSubmit(values: DiseaseDetectionFormValues) {
    if (!imageUrl) {
      toast.error('Vui lòng chụp hoặc tải lên ảnh cây trồng trước khi gửi.')
      return
    }

    const parsed = diseaseDetectionSchema.parse(values)
    onInvalidImage(null)

    detect.mutate(
      { imageUrl, cropTypeHint: parsed.cropTypeHint || undefined, note: parsed.note || undefined },
      {
        onSuccess: (result) => {
          onResult(result)
          toast.success('Đã có kết quả chẩn đoán!')
          setImageUrl(null)
          form.reset()
        },
        onError: (err) => {
          const invalid = extractInvalidPlantImage(err)
          if (invalid) {
            onInvalidImage(invalid)
            return
          }
          toast.error(extractApiErrorMessage(err, 'Không thể chẩn đoán, vui lòng thử lại.'))
        },
      }
    )
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <div className="space-y-2">
          <Label>Ảnh cây trồng</Label>
          <DiseasePhotoUploadField value={imageUrl} onChange={setImageUrl} onUploadingChange={setIsUploadingPhoto} />
        </div>

        <FormField
          control={form.control}
          name="cropTypeHint"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Loại cây (tuỳ chọn)</FormLabel>
              <FormControl>
                <Input placeholder="Vd: Cà chua" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="note"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Ghi chú thêm (tuỳ chọn)</FormLabel>
              <FormControl>
                <Textarea placeholder="Mô tả triệu chứng, vị trí phát hiện trên cây..." {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={detect.isPending || isUploadingPhoto}>
          {detect.isPending ? 'Đang chẩn đoán...' : 'Nhận diện bệnh'}
        </Button>
      </form>
    </Form>
  )
}
