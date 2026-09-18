import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { stixApi } from '@/api'
import { queryKeys } from './queryKeys'
import type { StixBundle } from '@/types'

export function useStixUploadMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (bundle: StixBundle | unknown) => stixApi.uploadStix(bundle),
    onSuccess: () => {
      // Nach erfolgreichem STIX Ingest werden im Aggregator neue Incidents und Relationen erzeugt
      queryClient.invalidateQueries({ queryKey: queryKeys.incidents.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.relationships.all })
    },
  })
}

export const validateStixBundle = stixApi.validateStixBundle
