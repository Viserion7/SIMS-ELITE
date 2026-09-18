import { stixApiClient } from './client'
import type { StixBundle } from '@/types'

export const stixApi = {
  uploadStix: async (bundle: StixBundle | unknown): Promise<void> => {
    await stixApiClient<void>('/api/v1/Stix', {
      method: 'PUT',
      body: bundle,
    })
  },

  validateStixBundle: (data: unknown): { valid: boolean; error?: string } => {
    if (!data || typeof data !== 'object') {
      return { valid: false, error: 'Die Datei enthält kein gültiges JSON-Objekt.' }
    }

    const doc = data as Record<string, unknown>

    if (!doc.id || typeof doc.id !== 'string') {
      return { valid: false, error: 'STIX Dokument erfordert ein "id" Attribut.' }
    }

    if (doc.type !== 'bundle') {
      return { valid: false, error: 'STIX Dokument "type" muss "bundle" sein.' }
    }

    if (!Array.isArray(doc.objects) || doc.objects.length === 0) {
      return { valid: false, error: 'STIX Bundle erfordert ein nicht-leeres "objects" Array.' }
    }

    for (const item of doc.objects) {
      if (!item || typeof item !== 'object' || !item.id || !item.type) {
        return {
          valid: false,
          error: 'Jedes Element in "objects" muss ein Objekt mit "id" und "type" sein.',
        }
      }
    }

    return { valid: true }
  },
}
