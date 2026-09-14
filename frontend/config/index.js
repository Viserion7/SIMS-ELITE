/**
 * Frontend Konfiguration
 *
 * Priorität:
 * 1. window.APP_CONFIG  → Docker Runtime (gesetzt durch docker-entrypoint.sh)
 * 2. __APP_CONFIG__     → Vite Build-Zeit (aus config.yaml, für lokale Entwicklung)
 * 3. Hardcoded Fallback → Letzte Absicherung
 *
 * Endpunkte werden NICHT aus der Config gelesen – sie sind direkt im Code hardcoded.
 * Nur Base-URLs sind umgebungsabhängig und daher konfigurierbar.
 */

// Build-Zeit Konfiguration (Vite, lokale Entwicklung)
// FIX: __APP_CONFIG__ wird explizit als globale Variable deklariert
// Das macht sie in ESLint sichtbar und verhindert den "undefined" Fehler
// eslint-disable-next-line no-undef
const appConfig = typeof __APP_CONFIG__ !== 'undefined' ? __APP_CONFIG__ : {}

// Laufzeit-Konfiguration aus public/config.js (Docker, überschreibt Build-Zeit)
const runtimeConfig = typeof window !== 'undefined' && window.APP_CONFIG ? window.APP_CONFIG : {}

// Firebase Client Konfiguration
export const firebaseConfig = {
  apiKey: runtimeConfig.FIREBASE_API_KEY || appConfig.firebase?.apiKey || '',
  authDomain: runtimeConfig.FIREBASE_AUTH_DOMAIN || appConfig.firebase?.authDomain || '',
  projectId: runtimeConfig.FIREBASE_PROJECT_ID || appConfig.firebase?.projectId || '',
  storageBucket: runtimeConfig.FIREBASE_STORAGE_BUCKET || appConfig.firebase?.storageBucket || '',
  messagingSenderId:
    runtimeConfig.FIREBASE_MESSAGING_SENDER_ID || appConfig.firebase?.messagingSenderId || '',
  appId: runtimeConfig.FIREBASE_APP_ID || appConfig.firebase?.appId || '',
}

// Umgebung
export const nodeEnv = runtimeConfig.NODE_ENV || appConfig.nodeEnv || 'development'

// MFA Policy (off | optional; required später) — synced from config.yaml
const rawMfaMode = runtimeConfig.MFA_MODE || appConfig.mfa?.mode || 'off'
export const mfaMode = rawMfaMode === 'optional' ? 'optional' : 'off'
export const mfaEnabled = mfaMode !== 'off'

// App Version & Stage (VITE_APP_VERSION wird per GitHub Action beim Build gesetzt)
export const appVersion = import.meta.env.VITE_APP_VERSION || '0.1.2'
export const appStage = 'BETA'

// Backend Base URL – der einzige konfigurierbare API-Wert
const backendBaseUrl =
  runtimeConfig.API_BACKEND_BASE_URL || appConfig.api?.backendBaseUrl || 'http://localhost:3033'

// Helper: vollständige API URL bauen
export const getApiUrl = (endpoint) => {
  const base = backendBaseUrl.replace(/\/$/, '')
  const path = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
  return `${base}${path}`
}

export default {
  firebase: firebaseConfig,
  backendBaseUrl,
  getApiUrl,
  mfaMode,
  mfaEnabled,
}
