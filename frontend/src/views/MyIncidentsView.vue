<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useIncidentsQuery } from '@/composables/queries/useIncidentQueries'
import { useCurrentUserQuery } from '@/composables/queries/useAuthQueries'
import { useMyCategories } from '@/composables/useMyCategories'
import type { Incident } from '@/types'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
import IncidentCard from '@/components/IncidentCard.vue'
import Button from 'primevue/button'
import Skeleton from 'primevue/skeleton'

const router = useRouter()
const { data: userDetails } = useCurrentUserQuery()
const allowedTypes = useMyCategories()

const page = ref(0)
const count = ref(20) // Load chunks of 20

const { data: rawIncidents, isPending, isError } = useIncidentsQuery(page, count)

// Wir sammeln alle geladenen Incidents in einer Liste
const allLoadedIncidents = ref<Incident[]>([])
const hasMoreToLoad = ref(true)

// Wir hängen neue Incidents an, sobald sie geladen sind
// Da useIncidentsQuery reaktiv ist, müssen wir auf Änderungen reagieren
import { watch } from 'vue'
watch(rawIncidents, (newIncidents) => {
  if (newIncidents) {
    if (newIncidents.length < count.value) {
      hasMoreToLoad.value = false // Wenn das Backend weniger liefert, als angefragt, sind wir am Ende
    }
    
    // Vermeide Duplikate beim Nachladen
    const newItems = newIncidents.filter(n => !allLoadedIncidents.value.some(o => o.id === n.id))
    allLoadedIncidents.value.push(...newItems)
  }
}, { immediate: true })

const filteredIncidents = computed(() => {
  if (!allowedTypes.value.size) return []
  return allLoadedIncidents.value.filter(incident => allowedTypes.value.has(incident.type))
})

const loadMore = () => {
  if (!hasMoreToLoad.value) return
  page.value += 1
}

const goToDetail = (incident: Incident) => {
  router.push(`/incidents/${incident.id}`)
}
</script>

<template>
  <div class="page-container">
    <PageHeader 
      title="Meine Incidents" 
      description="Sicherheitsvorfälle in deinem Zuständigkeitsbereich (basierend auf deinen Eskalationsstufen)."
    />

    <div v-if="isError" class="error-msg">
      Fehler beim Laden der Incidents.
    </div>

    <!-- Keine Levels zugewiesen -->
    <EmptyState 
      v-else-if="!isPending && allowedTypes.size === 0"
      icon="pi-lock"
      title="Keine Zuständigkeit"
      description="Dir wurden noch keine Eskalationsstufen zugewiesen. Bitte wende dich an einen Administrator."
    />

    <!-- Gefilterte Liste ist leer -->
    <EmptyState 
      v-else-if="!isPending && filteredIncidents.length === 0 && !hasMoreToLoad"
      icon="pi-check-circle"
      title="Alles ruhig"
      description="Aktuell gibt es keine Vorfälle in deinem Zuständigkeitsbereich."
    >
      <Button label="Alle Incidents ansehen" outlined @click="router.push('/incidents/all')" />
    </EmptyState>

    <div v-else>
      <div class="card-grid">
        <IncidentCard 
          v-for="incident in filteredIncidents" 
          :key="incident.id" 
          :incident="incident" 
          highlighted
          @click="goToDetail"
        />
        
        <!-- Skeletons while loading -->
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
        <p class="load-hint">Es könnten sich in älteren Einträgen noch relevante Vorfälle befinden.</p>
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
  flex-direction: column;
  align-items: center;
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px dashed var(--border-color);
}

.load-hint {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  margin-top: 0.75rem;
}
</style>
