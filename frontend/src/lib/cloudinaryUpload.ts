import axios from 'axios'
import type { UploadSignatureResponse } from '@/api/types'

interface CloudinaryUploadResult {
  secure_url: string
}

/**
 * Upload thẳng từ browser lên Cloudinary bằng chữ ký lấy từ Marketplace Service
 * (xem GET /api/marketplace/uploads/signature) — không đi qua API Gateway vì đích là
 * api.cloudinary.com, không phải backend HappyFarmer.
 */
export async function uploadImageToCloudinary(file: File, sig: UploadSignatureResponse): Promise<string> {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('api_key', sig.apiKey)
  formData.append('timestamp', String(sig.timestamp))
  formData.append('signature', sig.signature)
  formData.append('folder', sig.folder)

  const res = await axios.post<CloudinaryUploadResult>(
    `https://api.cloudinary.com/v1_1/${sig.cloudName}/image/upload`,
    formData
  )

  return res.data.secure_url
}
