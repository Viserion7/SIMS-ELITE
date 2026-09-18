<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useIncidentsQuery, useDeleteIncidentMutation } from '@/composables/queries/useIncidentQueries'
import { useMyCategories } from '@/composables/useMyCategories'
import type { Incident } from '@/types'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
import IncidentCard from '@/components/IncidentCard.vue'
import Button from 'primevue/button'
import Skeleton from 'primevue/skeleton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'

const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const deleteMutation = useDeleteIncidentMutation()
const allowedTypes = useMyCategories()

const page = ref(0)
const count = ref(20)

const { data: rawIncidents, isPending, isError } = useIncidentsQuery(page, count)

const allLoadedIncidents = ref<Incident[]>([])
const hasMoreToLoad = ref(true)

watch(rawIncidents, (newIncidents) => {
  if (newIncidents) {
    if (newIncidents.length < count.value) {
      hasMoreToLoad.value = false
    }
    if (page.value === 0) {
      allLoadedIncidents.value = [...newIncidents]
    } else {
      const newItems = newIncidents.filter(n => !allLoadedIncidents.value.some(o => o.id === n.id))
      allLoadedIncidents.value.push(...newItems)
    }
  }
}, { immediate: true })

const loadMore = () => {
  if (!hasMoreToLoad.value) return
  page.value += 1
}

const goToDetail = (incident: Incident) => {
  router.push(`/incidents/${incident.id}`)
}

const handleDeleteIncident = (incident: Incident) => {
  confirm.require({
    message: `Möchtest du den Vorfall "${incident.name || incident.id}" wirklich als gesehen markieren? Er wird danach aus allen aktiven Listen ausgeblendet.`,
    header: 'Vorfall als gesehen markieren',
    icon: 'pi pi-check-circle',
    acceptLabel: 'Als gesehen markieren',
    rejectLabel: 'Abbrechen',
    acceptClass: 'p-button-success',
    accept: async () => {
      try {
        await deleteMutation.mutateAsync(incident.id)
        allLoadedIncidents.value = allLoadedIncidents.value.filter(i => i.id !== incident.id)
        toast.add({
          severity: 'success',
          summary: 'Als gesehen markiert',
          detail: 'Der Vorfall wurde erfolgreich als gesehen markiert.',
          life: 3000,
        })
      } catch (err: any) {
        toast.add({
          severity: 'error',
          summary: 'Fehler',
          detail: err?.message || 'Konnte nicht als gesehen markiert werden.',
          life: 4000,
        })
      }
    },
  })
}
</script>

<template>
  <div class="page-container">
    <PageHeader 
      title="Alle Incidents" 
      description="Globale Übersicht über alle verarbeiteten Sicherheitsvorfälle im System."
    />

    <div v-if="isError" class="error-msg">
      Fehler beim Laden der Incidents.
    </div>

    <EmptyState 
      v-else-if="!isPending && allLoadedIncidents.length === 0"
      icon="pi-inbox"
      title="Datenbank leer"
      description="Es wurden noch keine Sicherheitsvorfälle im System erfasst."
    >
      <Button label="STIX Datei hochladen" icon="pi pi-upload" @click="router.push('/upload')" />
    </EmptyState>

    <div v-else>
      <div class="card-grid">
        <IncidentCard 
          v-for="incident in allLoadedIncidents" 
          :key="incident.id" 
          :incident="incident" 
          :highlighted="allowedTypes.has(incident.type)"
          @click="goToDetail"
          @delete="handleDeleteIncident"
        />
        
        <template v-if="isPending">
          <div v-for="i in 6" :key="'skel-'+i" class="p-4 border-1 surface-border border-round surface-card">
            <div class="flex mb-3">
              <Skeleton width="10rem" class="mb-2" />
              <Skeleton width="4rem" class="ml-auto" />
            </div>
            <Skeleton width="100%" height="4rem" />
          </div>
        </template>
      </div>

      <div class="load-more-container" v-if="hasMoreToLoad && !isPending && allLoadedIncidents.length > 0">
        <Button label="Weitere laden" outlined icon="pi pi-refresh" @click="loadMore" :loading="isPending" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.error-msg {
  color: var(--red-500, #ef4444);
  padding: 1rem;
  background-color: rgba(239, 68, 68, 0.1);
  border-radius: 8px;
  margin-bottom: 1rem;
}

.load-more-container {
  display: flex;
  justify-content: center;
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px dashed var(--border-color);
}
</style>
