<script setup lang="ts">
import { ref, shallowRef, onMounted, onUnmounted, watch, computed } from 'vue'
import { useRouter } from 'vue-router'
import { Network } from 'vis-network'
import { DataSet } from 'vis-data'
import { useIncidentsQuery, useRelationshipsQuery } from '@/composables/queries/useIncidentQueries'
import type { Incident, Relationship } from '@/types'
import PageHeader from '@/components/PageHeader.vue'
import Skeleton from 'primevue/skeleton'
import Button from 'primevue/button'

const router = useRouter()
const networkContainer = ref<HTMLElement | null>(null)
const network = shallowRef<Network | null>(null)

// Fetch data
const page = ref(0)
const count = ref(1000) // Hohe Anzahl für den Graphen
const { data: incidents, isPending: loadingIncidents } = useIncidentsQuery(page, count)
const { data: relationships, isPending: loadingRels } = useRelationshipsQuery(page, count)

const isLoading = computed(() => loadingIncidents.value || loadingRels.value)
const selectedNodeId = ref<string | null>(null)

const selectedIncident = computed(() => {
  if (!selectedNodeId.value || !incidents.value) return null
  return incidents.value.find(i => i.id === selectedNodeId.value) || null
})

// Vis Network Datasets
const nodes = new DataSet<any>()
const edges = new DataSet<any>()

const getTypeColor = (type?: string) => {
  const t = type?.toLowerCase() || ''
  if (t.includes('malware') || t.includes('attack-pattern')) return { background: '#ef4444', border: '#b91c1c' } // Red
  if (t.includes('phishing') || t.includes('campaign')) return { background: '#f59e0b', border: '#b45309' } // Amber/Orange
  if (t.includes('identity') || t.includes('location')) return { background: '#3b82f6', border: '#1d4ed8' } // Blue
  return { background: '#64748b', border: '#475569' } // Gray
}

const buildGraph = () => {
  if (!incidents.value || !relationships.value || !networkContainer.value) return

  // Clear existing
  nodes.clear()
  edges.clear()

  // Add Nodes
  const newNodes = incidents.value.map(inc => ({
    id: inc.id,
    label: inc.name || 'Unbenannt',
    title: `Type: ${inc.type}\nID: ${inc.id}`, // Tooltip
    color: getTypeColor(inc.type),
    font: { color: '#ffffff' },
    shape: 'box',
    margin: 10,
    shadow: true,
  }))
  nodes.add(newNodes)

  // Add Edges
  // Wir ignorieren leere UUIDs (STIX Bug) für den Graphen, damit er nicht kaputt geht
  const validRels = relationships.value.filter(r => {
    const from = r.idFrom || r.from
    const to = r.idTo || r.to
    return from && to && from !== '00000000-0000-0000-0000-000000000000' && to !== '00000000-0000-0000-0000-000000000000'
  })

  const newEdges = validRels.map(rel => {
    const from = rel.idFrom || rel.from
    const to = rel.idTo || rel.to
    return {
      id: rel.id,
      from: from,
      to: to,
      arrows: 'to',
      color: { color: '#cbd5e1', highlight: '#94a3b8' },
      smooth: { type: 'continuous' }
    }
  })
  edges.add(newEdges)

  // Init Network
  const data = { nodes, edges }
  const options = {
    physics: {
      stabilization: true,
      barnesHut: {
        gravitationalConstant: -2000,
        springConstant: 0.04,
        springLength: 150
      }
    },
    interaction: {
      hover: true,
      tooltipDelay: 200,
    }
  }

  if (network.value) {
    network.value.destroy()
  }
  
  network.value = new Network(networkContainer.value, data, options)

  // Events
  network.value.on('click', (params) => {
    if (params.nodes.length > 0) {
      selectedNodeId.value = params.nodes[0]
    } else {
      selectedNodeId.value = null
    }
  })
}

watch([incidents, relationships], () => {
  if (!isLoading.value) {
    buildGraph()
  }
})

onMounted(() => {
  if (!isLoading.value && incidents.value && relationships.value) {
    buildGraph()
  }
})

onUnmounted(() => {
  if (network.value) {
    network.value.destroy()
  }
})

const fitGraph = () => {
  if (network.value) {
    network.value.fit({ animation: true })
  }
}
</script>

<template>
  <div class="page-container flex flex-column h-full">
    <PageHeader 
      title="Abhängigkeits-Graph" 
      description="Interaktive Visualisierung aller aktiven Vorfälle und deren Verknüpfungen."
    />

    <div class="flex-1 flex gap-4 h-full" style="min-height: 600px;">
      <!-- Graph Container -->
      <div class="surface-card border-round shadow-2 p-3 flex-1 flex flex-column relative overflow-hidden" style="height: 75vh;">
        <div v-if="isLoading" class="absolute inset-0 z-1 flex align-items-center justify-content-center surface-ground opacity-80">
          <div class="flex flex-column align-items-center gap-3">
            <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
            <span class="font-medium text-color-secondary">Lade Graph-Daten...</span>
          </div>
        </div>

        <!-- Toolbar -->
        <div class="absolute top-0 right-0 z-2 m-4 flex gap-2">
          <Button icon="pi pi-search-plus" severity="secondary" rounded text @click="network?.moveTo({ scale: network.getScale() * 1.5 })" />
          <Button icon="pi pi-search-minus" severity="secondary" rounded text @click="network?.moveTo({ scale: network.getScale() * 0.5 })" />
          <Button icon="pi pi-arrows-alt" severity="secondary" rounded text tooltip="Einpassen" @click="fitGraph" />
        </div>

        <div ref="networkContainer" class="w-full border-round" style="height: 100%; background-color: var(--surface-ground);"></div>
      </div>

      <!-- Side Panel for Selection -->
      <div v-if="selectedNodeId" class="surface-card border-round shadow-2 p-4 w-25rem flex flex-column fadein animation-duration-200">
        <div class="flex justify-content-between align-items-center mb-3">
          <h3 class="m-0 text-xl font-semibold">Auswahl Details</h3>
          <Button icon="pi pi-times" text rounded severity="secondary" @click="selectedNodeId = null" />
        </div>

        <div v-if="selectedIncident" class="flex flex-column gap-3">
          <div>
            <span class="text-500 text-sm block mb-1">Titel</span>
            <div class="font-medium text-lg">{{ selectedIncident.name || 'Unbenannt' }}</div>
          </div>
          <div>
            <span class="text-500 text-sm block mb-1">Typ</span>
            <span class="inline-block px-2 py-1 border-round text-sm font-medium surface-200">{{ selectedIncident.type }}</span>
          </div>
          <div>
            <span class="text-500 text-sm block mb-1">ID</span>
            <div class="text-xs font-mono text-500 bg-gray-100 p-2 border-round" style="word-break: break-all;">
              {{ selectedIncident.id }}
            </div>
          </div>
          <div class="flex-1"></div>
          <Button 
            label="Details ansehen" 
            icon="pi pi-arrow-right" 
            class="w-full mt-4" 
            @click="router.push(`/incidents/${selectedIncident.id}`)"
          />
        </div>
        <div v-else class="text-center text-500 py-4">
          Incident konnte nicht geladen werden (evtl. gelöscht).
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  height: calc(100vh - 2rem);
  padding-bottom: 0;
}
/* Focus outline im Canvas entfernen */
:deep(canvas:focus) {
  outline: none;
}
</style>
