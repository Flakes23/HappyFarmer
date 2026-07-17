import type { Variants } from 'framer-motion'
import { useReducedMotion } from 'framer-motion'

export const MOTION_TRANSITION = { duration: 0.3, ease: [0.16, 1, 0.3, 1] } as const

export const fadeInUp: Variants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: MOTION_TRANSITION },
}

export const fadeIn: Variants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: MOTION_TRANSITION },
}

export const staggerContainer: Variants = {
  hidden: {},
  visible: {
    transition: { staggerChildren: 0.08, delayChildren: 0.05 },
  },
}

export const scaleTap = { scale: 0.97 }

const REDUCED_FADE: Variants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { duration: 0.15 } },
}

const REDUCED_STAGGER: Variants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0 } },
}

/** Older/low-tech-literacy users are more likely to have `prefers-reduced-motion` on — swap in an opacity-only, non-staggered set instead of skipping the reveal entirely. */
export function useMotionVariants() {
  const reduced = useReducedMotion()
  if (reduced) {
    return {
      fadeInUp: REDUCED_FADE,
      fadeIn: REDUCED_FADE,
      staggerContainer: REDUCED_STAGGER,
      scaleTap: {},
      transition: { duration: 0.15 },
    }
  }
  return { fadeInUp, fadeIn, staggerContainer, scaleTap, transition: MOTION_TRANSITION }
}
