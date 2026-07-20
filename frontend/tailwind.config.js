import defaultTheme from 'tailwindcss/defaultTheme'

/** @type {import('tailwindcss').Config} */
export default {
	darkMode: ['class'],
	content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
	theme: {
		extend: {
			fontFamily: {
				sans: ['"Be Vietnam Pro"', ...defaultTheme.fontFamily.sans]
			},
			borderRadius: {
				lg: 'var(--radius)',
				md: 'calc(var(--radius) - 2px)',
				sm: 'calc(var(--radius) - 4px)'
			},
			// Elevation scale Ã¢â‚¬â€ warm-tinted (via --shadow-color), not pure black. `shadow`
			// (Tailwind default) stays the resting elevation on Card; these are for
			// hover/interactive/overlay states. See index.css for --shadow-color per theme.
			// NOTE: none of these keys may match an existing `colors.*` token name (e.g. `card`,
			// `primary`) Ã¢â‚¬â€ Tailwind's built-in shadow-color utilities silently win that collision
			// and produce a white/wrong-color shadow instead of this config's value.
			boxShadow: {
				raised: '0 4px 8px -2px rgb(var(--shadow-color) / 0.12), 0 2px 4px -2px rgb(var(--shadow-color) / 0.08)',
				floating: '0 12px 24px -4px rgb(var(--shadow-color) / 0.16), 0 4px 8px -4px rgb(var(--shadow-color) / 0.08)',
				modal: '0 20px 40px -8px rgb(var(--shadow-color) / 0.24), 0 8px 16px -8px rgb(var(--shadow-color) / 0.12)'
			},
			// Type scale Ã¢â‚¬â€ pair size/lineHeight/weight so pages pick e.g. `text-h1`
			// instead of ad hoc `text-3xl font-semibold`. `body` floors at 16px
			// (accessibility: default readable size for older/low-tech-literacy users).
			fontSize: {
				display: ['2.5rem', { lineHeight: '1.15', fontWeight: '700', letterSpacing: '-0.02em' }],
				h1: ['2rem', { lineHeight: '1.2', fontWeight: '700' }],
				h2: ['1.5rem', { lineHeight: '1.3', fontWeight: '600' }],
				h3: ['1.25rem', { lineHeight: '1.4', fontWeight: '600' }],
				body: ['1rem', { lineHeight: '1.6', fontWeight: '400' }],
				'body-sm': ['0.9375rem', { lineHeight: '1.5', fontWeight: '400' }],
				caption: ['0.875rem', { lineHeight: '1.4', fontWeight: '400' }]
			},
			// Shared easing/duration for both framer-motion (imported from lib/motion.ts)
			// and plain CSS transitions (hover states), so the two systems feel the same.
			transitionTimingFunction: {
				brand: 'cubic-bezier(0.16, 1, 0.3, 1)'
			},
			transitionDuration: {
				brand: '300ms'
			},
			// `transform`-based (not `background-position`) so the sweep runs on the GPU
			// compositor instead of repainting every frame -- matters when many skeleton
			// rows animate at once (e.g. an 8-card grid x 6 bars each).
			keyframes: {
				shimmer: {
					'0%': { transform: 'translateX(-100%)' },
					'100%': { transform: 'translateX(100%)' }
				}
			},
			animation: {
				shimmer: 'shimmer 1.6s ease-in-out infinite'
			},
			colors: {
				// shadcn/ui semantic tokens (used by generated components)
				background: 'var(--background)',
				foreground: 'var(--foreground)',
				card: {
					DEFAULT: 'var(--card)',
					foreground: 'var(--card-foreground)'
				},
				popover: {
					DEFAULT: 'var(--popover)',
					foreground: 'var(--popover-foreground)'
				},
				primary: {
					DEFAULT: 'var(--primary)',
					foreground: 'var(--primary-foreground)',
					light: 'var(--primary-light)'
				},
				secondary: {
					DEFAULT: 'var(--secondary)',
					foreground: 'var(--secondary-foreground)'
				},
				muted: {
					DEFAULT: 'var(--muted)',
					foreground: 'var(--muted-foreground)'
				},
				accent: {
					DEFAULT: 'var(--accent)',
					foreground: 'var(--accent-foreground)'
				},
				destructive: {
					DEFAULT: 'var(--destructive)',
					foreground: 'var(--destructive-foreground)'
				},
				border: 'var(--border)',
				input: 'var(--input)',
				ring: 'var(--ring)',

				// HappyFarmer palette Ã¢â‚¬â€ direct aliases matching the design spec
				// vocabulary (mÃ¡Â»Â¥c 1 cÃ¡Â»Â§a plan). Prefer these names in app code.
				surface: 'var(--card)',
				text: {
					DEFAULT: 'var(--foreground)',
					muted: 'var(--muted-foreground)'
				},
				error: {
					DEFAULT: 'var(--destructive)',
					foreground: 'var(--destructive-foreground)'
				},
				success: {
					DEFAULT: 'var(--success)',
					foreground: 'var(--success-foreground)'
				}
			}
		}
	},
	plugins: [require('tailwindcss-animate')],
}
