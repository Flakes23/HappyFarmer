import { useRef, useState } from 'react'
import { toast } from 'sonner'
import { Camera, Loader2, X } from 'lucide-react'
import { useUploadDiseasePhoto } from '@/hooks/mutations/useUploadDiseasePhoto'

interface DiseasePhotoUploadFieldProps {
  value: string | null
  onChange: (url: string | null) => void
  onUploadingChange?: (uploading: boolean) => void
}

export function DiseasePhotoUploadField({ value, onChange, onUploadingChange }: DiseasePhotoUploadFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const upload = useUploadDiseasePhoto()
  const [isUploading, setIsUploading] = useState(false)

  async function handleFile(file: File | undefined) {
    if (!file) return

    setIsUploading(true)
    onUploadingChange?.(true)
    try {
      onChange(await upload.mutateAsync(file))
    } catch {
      toast.error('Tải ảnh lên thất bại. Vui lòng thử lại.')
    } finally {
      setIsUploading(false)
      onUploadingChange?.(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  return (
    <div className="space-y-2">
      {value ? (
        <div className="relative h-48 w-full max-w-xs overflow-hidden rounded-lg border border-border">
          <img src={value} alt="Ảnh cây trồng" className="h-full w-full object-cover" />
          <button
            type="button"
            onClick={() => onChange(null)}
            aria-label="Xoá ảnh"
            className="absolute right-1.5 top-1.5 rounded-full bg-black/60 p-1 text-white"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => inputRef.current?.click()}
          disabled={isUploading}
          className="flex h-48 w-full max-w-xs flex-col items-center justify-center gap-2 rounded-lg border border-dashed border-border text-text-muted hover:border-primary hover:text-primary disabled:opacity-50"
        >
          {isUploading ? <Loader2 className="h-6 w-6 animate-spin" /> : <Camera className="h-6 w-6" />}
          <span className="text-sm">{isUploading ? 'Đang tải ảnh...' : 'Chụp/tải ảnh cây trồng'}</span>
        </button>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={(e) => handleFile(e.target.files?.[0])}
      />
    </div>
  )
}
