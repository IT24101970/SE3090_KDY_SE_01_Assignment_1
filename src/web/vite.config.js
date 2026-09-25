import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
<<<<<<< HEAD
  plugins: [react()],
  server: {
    host: true, // Listens on 0.0.0.0 allowing phone access on Wi-Fi
    port: 5173
  }
=======
  plugins: [react({ jsxRuntime: 'automatic' })],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.js',
  },
>>>>>>> 24cbf348f696adef91fd1390f65498b6e69388cb
})
