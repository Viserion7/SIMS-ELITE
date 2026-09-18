import { useMutation } from '@tanstack/vue-query'
import { incidentManagerApi } from '@/api'
import type { EscalateRequestDto } from '@/types'

export function useEscalateMutation() {
  return useMutation({
    mutationFn: (dto: EscalateRequestDto) => incidentManagerApi.escalate(dto),
  })
}
