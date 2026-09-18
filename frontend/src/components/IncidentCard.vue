<script setup lang="ts">
import type { Incident } from '@/types'
import Badge from 'primevue/badge'
import Button from 'primevue/button'

const props = defineProps<{
  incident: Incident
  highlighted?: boolean
}>()

const emit = defineEmits<{
  (e: 'click', incident: Incident): void
  (e: 'delete', incident: Incident): void
}>()

const formatDate = (dateString: string) => {
  if (!dateString) return 'Unbekannt'
  return new Date(dateString).toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const getTypeSeverity = (type?: string): 'danger' | 'warn' | 'info' | 'secondary' | 'success' => {
  if (!type) return 'secondary'
  const t = type.toLowerCase()
  if (t.includes('malware') || t.includes('attack-pattern') || t.includes('vulnerability')) return 'danger'
  if (t.includes('indicator') || t.includes('threat-actor') || t.includes('campaign')) return 'warn'
  if (t.includes('identity') || t.includes('location') || t.includes('grouping')) return 'info'
  return 'secondary'
}

const shortId = (id: string) => {
  if (!id) return ''
  return id.length > 10 ? id.substring(0, 8) + '...' : id
}
</script>

<template>
  <div 
    class="incident-card" 
    :class="{ 'is-highlighted': highlighted }"
    @click="emit('click', incident)"
  >
    <!-- Accent line if highlighted -->
    <div v-if="highlighted" class="card-accent-bar"></div>

    <div class="card-header">
      <div class="header-left">
        <Badge 
          :value="incident.type" 
          :severity="getTypeSeverity(incident.type)" 
          class="type-badge" 
        />
        <span class="incident-id" :title="incident.id">
          <i class="pi pi-hashtag"></i> {{ shortId(incident.id) }}
        </span>
      </div>

      <!-- Quick Action: Als gesehen markieren -->
      <Button 
        icon="pi pi-check" 
        severity="success" 
        text 
        rounded 
        size="small"
        v-tooltip.top="'Als gesehen markieren (Löschen)'"
        class="quick-seen-btn" 
        aria-label="Als gesehen markieren"
        @click.stop="emit('delete', incident)"
      />
    </div>
    
    <h3 class="incident-name" :title="incident.name || 'Unbenannter Vorfall'">
      {{ incident.name || 'Unbenannter Vorfall' }}
    </h3>
    
    <div class="card-meta">
      <span class="meta-item" :title="'Erstellt am ' + formatDate(incident.created_at)">
        <i class="pi pi-calendar text-xs"></i>
        {{ formatDate(incident.created_at) }}
      </span>
      <span class="meta-item" :title="'Quellformat: ' + incident.source_format">
        <i class="pi pi-database text-xs"></i>
        {{ incident.source_format }}
      </span>
    </div>
    
    <p class="incident-desc">
      {{ incident.desc || 'Keine Beschreibung vorhanden.' }}
    </p>

    <div class="card-footer">
      <div v-if="highlighted" class="highlight-indicator">
        <i class="pi pi-shield-fill"></i>
        <span>In deiner Zuständigkeit</span>
      </div>
      <div v-else class="status-indicator">
        <i class="pi pi-circle-fill text-green-500 text-xs"></i>
        <span>Aktiv</span>
      </div>

      <span class="details-hint">
        Details <i class="pi pi-arrow-right text-xs ml-1"></i>
      </span>
    </div>
  </div>
</template>

<style scoped>
.incident-card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 1.25rem 1.25rem 1rem 1.25rem;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.incident-card:hover {
  border-color: var(--primary-color);
  transform: translateY(-3px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
}

.card-accent-bar {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, var(--primary-color), #6366f1);
}

.incident-card.is-highlighted {
  border-color: rgba(59, 130, 246, 0.4);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
  gap: 0.5rem;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.type-badge {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.incident-id {
  font-size: 0.75rem;
  font-family: monospace;
  color: var(--text-color-secondary);
  background-color: var(--bg-color);
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
}

.quick-seen-btn {
  width: 2rem !important;
  height: 2rem !important;
  transition: background-color 0.2s;
}

.quick-seen-btn:hover {
  background-color: rgba(34, 197, 94, 0.15) !important;
}

.incident-name {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--text-color);
  line-height: 1.35;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-meta {
  display: flex;
  gap: 1rem;
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}

.meta-item {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.incident-desc {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
  margin: 0 0 1rem 0;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 0.75rem;
  border-top: 1px solid var(--border-color);
  margin-top: auto;
}

.highlight-indicator {
  font-size: 0.75rem;
  color: var(--primary-color);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.status-indicator {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.details-hint {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
  display: flex;
  align-items: center;
  transition: color 0.2s, transform 0.2s;
}

.incident-card:hover .details-hint {
  color: var(--primary-color);
  transform: translateX(2px);
}
</style>
