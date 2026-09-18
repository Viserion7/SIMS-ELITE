<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  useIncidentDetailQuery, 
  useIncidentRelationshipsQuery,
  useDeleteIncidentMutation 
} from '@/composables/queries/useIncidentQueries'
import { useEscalateMutation } from '@/composables/queries/useEscalationQueries'
import { useMyCategories } from '@/composables/useMyCategories'
import PageHeader from '@/components/PageHeader.vue'
import RelatedIncidentCard from '@/components/RelatedIncidentCard.vue'
import Button from 'primevue/button'
import Badge from 'primevue/badge'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import Textarea from 'primevue/textarea'
import Skeleton from 'primevue/skeleton'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()

const incidentId = computed(() => route.params.id as string)

const { data: incident, isPending: loadingIncident, isError: errorIncident } = useIncidentDetailQuery(incidentId)
const { data: relationships, isPending: loadingRels } = useIncidentRelationshipsQuery(incidentId)
const escalateMutation = useEscalateMutation()
const deleteMutation = useDeleteIncidentMutation()
const allowedTypes = useMyCategories()

const showEscalateModal = ref(false)
const escalateMessage = ref('')

const isMyIncident = computed(() => {
  if (!incident.value) return false
  return allowedTypes.value.has(incident.value.type)
})

const getTypeSeverity = (type?: string): 'danger' | 'warn' | 'info' | 'secondary' | 'success' => {
  if (!type) return 'secondary'
  const t = type.toLowerCase()
  if (t.includes('malware') || t.includes('attack-pattern') || t.includes('vulnerability')) return 'danger'
  if (t.includes('indicator') || t.includes('threat-actor') || t.includes('campaign')) return 'warn'
  if (t.includes('identity') || t.includes('location') || t.includes('grouping')) return 'info'
  return 'secondary'
}

// Soft delete (Als gesehen markieren)
const confirmMarkAsSeen = () => {
  if (!incident.value) return

  confirm.require({
    message: `Möchtest du den Vorfall "${incident.value.name || incident.value.id}" wirklich als gesehen markieren? Er wird danach aus den aktiven Vorfalllisten ausgeblendet.`,
    header: 'Vorfall als gesehen markieren',
    icon: 'pi pi-check-circle',
    acceptLabel: 'Als gesehen markieren',
    rejectLabel: 'Abbrechen',
    acceptClass: 'p-button-success',
    accept: async () => {
      try {
        await deleteMutation.mutateAsync(incidentId.value)
        toast.add({
          severity: 'success',
          summary: 'Als gesehen markiert',
          detail: 'Der Vorfall wurde erfolgreich als gesehen markiert und archiviert.',
          life: 3000,
        })
        router.push('/incidents/my')
      } catch (err: any) {
        toast.add({
          severity: 'error',
          summary: 'Fehler',
          detail: err?.message || 'Der Vorfall konnte nicht als gesehen markiert werden.',
          life: 4000,
        })
      }
    },
  })
}

const handleEscalate = async () => {
  if (!escalateMessage.value.trim()) {
    toast.add({ severity: 'warn', summary: 'Eingabe fehlt', detail: 'Bitte gib eine kurze Eskalationsnachricht ein.', life: 3000 })
    return
  }

  try {
    await escalateMutation.mutateAsync({
      incidentId: incidentId.value,
      message: escalateMessage.value.trim(),
    })
    toast.add({ 
      severity: 'success', 
      summary: 'Erfolg', 
      detail: `Eskalation für Vorfall ${incident.value?.name || incidentId.value} wurde erfolgreich versendet.`, 
      life: 3500 
    })
    showEscalateModal.value = false
    escalateMessage.value = ''
  } catch (error: any) {
    toast.add({ 
      severity: 'error', 
      summary: 'Eskalation fehlgeschlagen', 
      detail: error?.message || 'Die Eskalations-E-Mail konnte nicht gesendet werden.', 
      life: 4000 
    })
  }
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    toast.add({ severity: 'info', summary: 'Kopiert', detail: 'ID in Zwischenablage kopiert.', life: 2000 })
  } catch (err) {
    console.error('Failed to copy: ', err)
  }
}

