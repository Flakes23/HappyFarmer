import { z } from 'zod'

export const harvestPredictionSchema = z.object({
  cropType: z.string().trim().min(1, 'Nhập tên cây trồng'),
  plantingDate: z.string().min(1, 'Chọn ngày trồng'),
  location: z.string().min(1, 'Chọn tỉnh/thành'),
})

export type HarvestPredictionFormValues = z.input<typeof harvestPredictionSchema>
export type HarvestPredictionFormOutput = z.output<typeof harvestPredictionSchema>

export const diseaseDetectionSchema = z.object({
  cropTypeHint: z.string().trim().optional(),
  note: z.string().trim().optional(),
})

export type DiseaseDetectionFormValues = z.input<typeof diseaseDetectionSchema>
export type DiseaseDetectionFormOutput = z.output<typeof diseaseDetectionSchema>
