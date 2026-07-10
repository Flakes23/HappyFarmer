import { useMutation } from '@tanstack/react-query'
import { authApi } from '@/api/authApi'
import { uploadImageToCloudinary } from '@/lib/cloudinaryUpload'

export function useUploadAvatar() {
  return useMutation({
    mutationFn: async (file: File) => {
      const signature = await authApi.getUploadSignature()
      return uploadImageToCloudinary(file, signature)
    },
  })
}
