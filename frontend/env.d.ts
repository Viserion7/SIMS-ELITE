/// <reference types="vite/client" />

interface AppConfig {
  /** Pfade oder URLs für die Backend Microservices.
   *  Relative Pfade (z. B. /api/identity) nutzen den Nginx- bzw. Vite-Proxy (Same-Origin).
   */
  identityUrl?: string
  aggregatorUrl?: string
  incidentManagerUrl?: string
  stixUrl?: string
}

declare module '*.md?raw' {
  const content: string
  export default content
}

declare global {
  interface Window {
    APP_CONFIG?: AppConfig
  }
}

export {}
