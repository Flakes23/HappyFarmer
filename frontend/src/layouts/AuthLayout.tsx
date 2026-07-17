import type { ReactNode } from 'react'
import { motion } from 'framer-motion'
import { useMotionVariants } from '@/lib/motion'
import { Illustration } from '@/components/shared/Illustration'

interface AuthLayoutProps {
  illustration: string
  children: ReactNode
}

/** Shared wrapper for Login/Register/ForgotPassword — form column (untouched Card/Form internals) + illustration column, collapsing to form-only below `lg:`. Stays inside RootLayout's existing `max-w-5xl` rather than breaking out full-bleed, to avoid viewport-width overflow risk. */
export function AuthLayout({ illustration, children }: AuthLayoutProps) {
  const { fadeInUp } = useMotionVariants()

  return (
    <div className="grid items-center gap-10 py-4 lg:grid-cols-2">
      <motion.div initial="hidden" animate="visible" variants={fadeInUp} className="mx-auto w-full max-w-sm">
        {children}
      </motion.div>
      <div className="hidden justify-center lg:flex">
        <Illustration src={illustration} className="w-full max-w-md" />
      </div>
    </div>
  )
}
