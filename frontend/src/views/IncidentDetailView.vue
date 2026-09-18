<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useIncidentDetailQuery, useIncidentRelationshipsQuery } from '@/composables/queries/useIncidentQueries'
import { useEscalateMutation } from '@/composables/queries/useEscalationQueries'
import { useMyCategories } from '@/composables/useMyCategories'
import PageHeader from '@/components/PageHeader.vue'
import Button from 'primevue/button'
import Badge from 'primevue/badge'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import Textarea from 'primevue/textarea'
import Skeleton from 'primevue/skeleton'
import { useToast } from 'primevue/usetoast'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const incidentId = route.params.id as string

const { data: incident, isPending: loadingIncident, isError: errorIncident } = useIncidentDetailQuery(incidentId)
const { data: relationships, isPending: loadingRels } = useIncidentRelationshipsQuery(incidentId)
const escalateMutation = useEscalateMutation()
const allowedTypes = useMyCategories()

const showEscalateModal = ref(false)
const escalateMessage = ref('')

const isMyIncident = computed(() => {
  if (!incident.value) return false
  return allowedTypes.value.has(incident.value.type)
})

const handleEscalate = async () => {
  if (!escalateMessage.value.trim()) return

  try {
    await escalateMutation.mutateAsync({
      incidentId: incidentId,
      message: escalateMessage.value,
    })
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Eskalation wurde erfolgreich versendet.', life: 3000 })
    showEscalateModal.value = false
    escalateMessage.value = ''
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Eskalation fehlgeschlagen.', life: 3000 })
  }
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    toast.add({ severity: 'info', summary: 'Kopiert', detail: 'ID wurde in die Zwischenablage kopiert.', life: 2000 })
  } catch (err) {
    console.error('Failed to copy: ', err)
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('de-DE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

// Function to truncate UUIDs for display
const shortId = (id: string) => {
  if (!id) return ''
  return id.length > 13 ? id.substring(0, 13) + '...' : id
}
</script>

<template>
  <div class="page-container">
    <Button 
      icon="pi pi-arrow-left" 
      label="Zurück zur Übersicht" 
      text 
      class="mb-4 p-0" 
      @click="router.back()" 
    />

    <div v-if="loadingIncident">
      <Skeleton width="50%" height="3rem" class="mb-4" />
      <Skeleton width="100%" height="20rem" />
    </div>

    <div v-else-if="errorIncident || !incident" class="error-msg">
      Incident konnte nicht gefunden oder geladen werden.
    </div>

    <template v-else>
      <PageHeader 
        :title="incident.name || 'Unbenannter Vorfall'" 
      >
        <Badge :value="incident.type" size="large" severity="info" class="mr-2" />
        <Badge v-if="isMyIncident" value="In deiner Zuständigkeit" severity="success" size="large" />
      </PageHeader>

      <div class="detail-card mb-4">
        <div class="grid-info">
          <div class="info-group">
            <span class="info-label">Incident ID</span>
            <span class="info-value clickable-id" @click="copyToClipboard(incident.id)" title="Klicken zum Kopieren">
              {{ incident.id }} <i class="pi pi-copy text-sm ml-1"></i>
            </span>
          </div>
          <div class="info-group">
            <span class="info-label">Erstellt am</span>
            <span class="info-value">{{ formatDate(incident.created_at) }}</span>
          </div>
          <div class="info-group">
            <span class="info-label">Quellformat</span>
            <span class="info-value"><Badge :value="incident.source_format" severity="secondary" /></span>
          </div>
        </div>

        <div class="desc-box mt-4">
          <span class="info-label block mb-2">Beschreibung</span>
          <p class="desc-text">{{ incident.desc || 'Keine Beschreibung vorhanden.' }}</p>
        </div>
      </div>

      <div class="relationships-section mb-5">
        <h3 class="section-title">Verknüpfte Objekte</h3>
        
        <div class="card">
          <DataTable :value="relationships" :loading="loadingRels" stripedRows 
                     emptyMessage="Keine Verknüpfungen vorhanden.">
            <Column field="from" header="Von (ID)">
              <template #body="slotProps">
                <span :title="slotProps.data.from" class="text-sm font-mono cursor-pointer hover:text-primary" @click="copyToClipboard(slotProps.data.from)">
                  {{ shortId(slotProps.data.from) }}
                </span>
              </template>
            </Column>
            <Column header="Richtung">
              <template #body>
                <i class="pi pi-arrow-right text-color-secondary"></i>
              </template>
            </Column>
            <Column field="to" header="Nach (ID)">
              <template #body="slotProps">
                <span :title="slotProps.data.to" class="text-sm font-mono cursor-pointer hover:text-primary" @click="copyToClipboard(slotProps.data.to)">
                  {{ shortId(slotProps.data.to) }}
                </span>
              </template>
            </Column>
          </DataTable>
        </div>
      </div>

      <div class="action-bar">
        <div class="action-text">
          <p class="m-0 text-color-secondary text-sm">
            Wird eine Eskalation ausgelöst, werden alle hinterlegten Administratoren/Verantwortlichen sofort benachrichtigt.
          </p>
        </div>
        <Button 
          label="Eskalieren" 
          icon="pi pi-exclamation-triangle" 
          severity="danger" 
          size="large"
          @click="showEscalateModal = true" 
        />
      </div>
    </template>

    <Dialog v-model:visible="showEscalateModal" modal header="Vorfall eskalieren" :style="{ width: '35rem' }">
      <p class="mb-4">
        Bitte beschreibe kurz, warum dieser Vorfall eskaliert wird. Alle zuständigen Personen (die den "Benachrichtigen"-Status haben) erhalten umgehend eine E-Mail.
      </p>
      
      <div class="field flex flex-column gap-2">
        <label for="escalateMsg" class="font-bold">Eskalationsnachricht</label>
        <Textarea 
          id="escalateMsg" 
          v-model="escalateMessage" 
          autoResize 
          rows="5" 
          class="w-full"
          placeholder="Dringende Prüfung erforderlich, da..." 
        />
      </div>

      <template #footer>
        <Button label="Abbrechen" icon="pi pi-times" text severity="secondary" @click="showEscalateModal = false" />
        <Button label="Eskalation auslösen" icon="pi pi-check" severity="danger" @click="handleEscalate" :loading="escalateMutation.isPending.value" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.error-msg {
  color: var(--red-500, #ef4444);
  padding: 1rem;
  background-color: rgba(239, 68, 68, 0.1);
  border-radius: 8px;
}

.detail-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 2rem;
}

.grid-info {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 2rem;
}

.info-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  font-size: 1rem;
  color: var(--text-color);
  font-weight: 500;
}

.clickable-id {
  font-family: monospace;
  cursor: pointer;
  background: var(--bg-color);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  display: inline-flex;
  align-items: center;
  width: max-content;
}

.clickable-id:hover {
  background: var(--surface-hover);
  color: var(--primary-color);
}

.desc-box {
  padding-top: 1.5rem;
  border-top: 1px dashed var(--border-color);
}

.desc-text {
  margin: 0;
  line-height: 1.6;
  color: var(--text-color);
  white-space: pre-wrap;
}

.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0 0 1rem 0;
}

.action-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  gap: 2rem;
}

.action-text {
  flex: 1;
}

@media (max-width: 768px) {
  .action-bar {
    flex-direction: column;
    align-items: stretch;
    text-align: center;
    gap: 1rem;
  }
}
</style>
