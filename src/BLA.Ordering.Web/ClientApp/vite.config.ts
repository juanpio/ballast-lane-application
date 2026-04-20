import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { visualizer } from 'rollup-plugin-visualizer'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    ...(process.env.BUNDLE_ANALYZE === 'true'
      ? [
          visualizer({
            filename: 'dist/bundle-stats.html',
            gzipSize: true,
            brotliSize: true,
            open: false,
          }),
        ]
      : []),
  ],
})
