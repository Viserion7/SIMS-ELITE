<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useIncidentDetailQuery, useDeleteIncidentMutation } from '@/composables/queries/useIncidentQueries'
import Button from 'primevue/button'
import Skeleton from 'primevue/skeleton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'

const props = defineProps<{
  incidentId: string
}>()

const router = useRouter()
const confirm = useConfirm()
const toast = useToast()

const { data: incident, isPending, isError } = useIncidentDetailQuery(props.incidentId)
const deleteMutation = useDeleteIncidentMutation()

const typeColor = computed(() => {
  switch (incident.value?.type?.toLowerCase()) {
    case 'phishing': return 'var(--blue-500)'
    case 'malware': return 'var(--red-500)'
    case 'ddos': return 'var(--orange-500)'
    default: return 'var(--gray-500)'
  }
})

const goToDetail = () => {
  if (incident.value && !incident.value.is_deleted) {
    router.push(`/incidents/${incident.value.id}`)
  }
}

const handleDelete = (event: Event) => {
  event.stopPropagation()
  confirm.require({
    message: `Möchtest du den verknüpften Vorfall "${incident.value?.name || incident.value?.id}" wirklich als gesehen markieren?`,
    header: 'Verknüpfung als gesehen markieren',
    icon: 'pi pi-check-circle',
    acceptLabel: 'Als gesehen markieren',
    rejectLabel: 'Abbrechen',
    acceptClass: 'p-button-success',
    accept: async () => {
      try {
        await deleteMutation.mutateAsync(props.incidentId)
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
  <div class="related-card-wrapper">
    <!-- Ladezustand -->
    <div v-if="isPending" class="surface-card p-3 border-round shadow-1 flex align-items-center gap-3">
      <Skeleton shape="circle" size="2rem" />
      <div class="flex-1">
        <Skeleton width="60%" class="mb-2" />
        <Skeleton width="30%" />
      </div>
    </div>

    <!-- Fehler / Nicht gefunden (z.B. soft-deleted) -->
    <div v-else-if="isError || !incident" class="surface-card p-3 border-round shadow-1 flex align-items-center gap-3 opacity-60">
      <div class="flex align-items-center justify-content-center border-circle surface-200" style="width: 2rem; height: 2rem;">
        <i class="pi pi-ban text-500"></i>
      </div>
      <div class="flex-1">
        <div class="text-600 font-medium line-height-1">Gelöscht oder nicht gefunden</div>
        <div class="text-500 text-sm mt-1">ID: {{ incidentId }}</div>
      </div>
    </div>

    <!-- Gefunden -->
    <div 
      v-else 
      class="surface-card p-3 border-round shadow-2 flex flex-column md:flex-row align-items-start md:align-items-center gap-3 cursor-pointer hover:shadow-3 transition-colors transition-duration-200 hover:surface-hover"
      @click="goToDetail"
    >
      <div class="flex align-items-center gap-3 flex-1">
        <!-- Typ Icon -->
        <div 
          class="flex align-items-center justify-content-center border-circle"
          :style="{ backgroundColor: typeColor + '20', color: typeColor, width: '2.5rem', height: '2.5rem' }"
        >
          <i class="pi pi-ticket font-bold"></i>
        </div>
        
        <!-- Info -->
        <div class="flex flex-column">
          <div class="font-semibold text-lg line-height-1 mb-1">{{ incident.name || 'Unbenannter Vorfall' }}</div>
          <div class="text-500 text-sm flex gap-2 align-items-center">
            <span class="font-medium text-color">{{ incident.type || 'Unbekannt' }}</span>
            <span>&bull;</span>
            <span class="text-xs">{{ new Date(incident.created_at).toLocaleDateString() }}</span>
          </div>
        </div>
      </div>

      <!-- Aktionen -->
      <div class="flex gap-2 w-full md:w-auto mt-2 md:mt-0 justify-content-end">
        <Button 
          icon="pi pi-eye-slash" 
          severity="success" 
          text
          tooltip="Als gesehen markieren" 
          tooltipOptions="{position: 'top'}"
          @click="handleDelete"
        />
        <Button 
          icon="pi pi-arrow-right" 
          severity="secondary" 
          text
          @click="goToDetail"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.related-card-wrapper {
  margin-bottom: 0.5rem;
}
</style>
