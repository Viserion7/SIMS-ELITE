/// <reference types="vite/client" />

interface AppConfig {
  /** Leerer String = same-origin über den Nginx-Proxy. */
  identityUrl?: string
  aggregatorUrl?: string
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