const formatDate = (dateString: string) => {
  if (!dateString) return 'Unbekannt'
  return new Date(dateString).toLocaleDateString('de-DE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

const shortId = (id?: string) => {
  if (!id) return ''
  return id.length > 13 ? id.substring(0, 13) + '...' : id
}
</script>

<template>
  <div class="page-container incident-detail-page">
    <!-- Top Navigation -->
    <div class="navigation-bar">
      <Button 
        icon="pi pi-arrow-left" 
        label="Zurück zur Übersicht" 
        text 
        class="back-btn" 
        @click="router.back()" 
      />
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loadingIncident" class="loading-state">
      <Skeleton width="50%" height="2.5rem" class="skeleton-item" />
      <Skeleton width="100%" height="16rem" class="skeleton-item" />
      <Skeleton width="100%" height="8rem" class="skeleton-item" />
    </div>

    <!-- Error State -->
    <div v-else-if="errorIncident || !incident" class="error-card">
      <i class="pi pi-exclamation-circle error-icon"></i>
      <div class="error-content">
        <h4 class="error-title">Vorfall nicht gefunden</h4>
        <p class="error-desc">
          Der Vorfall konnte nicht geladen werden oder wurde bereits als gesehen archiviert.
        </p>
      </div>
      <Button label="Zu meinen Incidents" class="error-action-btn" @click="router.push('/incidents/my')" />
    </div>

    <!-- Main Content -->
    <template v-else>
      <div class="header-section">
        <PageHeader :title="incident.name || 'Unbenannter Vorfall'">
          <div class="header-badges">
            <Badge 
              :value="incident.type" 
              size="large" 
              :severity="getTypeSeverity(incident.type)" 
              class="uppercase-badge" 
            />
            <Badge 
              v-if="isMyIncident" 
              value="In deiner Zuständigkeit" 
              severity="success" 
              size="large" 
            />
          </div>
        </PageHeader>
      </div>

      <!-- 1. Detail Card: Stammdaten des Vorfalls -->
      <div class="detail-card">
        <div class="card-section-header">
          <div class="section-indicator">
            <i class="pi pi-info-circle"></i>
            <span>Stammdaten & Information</span>
          </div>
        </div>

        <div class="grid-info">
          <div class="info-group">
            <span class="info-label">Incident ID</span>
            <div class="clickable-id" @click="copyToClipboard(incident.id)" title="Klicken zum Kopieren">
              <span class="id-text">{{ incident.id }}</span>
              <i class="pi pi-copy copy-icon"></i>
            </div>
          </div>

          <div class="info-group">
            <span class="info-label">Erstellt am</span>
            <div class="info-value date-value">
              <i class="pi pi-calendar"></i>
              <span>{{ formatDate(incident.created_at) }}</span>
            </div>
          </div>

          <div class="info-group">
            <span class="info-label">Quellformat</span>
            <div class="info-value">
              <Badge :value="incident.source_format" severity="secondary" />
            </div>
          </div>

          <div class="info-group">
            <span class="info-label">Status</span>
            <div class="info-value status-active">
              <i class="pi pi-check-circle"></i>
              <span>Aktiv im System</span>
            </div>
          </div>
        </div>

        <!-- Description Box -->
        <div class="desc-box">
          <div class="desc-header">
            <i class="pi pi-align-left"></i>
            <span>Beschreibung</span>
          </div>
          <div class="desc-content">
            <p class="desc-text">{{ incident.desc || 'Keine Beschreibung vorhanden.' }}</p>
          </div>
        </div>
      </div>

      <!-- 2. Action Card: Vorgangs-Aktionen (Als gesehen markieren / Eskalieren) -->
      <div class="action-card">
        <div class="action-header">
          <div class="action-title-group">
            <div class="action-title-row">
              <div class="action-icon-box">
                <i class="pi pi-bolt"></i>
              </div>
              <div>
                <h3 class="action-title">Aktionen für diesen Vorfall</h3>
                <p class="action-subtitle">
                  Vorfall: <strong>{{ incident.name || incident.id }}</strong>
                </p>
              </div>
            </div>
          </div>
          
          <div class="action-buttons">
            <!-- 1. Als gesehen markieren (Soft Delete) -->
            <Button 
              label="Als gesehen markieren" 
              icon="pi pi-check-circle" 
              severity="success" 
              outlined
              class="action-btn action-btn-seen"
              :loading="deleteMutation.isPending.value"
              @click="confirmMarkAsSeen" 
            />

            <!-- 2. Vorfall eskalieren -->
            <Button 
              label="Vorfall eskalieren" 
              icon="pi pi-exclamation-triangle" 
              severity="danger" 
              class="action-btn action-btn-escalate"
              @click="showEscalateModal = true" 
            />
          </div>
        </div>

        <div class="action-info-banner">
          <div class="banner-icon-box">
            <i class="pi pi-info-circle"></i>
          </div>
          <div class="banner-text">
            <strong>Als gesehen markieren</strong> archiviert diesen Vorfall aus deinen aktiven Listen. 
            <strong>Eskalieren</strong> sendet eine dringende Benachrichtigung mit dieser Vorfall-ID per E-Mail an alle zuständigen Sicherheitsbeauftragten.
          </div>
        </div>
      </div>

      <!-- 3. Relationen: Nur anzeigen, wenn tatsächlich Relationen vorhanden sind! -->
      <div v-if="relationships && relationships.length > 0" class="relationships-section">
        <div class="relationships-card">
          <div class="card-section-header">
            <div class="section-indicator">
              <i class="pi pi-share-alt"></i>
              <span>Verknüpfte Objekte ({{ relationships.length }})</span>
            </div>
          </div>
          
          <div v-if="relationships.length > 0" class="relations-grid">
            <div v-for="rel in relationships" :key="rel.id" class="relation-item">
              <!-- Da wir den STIX-Bug noch haben, kann ID leer sein oder es steht was in from/to.
                   Wir versuchen die ID zu erraten, je nach dem was gefüllt ist. -->
              <div class="relation-badge">
                <Badge value="Verknüpft" severity="info" class="mb-2" />
              </div>
              <RelatedIncidentCard 
                :incidentId="(rel.idTo === incident.id ? (rel.idFrom || rel.from) : (rel.idTo || rel.to)) as string" 
              />
            </div>
          </div>
          <div v-else class="text-500 font-italic py-4 text-center surface-100 border-round">
            Keine Verknüpfungen zu anderen Vorfällen vorhanden.
          </div>
        </div>
      </div>
    </template>

    <!-- Dialog: DIESEN Vorfall eskalieren -->
    <Dialog 
      v-model:visible="showEscalateModal" 
      modal 
      header="Vorfall eskalieren" 
      :style="{ width: '38rem', maxWidth: '95vw' }"
    >
      <div class="escalate-dialog-content">
        <!-- Zusammenfassung des zu eskalierenden Vorfalls -->
        <div v-if="incident" class="incident-preview-box">
          <div class="preview-header">
            <span class="preview-tag">Zu eskalierender Vorfall</span>
            <Badge :value="incident.type" :severity="getTypeSeverity(incident.type)" size="small" />
          </div>
          <div class="preview-name">
            {{ incident.name || 'Unbenannter Vorfall' }}
          </div>
          <div class="preview-id">
            ID: {{ incident.id }}
          </div>
        </div>

        <p class="dialog-explanation">
          Gib eine kurze Begründung an. Alle zuständigen Personen (im Identity-Service als 
          <em>"Benachrichtigen"</em> markiert) erhalten umgehend eine E-Mail mit den Details dieses Vorfalls.
        </p>
        
        <div class="field-container">
          <label for="escalateMsg" class="dialog-field-label">
            Eskalationsnachricht <span class="required-star">*</span>
          </label>
          <Textarea 
            id="escalateMsg" 
            v-model="escalateMessage" 
            autoResize 
            rows="4" 
            class="dialog-textarea"
            placeholder="z.B. Kritischer Vorfall erfordert sofortige Analyse und Isolierung des betroffenen Hosts..." 
          />
        </div>
      </div>

      <template #footer>
        <Button 
          label="Abbrechen" 
          icon="pi pi-times" 
          text 
          severity="secondary" 
          @click="showEscalateModal = false" 
        />
        <Button 
          label="Eskalation absenden" 
          icon="pi pi-send" 
          severity="danger" 
          :loading="escalateMutation.isPending.value" 
          @click="handleEscalate" 
        />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.incident-detail-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.navigation-bar {
  margin-bottom: 1.5rem;
}

.back-btn {
  font-weight: 500;
  padding: 0;
  transition: transform 0.2s ease;
}

.back-btn:hover {
  transform: translateX(-3px);
}

.loading-state {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.skeleton-item {
  border-radius: 12px;
}

.error-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  background-color: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.25);
  border-radius: 14px;
  padding: 1.75rem;
}

.error-icon {
  font-size: 2rem;
  color: #ef4444;
  flex-shrink: 0;
}

.error-content {
  flex: 1;
}

.error-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-color);
}

