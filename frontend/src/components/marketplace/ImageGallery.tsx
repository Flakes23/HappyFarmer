import { useState } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface ImageGalleryProps {
  images: string[]
  alt?: string
}

export function ImageGallery({ images, alt }: ImageGalleryProps) {
  const [open, setOpen] = useState(false)
  const [index, setIndex] = useState(0)

  if (images.length === 0) return null

  function openAt(i: number) {
    setIndex(i)
    setOpen(true)
  }

  function prev() {
    setIndex((i) => (i - 1 + images.length) % images.length)
  }

  function next() {
    setIndex((i) => (i + 1) % images.length)
  }

  return (
    <>
      <div className="flex flex-wrap gap-2">
        {images.map((url, i) => (
          <button
            key={url}
            type="button"
            onClick={() => openAt(i)}
            className="h-24 w-24 overflow-hidden rounded-md border border-border transition-opacity hover:opacity-80"
          >
            <img src={url} alt={alt ? `${alt} ${i + 1}` : `Ảnh ${i + 1}`} className="h-full w-full object-cover" />
          </button>
        ))}
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="max-w-3xl border-none bg-transparent p-0 shadow-none">
          <DialogTitle className="sr-only">{alt ? `Ảnh ${alt}` : 'Xem ảnh'}</DialogTitle>
          <div className="relative flex items-center justify-center">
            <img
              src={images[index]}
              alt={alt ? `${alt} ${index + 1}` : `Ảnh ${index + 1}`}
              className="max-h-[80vh] w-full rounded-md object-contain"
            />

            {images.length > 1 ? (
              <>
                <Button
                  type="button"
                  variant="secondary"
                  size="icon"
                  aria-label="Ảnh trước"
                  onClick={prev}
                  className={cn('absolute left-2 top-1/2 -translate-y-1/2 rounded-full')}
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <Button
                  type="button"
                  variant="secondary"
                  size="icon"
                  aria-label="Ảnh sau"
                  onClick={next}
                  className={cn('absolute right-2 top-1/2 -translate-y-1/2 rounded-full')}
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
                <span className="absolute bottom-2 left-1/2 -translate-x-1/2 rounded-full bg-black/60 px-2 py-0.5 text-xs text-white">
                  {index + 1} / {images.length}
                </span>
              </>
            ) : null}
          </div>
        </DialogContent>
      </Dialog>
    </>
  )
}
