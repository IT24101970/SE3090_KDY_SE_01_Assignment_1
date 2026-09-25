import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true, // Listens on 0.0.0.0 allowing phone access on Wi-Fi
    port: 5173
  },
  test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: './src/test/setup.js',
  },
})