.error-desc {
  margin: 0.35rem 0 0 0;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.header-section {
  margin-bottom: 2rem;
}

.header-badges {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.uppercase-badge {
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* 1. Detail Card */
.detail-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  padding: 1.75rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
  margin-bottom: 2.25rem; /* Generous separation */
}

.card-section-header {
  margin-bottom: 1.25rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--border-color);
}

.section-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.section-indicator i {
  color: var(--primary-color);
  font-size: 1rem;
}

.grid-info {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.info-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.info-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  font-size: 0.95rem;
  color: var(--text-color);
  font-weight: 500;
  display: flex;
  align-items: center;
}

.date-value {
  gap: 0.4rem;
}

.date-value i {
  color: var(--text-color-secondary);
}

.status-active {
  color: #22c55e;
  font-weight: 600;
  gap: 0.4rem;
}

.clickable-id {
  font-family: monospace;
  font-size: 0.85rem;
  cursor: pointer;
  background: var(--bg-color);
  border: 1px solid var(--border-color);
  padding: 0.4rem 0.7rem;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  max-width: 100%;
  transition: all 0.2s ease;
}

.clickable-id:hover {
  border-color: var(--primary-color);
  color: var(--primary-color);
  background: rgba(59, 130, 246, 0.08);
}

.id-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.copy-icon {
  font-size: 0.8rem;
  flex-shrink: 0;
  opacity: 0.7;
}

.desc-box {
  background: var(--bg-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 1.25rem;
}

.desc-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.75rem;
}

.desc-header i {
  color: var(--primary-color);
}

.desc-text {
  margin: 0;
  line-height: 1.65;
  color: var(--text-color);
  white-space: pre-wrap;
  font-size: 0.925rem;
}

/* 2. Action Card */
.action-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  padding: 1.75rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
  margin-bottom: 2.25rem; /* Generous separation */
  position: relative;
  overflow: hidden;
}

