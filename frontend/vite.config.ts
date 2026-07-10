import path from 'node:path'
import { defineConfig, type Plugin } from 'vite'
import react from '@vitejs/plugin-react'

// Trang chủ (route "/") hiển thị ngay khi vào web dùng font weight 400/600 (xem HomePage.tsx) —
// preload đúng 2 file font đó (subset latin + vietnamese) để trình duyệt tải song song với CSS
// thay vì phải đợi CSS tải xong mới "biết" cần tải font (nguyên nhân chuỗi phụ thuộc dài trong
// Lighthouse: HTML -> CSS -> font). Tên file có hash đổi mỗi build nên phải dò trong bundle,
// không ghi cứng đường dẫn được.
function preloadCriticalFonts(): Plugin {
  const criticalFontPattern = /be-vietnam-pro-(latin|vietnamese)-(400|600)-normal-.*\.woff2$/
  let fontFileNames: string[] = []

  return {
    name: 'preload-critical-fonts',
    generateBundle(_, bundle) {
      fontFileNames = Object.keys(bundle).filter((fileName) => criticalFontPattern.test(fileName))
    },
    transformIndexHtml() {
      return fontFileNames.map((fileName) => ({
        tag: 'link',
        attrs: {
          rel: 'preload',
          href: `/${fileName}`,
          as: 'font',
          type: 'font/woff2',
          crossorigin: 'anonymous',
        },
        injectTo: 'head-prepend' as const,
      }))
    },
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), preloadCriticalFonts()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
  },
})
