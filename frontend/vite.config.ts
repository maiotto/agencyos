import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/working-calendars': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/holidays': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/working-hours': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/resource-availabilities': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/execution-resources': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/capacity': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/workload': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/planning-templates': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/portfolios': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/recommendations': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/decisions': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/decision-profiles': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/audit': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/ai-recommendations': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/explainability': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/executive-summaries': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/companies': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/enterprise-dashboard': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/my-work': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/planning-workspace': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/recommendation-workspace': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/decision-workspace': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/executive-workspace': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/notifications': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/personal-dashboard': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/portfolio-analytics': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
      '/cross-portfolio-planning': {
        target: 'http://localhost:5115',
        changeOrigin: true,
      },
    },
  },
})