/* Subtle accent stripe */
.action-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, #22c55e, #3b82f6, #ef4444);
}

.action-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1.5rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.action-title-row {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.action-icon-box {
  width: 42px;
  height: 42px;
  border-radius: 10px;
  background: rgba(59, 130, 246, 0.12);
  color: var(--primary-color);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  flex-shrink: 0;
}

.action-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--text-color);
}

.action-subtitle {
  margin: 0.2rem 0 0 0;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.action-buttons {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.action-btn {
  font-weight: 600;
  border-radius: 10px;
  padding: 0.65rem 1.25rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.action-info-banner {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background-color: var(--bg-color);
  border-radius: 10px;
  border: 1px solid var(--border-color);
}

.banner-icon-box {
  color: var(--primary-color);
  font-size: 1.2rem;
  flex-shrink: 0;
  margin-top: 0.1rem;
}

.banner-text {
  font-size: 0.825rem;
  line-height: 1.55;
  color: var(--text-color-secondary);
}

.banner-text strong {
  color: var(--text-color);
}

/* 3. Relationships */
.relationships-section {
  margin-bottom: 2.25rem;
}

.relationships-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  padding: 1.75rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.table-container {
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid var(--border-color);
}

.mono-chip {
  background: var(--bg-color);
  padding: 0.25rem 0.6rem;
  border-radius: 6px;
  border: 1px solid var(--border-color);
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-family: monospace;
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.2s;
}

.mono-chip:hover {
  border-color: var(--primary-color);
  color: var(--primary-color);
}

.chip-copy-icon {
  font-size: 0.75rem;
  opacity: 0.7;
}

.relation-arrow {
  display: inline-flex;
  justify-content: center;
  align-items: center;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: var(--bg-color);
  border: 1px solid var(--border-color);
  color: var(--text-color-secondary);
}

/* Dialog Styling */
.incident-preview-box {
  background-color: var(--bg-color);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  padding: 1rem 1.25rem;
  margin-bottom: 1rem;
}

.preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.35rem;
}

.preview-tag {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.preview-name {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--text-color);
  margin-bottom: 0.25rem;
}

.preview-id {
  font-family: monospace;
  font-size: 0.775rem;
  color: var(--text-color-secondary);
}

.dialog-explanation {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
  line-height: 1.5;
  margin-bottom: 1.25rem;
}

.field-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.dialog-field-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-color);
}

.required-star {
  color: #ef4444;
}

.dialog-textarea {
  width: 100%;
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .incident-detail-page {
    padding: 1rem;
  }
  .action-header {
    flex-direction: column;
    align-items: stretch;
  }
  .action-buttons {
    flex-direction: column;
  }
}
</style>
