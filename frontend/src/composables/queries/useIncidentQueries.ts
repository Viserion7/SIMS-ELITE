import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { aggregatorApi } from '@/api'
import { queryKeys } from './queryKeys'
import type { CreateIncidentDto, Relationship } from '@/types'

export function useIncidentsQuery(
  page: MaybeRefOrGetter<number> = 0,
  count: MaybeRefOrGetter<number> = 20,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.incidents.list(toValue(page), toValue(count))),
    queryFn: () => aggregatorApi.getIncidents(toValue(page), toValue(count)),
  })
}

export function useIncidentDetailQuery(id: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => queryKeys.incidents.detail(toValue(id))),
    queryFn: () => aggregatorApi.getIncidentById(toValue(id)),
    enabled: () => !!toValue(id),
  })
}

export function useIncidentRelationshipsQuery(incidentId: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => queryKeys.incidents.relationships(toValue(incidentId))),
    queryFn: () => aggregatorApi.getRelationshipsByIncident(toValue(incidentId)),
    enabled: () => !!toValue(incidentId),
  })
}

export function useRelationshipsQuery(
  page: MaybeRefOrGetter<number> = 0,
  count: MaybeRefOrGetter<number> = 50,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.relationships.list(toValue(page), toValue(count))),
    queryFn: () => aggregatorApi.getRelationships(toValue(page), toValue(count)),
  })
}

export function useCreateIncidentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: CreateIncidentDto) => aggregatorApi.createIncident(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.incidents.all })
    },
  })
}

export function useDeleteIncidentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => aggregatorApi.deleteIncident(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.incidents.all })
    },
  })
}

export function useCreateRelationshipMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: Relationship) => aggregatorApi.createRelationship(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.relationships.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.incidents.all })
    },
  })
}
