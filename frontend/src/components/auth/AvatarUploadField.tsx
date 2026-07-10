import { useRef, useState } from 'react'
import { toast } from 'sonner'
import { Camera, Loader2 } from 'lucide-react'
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar'
import { useUploadAvatar } from '@/hooks/mutations/useUploadAvatar'

interface AvatarUploadFieldProps {
  value: string | null
  onChange: (url: string) => void
  fallbackText: string
}

/** Upload trực tiếp lên Cloudinary (signed upload, xem useUploadAvatar) rồi gắn URL trả về vào form. */
export function AvatarUploadField({ value, onChange, fallbackText }: AvatarUploadFieldProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const upload = useUploadAvatar()
  const [isUploading, setIsUploading] = useState(false)

  async function handleFile(file: File | undefined) {
    if (!file) return

    setIsUploading(true)
    try {
      onChange(await upload.mutateAsync(file))
    } catch {
      toast.error('Tải ảnh lên thất bại. Vui lòng thử lại.')
    } finally {
      setIsUploading(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  return (
    <div className="flex flex-col items-center gap-2">
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={isUploading}
        aria-label="Đổi ảnh đại diện"
        className="group relative h-28 w-28 shrink-0 rounded-full disabled:opacity-50"
      >
        <Avatar className="h-28 w-28">
          <AvatarImage src={value ?? undefined} alt="Ảnh đại diện" />
          <AvatarFallback className="bg-primary-light text-3xl text-white">
            {fallbackText}
          </AvatarFallback>
        </Avatar>
        <span className="absolute inset-0 flex items-center justify-center rounded-full bg-black/40 opacity-0 transition-opacity group-hover:opacity-100">
          {isUploading ? (
            <Loader2 className="h-6 w-6 animate-spin text-white" />
          ) : (
            <Camera className="h-6 w-6 text-white" />
          )}
        </span>
      </button>

      <span className="text-xs text-text-muted">Nhấn để đổi ảnh</span>

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
