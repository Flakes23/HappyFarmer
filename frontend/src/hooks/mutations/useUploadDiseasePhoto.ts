import { useMutation } from '@tanstack/react-query'
import { aiAdvisoryApi } from '@/api/aiAdvisoryApi'
import { uploadImageToCloudinary } from '@/lib/cloudinaryUpload'

export function useUploadDiseasePhoto() {
  return useMutation({
    mutationFn: async (file: File) => {
      const signature = await aiAdvisoryApi.getDiseaseDetectionSignature()
      return uploadImageToCloudinary(file, signature)
    },
  })
}
