import { incidentManagerApiClient } from './client'
import type { EscalateRequestDto } from '@/types'

export const incidentManagerApi = {
  escalate: async (dto: EscalateRequestDto): Promise<void> => {
    await incidentManagerApiClient<void>('/api/v1/Eskalation', {
      method: 'POST',
      body: dto,
    })
  },
}
