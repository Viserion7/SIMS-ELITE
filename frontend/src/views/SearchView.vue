<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useIncidentsQuery, useDeleteIncidentMutation } from '@/composables/queries/useIncidentQueries'
import type { Incident } from '@/types'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
import IncidentCard from '@/components/IncidentCard.vue'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Skeleton from 'primevue/skeleton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'

const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const deleteMutation = useDeleteIncidentMutation()

const page = ref(0)
const count = ref(1000) // Fetch a large chunk for client-side search
const searchQuery = ref('')

const { data: rawIncidents, isPending, isError } = useIncidentsQuery(page, count)

const allLoadedIncidents = ref<Incident[]>([])

watch(rawIncidents, (newIncidents) => {
  if (newIncidents) {
    if (page.value === 0) {
      allLoadedIncidents.value = [...newIncidents]
    } else {
      const newItems = newIncidents.filter(n => !allLoadedIncidents.value.some(o => o.id === n.id))
      allLoadedIncidents.value.push(...newItems)
    }
  }
}, { immediate: true })

const filteredIncidents = computed(() => {
  if (!searchQuery.value.trim()) return []
  const query = searchQuery.value.toLowerCase()
  return allLoadedIncidents.value.filter(incident => 
    incident.name?.toLowerCase().includes(query) || 
    incident.desc?.toLowerCase().includes(query) ||
    incident.type?.toLowerCase().includes(query) ||
    incident.id.toLowerCase().includes(query)
  )
})

const goToDetail = (incident: Incident) => {
  router.push(`/incidents/${incident.id}`)
}

const handleDeleteIncident = (incident: Incident) => {
  confirm.require({
    message: `Möchtest du den Vorfall "${incident.name || incident.id}" wirklich als gesehen markieren? Er wird danach auch nicht mehr in der Suche gefunden.`,
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
      title="Suche" 
      description="Durchsuche alle aktiven Vorfälle im System. (Bereits als 'gesehen' markierte Vorfälle sind ausgeblendet)."
    />

    <div class="search-bar surface-card p-4 border-round mb-4 flex gap-3 align-items-center">
      <span class="p-input-icon-left w-full">
        <i class="pi pi-search" />
        <InputText 
          v-model="searchQuery" 
          placeholder="Suche nach Name, Typ, Beschreibung oder ID..." 
          class="w-full"
        />
      </span>
      <Button icon="pi pi-times" severity="secondary" text @click="searchQuery = ''" v-if="searchQuery" />
    </div>

    <div v-if="isError" class="error-msg">
      Fehler beim Laden der Incidents.
    </div>

    <div v-else-if="isPending" class="grid">
      <div class="col-12" v-for="i in 3" :key="i">
        <div class="surface-card p-4 border-round mb-3">
          <Skeleton width="60%" height="2rem" class="mb-2"></Skeleton>
          <Skeleton width="40%" class="mb-2"></Skeleton>
          <Skeleton width="20%"></Skeleton>
        </div>
      </div>
    </div>

    <EmptyState 
      v-else-if="!searchQuery.trim()"
      icon="pi-search"
      title="Suchbegriff eingeben"
      description="Tippe etwas in die Suchleiste, um aktive Incidents zu finden."
    />

    <EmptyState 
      v-else-if="filteredIncidents.length === 0"
      icon="pi-filter-slash"
      title="Keine Ergebnisse"
      description="Zu deinem Suchbegriff wurden keine aktiven Vorfälle gefunden."
    />

    <div v-else class="grid">
      <div class="col-12" v-for="incident in filteredIncidents" :key="incident.id">
        <IncidentCard 
          :incident="incident" 
          @click="goToDetail(incident)"
          @delete="handleDeleteIncident(incident)"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.search-bar {
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
}
</style>
