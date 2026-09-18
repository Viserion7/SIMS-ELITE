import { fileURLToPath, URL } from 'node:url'

import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  const identityTarget = env.VITE_IDENTITY_URL || 'http://localhost:67'
  const aggregatorTarget = env.VITE_AGGREGATOR_URL || 'http://localhost:88'
  const incidentManagerTarget = env.VITE_INCIDENT_MANAGER_URL || 'http://localhost:420'
  const stixTarget = env.VITE_STIX_URL || 'http://localhost:8080'

  return {
    plugins: [
      vue(),
      vueDevTools(),
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      proxy: {
        '/api/identity': {
          target: identityTarget,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api\/identity/, ''),
        },
        '/api/aggregator': {
          target: aggregatorTarget,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api\/aggregator/, ''),
        },
        '/api/incident-manager': {
          target: incidentManagerTarget,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api\/incident-manager/, ''),
        },
        '/api/stix': {
          target: stixTarget,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api\/stix/, ''),
        },
      },
    },
  }
})
