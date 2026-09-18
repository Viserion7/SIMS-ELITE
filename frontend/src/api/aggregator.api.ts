import { aggregatorApiClient } from './client'
import type { Incident, CreateIncidentDto, Relationship } from '@/types'

export const aggregatorApi = {
  // Incidents (Hinweis: Backend nutzt 0-basiertes Paging via .Skip(page * count))
  getIncidents: async (page: number = 0, count: number = 20): Promise<Incident[]> => {
    const res = await aggregatorApiClient<Incident[]>(`/api/v1/Incidents/${page}/${count}`, {
      method: 'GET',
    })
    return res ?? []
  },

  getIncidentById: async (id: string): Promise<Incident> => {
    const res = await aggregatorApiClient<Incident>(`/api/v1/Incidents/${id}`, {
      method: 'GET',
    })
    return res!
  },

  createIncident: async (dto: CreateIncidentDto): Promise<void> => {
    await aggregatorApiClient<void>('/api/v1/Incidents', {
      method: 'POST',
      body: dto,
    })
  },

  deleteIncident: async (id: string): Promise<void> => {
    await aggregatorApiClient<void>(`/api/v1/Incidents/${id}`, {
      method: 'DELETE',
    })
  },

  // Relationships (0-basiertes Paging)
  getRelationships: async (page: number = 0, count: number = 50): Promise<Relationship[]> => {
    const res = await aggregatorApiClient<Relationship[]>(`/api/v1/Relationship/${page}/${count}`, {
      method: 'GET',
    })
    return res ?? []
  },

  getRelationshipById: async (id: string): Promise<Relationship> => {
    const res = await aggregatorApiClient<Relationship>(`/api/v1/Relationship/${id}`, {
      method: 'GET',
    })
    return res!
  },

  getRelationshipsByIncident: async (incidentId: string): Promise<Relationship[]> => {
    const res = await aggregatorApiClient<Relationship[]>(
      `/api/v1/Relationship/incident/${incidentId}`,
      {
        method: 'GET',
      },
    )
    return res ?? []
  },

  createRelationship: async (dto: Relationship): Promise<void> => {
    await aggregatorApiClient<void>('/api/v1/Relationship', {
      method: 'POST',
      body: dto,
    })
  },
}
