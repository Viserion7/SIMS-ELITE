<script setup lang="ts">
import { ref, computed } from 'vue'
import { useUsersQuery, useUserDetailsQuery, useLevelsQuery, useCategoriesQuery, useAssignLevelMutation, useRemoveLevelMutation } from '@/composables/queries/useUserQueries'
import PageHeader from '@/components/PageHeader.vue'
import EmptyState from '@/components/EmptyState.vue'
import Listbox from 'primevue/listbox'
import ToggleSwitch from 'primevue/toggleswitch'
import Badge from 'primevue/badge'
import Skeleton from 'primevue/skeleton'
import { useToast } from 'primevue/usetoast'

const toast = useToast()

const selectedUserId = ref<number | null>(null)

const { data: users, isPending: loadingUsers } = useUsersQuery()
const { data: levels, isPending: loadingLevels } = useLevelsQuery()
const { data: categories, isPending: loadingCats } = useCategoriesQuery()
const { data: userDetails, isPending: loadingDetails } = useUserDetailsQuery(() => selectedUserId.value || 0)

const assignMutation = useAssignLevelMutation()
const removeMutation = useRemoveLevelMutation()

// Kategorien gruppiert nach Level ID für die Anzeige
const categoriesByLevel = computed(() => {
  const map = new Map<number, string[]>()
  if (!categories.value) return map
  
  for (const cat of categories.value) {
    if (!map.has(cat.levelId)) {
      map.set(cat.levelId, [])
    }
    if (cat.name) {
      map.get(cat.levelId)!.push(cat.name)
    }
  }
  return map
})

const isLevelAssigned = (levelId: number) => {
  if (!userDetails.value?.levels) return false
  return userDetails.value.levels.some(l => l.id === levelId)
}

const handleToggleLevel = async (levelId: number, currentlyAssigned: boolean) => {
  if (!selectedUserId.value) return

  try {
    if (currentlyAssigned) {
      await removeMutation.mutateAsync({ userId: selectedUserId.value, levelId })
    } else {
      await assignMutation.mutateAsync({ userId: selectedUserId.value, levelId })
    }
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Zuständigkeit aktualisiert.', life: 2000 })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Update fehlgeschlagen.', life: 3000 })
  }
}
</script>

<template>
  <div class="page-container">
    <PageHeader 
      title="Eskalationsstufen" 
      description="Weisen Sie Benutzern Zuständigkeitsbereiche (Levels) zu, um deren Sichtbarkeit und Eskalationsbenachrichtigungen zu steuern."
    />

    <div class="layout-grid">
      <!-- Left Column: User Selection -->
      <div class="users-panel">
        <h3 class="panel-title">Benutzer wählen</h3>
        
        <div v-if="loadingUsers" class="p-3">
          <Skeleton height="3rem" class="mb-2" v-for="i in 5" :key="i" />
        </div>
        
        <Listbox 
          v-else
          v-model="selectedUserId" 
          :options="users" 
          optionLabel="email" 
          optionValue="id"
          class="w-full users-listbox" 
          listStyle="max-height: 60vh"
        >
          <template #option="slotProps">
            <div class="flex align-items-center justify-content-between w-full">
              <span>{{ slotProps.option.email }}</span>
              <Badge v-if="slotProps.option.is_Admin" value="Admin" severity="secondary" size="small" />
            </div>
          </template>
        </Listbox>
      </div>

      <!-- Right Column: Level Assignment -->
      <div class="levels-panel">
        <EmptyState 
          v-if="!selectedUserId"
          icon="pi-user"
          title="Kein Benutzer ausgewählt"
          description="Wählen Sie links einen Benutzer aus, um dessen Zuständigkeiten zu bearbeiten."
        />
        
        <div v-else-if="loadingDetails || loadingLevels || loadingCats" class="p-4">
          <Skeleton height="8rem" class="mb-4" v-for="i in 3" :key="i" />
        </div>

        <div v-else class="levels-content">
          <h3 class="panel-title mb-4">
            Zuständigkeiten für: <span class="text-primary">{{ userDetails?.email }}</span>
          </h3>

          <div v-if="!levels?.length" class="p-3 surface-200 border-round text-center">
            Keine Eskalationsstufen im System definiert.
          </div>

          <div class="level-cards">
            <div 
              v-for="level in levels" 
              :key="level.id" 
              class="level-card"
              :class="{ 'is-active': isLevelAssigned(level.id) }"
            >
              <div class="level-header">
                <div class="level-title">
                  <h4>{{ level.name }}</h4>
                </div>
                <ToggleSwitch 
                  :modelValue="isLevelAssigned(level.id)" 
                  @change="handleToggleLevel(level.id, isLevelAssigned(level.id))"
                  :disabled="assignMutation.isPending.value || removeMutation.isPending.value"
                />
              </div>
              
              <div class="categories-list" v-if="categoriesByLevel.get(level.id)?.length">
                <span class="cat-label">Zugehörige Vorfalls-Kategorien:</span>
                <div class="cat-tags">
                  <Badge 
                    v-for="cat in categoriesByLevel.get(level.id)" 
                    :key="cat" 
                    :value="cat" 
                    severity="info" 
                  />
                </div>
              </div>
              <div v-else class="text-sm text-color-secondary mt-2">
                Keine Kategorien mit dieser Stufe verknüpft.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.layout-grid {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 2rem;
  align-items: start;
}

@media (max-width: 900px) {
  .layout-grid {
    grid-template-columns: 1fr;
  }
}

.panel-title {
  margin: 0 0 1rem 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.users-panel {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 1.5rem;
}

:deep(.users-listbox) {
  border: 1px solid var(--border-color);
  background: var(--bg-color);
}

.levels-panel {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  min-height: 400px;
}

.levels-content {
  padding: 1.5rem;
}

.level-cards {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.level-card {
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.5rem;
  background-color: var(--bg-color);
  transition: all 0.2s ease;
}

.level-card.is-active {
  border-color: var(--primary-color);
  background-color: rgba(59, 130, 246, 0.05); /* very light primary */
}

.level-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.level-title h4 {
  margin: 0 0 0.25rem 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-color);
  text-transform: capitalize;
}

.level-desc {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.categories-list {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px dashed var(--border-color);
}

.cat-label {
  display: block;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-color-secondary);
  text-transform: uppercase;
  margin-bottom: 0.5rem;
}

.cat-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
</style>
