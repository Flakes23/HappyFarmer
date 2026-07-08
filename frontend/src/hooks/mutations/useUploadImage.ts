import { useMutation } from '@tanstack/react-query'
import { marketplaceApi } from '@/api/marketplaceApi'
import { uploadImageToCloudinary } from '@/lib/cloudinaryUpload'

export function useUploadImage() {
  return useMutation({
    mutationFn: async (file: File) => {
      const signature = await marketplaceApi.getUploadSignature()
      return uploadImageToCloudinary(file, signature)
    },
  })
}
