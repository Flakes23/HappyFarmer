interface IllustrationProps {
  src: string
  alt?: string
  className?: string
}

export function Illustration({ src, alt = '', className }: IllustrationProps) {
  return <img src={src} alt={alt} className={className} />
}
