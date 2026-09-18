<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useStixUploadMutation, validateStixBundle } from '@/composables/queries/useStixQueries'
import PageHeader from '@/components/PageHeader.vue'
import AuthLayout from '@/layouts/AuthLayout.vue'
import FileUpload, { type FileUploadUploaderEvent } from 'primevue/fileupload'
import Message from 'primevue/message'
import Button from 'primevue/button'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import type { StixIngestResponse } from '@/types/stixResponse'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()
const uploadMutation = useStixUploadMutation()

const errorMsg = ref<string | null>(null)
const successResult = ref<StixIngestResponse | null>(null)

// If not authenticated, we wrap the content in AuthLayout manually here
// If authenticated, AppLayout is provided by router
const isAuth = computed(() => authStore.isAuthenticated)

const customUpload = async (event: FileUploadUploaderEvent) => {
  errorMsg.value = null
  successResult.value = null
  
  const file = Array.isArray(event.files) ? event.files[0] : event.files
  if (!file) return

  // Client-side file size check (50MB)
  if (file.size > 50 * 1024 * 1024) {
    errorMsg.value = 'Die Datei ist zu groß (max. 50 MB).'
    return
  }

  const reader = new FileReader()
  reader.onload = async (e) => {
    try {
      const text = e.target?.result as string
      const json = JSON.parse(text)

      // Validate
      const validation = validateStixBundle(json)
      if (!validation.valid) {
        errorMsg.value = validation.error || 'Ungültiges STIX Bundle.'
        return
      }

      // Upload
      const res = await uploadMutation.mutateAsync(json)
      successResult.value = res
      toast.add({ severity: 'success', summary: 'Erfolg', detail: 'STIX Bundle erfolgreich verarbeitet.', life: 3000 })
      
    } catch (err) {
      if (err instanceof SyntaxError) {
        errorMsg.value = 'Die Datei enthält kein gültiges JSON.'
      } else {
        errorMsg.value = 'Fehler beim Hochladen des Bundles.'
      }
    }
  }
  
  reader.onerror = () => {
    errorMsg.value = 'Fehler beim Lesen der Datei.'
  }

  reader.readAsText(file)
}

const resetUpload = () => {
  successResult.value = null
  errorMsg.value = null
}
</script>

<template>
  <div class="upload-view-container">
    
    <div class="guest-header">
      <span class="logo-text">SIMS-ELITE</span>
      <RouterLink v-if="!isAuth" to="/login" class="login-link">Mitarbeiter Login</RouterLink>
      <RouterLink v-else to="/incidents" class="login-link">&larr; Zurück zum Dashboard</RouterLink>
    </div>

    <div class="upload-card">
      <PageHeader 
        title="STIX Bundle hochladen" 
        description="Laden Sie ein STIX 2.1 JSON Bundle hoch, um Incidents und Relationen zu importieren."
      />

      <Message v-if="errorMsg" severity="error" :closable="false" class="mb-4">
        {{ errorMsg }}
      </Message>

      <div v-if="successResult" class="success-state">
        <div class="success-icon-wrap">
          <i class="pi pi-check-circle success-icon"></i>
        </div>
        <h3>Bundle erfolgreich verarbeitet</h3>
        
        <div class="stats-grid">
          <div class="stat-box">
            <span class="stat-val">{{ successResult.incidentsCreated ?? successResult.IncidentsCreated ?? 0 }}</span>
            <span class="stat-label">Incidents (Neu)</span>
          </div>
          <div class="stat-box">
            <span class="stat-val">{{ successResult.relationshipsCreated ?? successResult.RelationshipsCreated ?? 0 }}</span>
            <span class="stat-label">Relationen (Neu)</span>
          </div>
          <div class="stat-box">
            <span class="stat-val">{{ (successResult.incidentsDuplicate ?? successResult.IncidentsDuplicate ?? 0) + (successResult.relationshipsDuplicate ?? successResult.RelationshipsDuplicate ?? 0) }}</span>
            <span class="stat-label">Duplikate (Ignoriert)</span>
          </div>
        </div>

        <Button label="Weiteres Bundle hochladen" outlined icon="pi pi-upload" @click="resetUpload" class="mt-4" />
        <Button v-if="isAuth" label="Zu den Incidents" icon="pi pi-arrow-right" class="mt-2 ml-2" @click="router.push('/incidents')" />
      </div>

      <div v-else class="upload-area">
        <FileUpload 
          mode="advanced" 
          name="stixFile" 
          accept="application/json" 
          :maxFileSize="52428800" 
          customUpload 
          @uploader="customUpload"
          :auto="true"
          chooseLabel="JSON Datei auswählen"
          class="stix-uploader"
        >
          <template #empty>
            <div class="drop-zone-content">
              <i class="pi pi-cloud-upload drop-icon"></i>
              <p>Ziehen Sie eine .json Datei hierher</p>
            </div>
          </template>
        </FileUpload>
      </div>
    </div>
  </div>
