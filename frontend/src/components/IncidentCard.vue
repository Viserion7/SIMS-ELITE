<script setup lang="ts">
import type { Incident } from '@/types'
import Badge from 'primevue/badge'

defineProps<{
  incident: Incident
  highlighted?: boolean
}>()

const emit = defineEmits<{
  (e: 'click', incident: Incident): void
}>()

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <div 
    class="incident-card" 
    :class="{ 'is-highlighted': highlighted }"
    @click="emit('click', incident)"
  >
    <div class="card-header">
      <h3 class="incident-name" :title="incident.name || 'Unbenannter Vorfall'">
        {{ incident.name || 'Unbenannter Vorfall' }}
      </h3>
      <Badge :value="incident.type" severity="info" />
    </div>
    
    <div class="card-meta">
      <span class="meta-item">
        <i class="pi pi-clock"></i>
        {{ formatDate(incident.created_at) }}
      </span>
      <span class="meta-item">
        <i class="pi pi-database"></i>
        {{ incident.source_format }}
      </span>
    </div>
    
    <p class="incident-desc">
      {{ incident.desc || 'Keine Beschreibung vorhanden.' }}
    </p>

    <div v-if="highlighted" class="highlight-badge">
      <i class="pi pi-star-fill"></i> In deiner Zuständigkeit
    </div>
  </div>
</template>

<style scoped>
.incident-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.25rem;
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  display: flex;
  flex-direction: column;
}

.incident-card:hover {
  border-color: var(--primary-color);
  transform: translateY(-2px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
  gap: 1rem;
}

.incident-name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-meta {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

.meta-item {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.incident-desc {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.highlight-badge {
  margin-top: 1rem;
  font-size: 0.75rem;
  color: var(--primary-color);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}
</style>