</template>

<style scoped>
.upload-view-container {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.guest-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.logo-text {
  font-weight: 800;
  color: var(--primary-color);
  font-size: 1.5rem;
  letter-spacing: -0.5px;
}

.login-link {
  color: var(--text-color);
  text-decoration: none;
  font-weight: 600;
  font-size: 0.875rem;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: all 0.2s ease;
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
}

.login-link:hover {
  color: var(--primary-color);
  border-color: var(--primary-color);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
}

.upload-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  padding: 2.5rem;
  box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.1);
  width: 100%;
  position: relative;
  overflow: hidden;
}

.upload-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, var(--primary-color), #8b5cf6);
}



.upload-area {
  margin-top: 2rem;
}

:deep(.stix-uploader) {
  border: 2px dashed var(--border-color);
  border-radius: 12px;
  background-color: rgba(var(--surface-color-rgb), 0.5);
  transition: all 0.3s ease;
}

:deep(.stix-uploader:hover) {
  border-color: var(--primary-color);
  background-color: rgba(59, 130, 246, 0.03);
}

:deep(.p-fileupload-header) {
  display: flex;
  justify-content: center;
  border: none;
  background: transparent;
  padding: 1.5rem 1.5rem 0 1.5rem;
}

:deep(.p-fileupload-content) {
  border: none;
  background: transparent;
}

.drop-zone-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 3rem 1rem;
  color: var(--text-color-secondary);
  transition: all 0.2s ease;
}

.drop-zone-content:hover {
  transform: scale(1.02);
}

.drop-icon {
  font-size: 3.5rem;
  margin-bottom: 1rem;
  color: var(--primary-color);
  opacity: 0.8;
  filter: drop-shadow(0 4px 6px rgba(59, 130, 246, 0.2));
}

.success-state {
  text-align: center;
  padding: 3rem 0;
  animation: fade-in 0.5s ease-out;
}

@keyframes fade-in {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.success-icon-wrap {
  display: inline-flex;
  justify-content: center;
  align-items: center;
  width: 90px;
  height: 90px;
  background-color: rgba(34, 197, 94, 0.1);
  border-radius: 50%;
  margin-bottom: 1.5rem;
}

.success-icon {
  font-size: 4rem;
  color: var(--green-500, #22c55e);
}

.success-state h3 {
  font-size: 1.5rem;
  margin: 0 0 0.5rem 0;
  font-weight: 700;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  margin: 2.5rem 0;
}

.stat-box {
  background-color: var(--bg-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  transition: transform 0.2s;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}

.stat-box:hover {
  transform: translateY(-3px);
  border-color: var(--primary-color);
}

.stat-val {
  font-size: 2.5rem;
  font-weight: 800;
  color: var(--primary-color);
  line-height: 1;
  background: linear-gradient(135deg, var(--primary-color), #8b5cf6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.stat-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  margin-top: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}
</style>
