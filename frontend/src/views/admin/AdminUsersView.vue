<script setup lang="ts">
import { ref } from 'vue'
import { useUsersQuery, useUpdateUserMutation, useDeleteUserMutation } from '@/composables/queries/useUserQueries'
import PageHeader from '@/components/PageHeader.vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ToggleSwitch from 'primevue/toggleswitch'
import Badge from 'primevue/badge'
import Skeleton from 'primevue/skeleton'
import Button from 'primevue/button'
import { useToast } from 'primevue/usetoast'

const { data: users, isPending, isError } = useUsersQuery()
const updateMutation = useUpdateUserMutation()
const toast = useToast()

const handleToggleNotify = async (userId: number, currentVal: boolean) => {
  try {
    await updateMutation.mutateAsync({
      id: userId,
      dto: { is_ToNotify: !currentVal } // Toggle
    })
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Benachrichtigungsstatus aktualisiert.', life: 3000 })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Update fehlgeschlagen.', life: 3000 })
  }
}

const deleteMutation = useDeleteUserMutation()

const handleToggleAdmin = async (userId: number, currentVal: boolean) => {
  try {
    await updateMutation.mutateAsync({
      id: userId,
      dto: { is_Admin: !currentVal }
    })
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Admin-Status aktualisiert.', life: 3000 })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Update fehlgeschlagen.', life: 3000 })
  }
}

const handleDelete = async (userId: number) => {
  try {
    await deleteMutation.mutateAsync(userId)
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Benutzer erfolgreich gelöscht.', life: 3000 })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Löschen fehlgeschlagen.', life: 3000 })
  }
}

const handleRestore = async (userId: number) => {
  try {
    await updateMutation.mutateAsync({
      id: userId,
      dto: { is_deleted: false }
    })
    toast.add({ severity: 'success', summary: 'Erfolg', detail: 'Benutzer wiederhergestellt.', life: 3000 })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Fehler', detail: 'Wiederherstellung fehlgeschlagen.', life: 3000 })
  }
}
</script>

<template>
  <div class="page-container">
    <PageHeader 
      title="Benutzerverwaltung" 
      description="Verwalten Sie globale Berechtigungen und E-Mail-Benachrichtigungen für alle Mitarbeiter."
    />

    <div v-if="isError" class="error-msg">
      Fehler beim Laden der Benutzer.
    </div>

    <div class="card" v-else>
      <DataTable :value="users" :loading="isPending" stripedRows responsiveLayout="scroll" sortField="id" :sortOrder="1">
        <template #empty>
          <div class="text-center p-4">Keine Benutzer gefunden.</div>
        </template>
        <template #loading>
          <div class="p-4"><Skeleton width="100%" height="15rem" /></div>
        </template>

        <Column field="id" header="ID" style="width: 5rem" sortable></Column>
        
        <Column field="email" header="E-Mail" sortable>
          <template #body="slotProps">
            <span class="font-medium" :class="{ 'text-red-500 line-through': slotProps.data.is_deleted }">
              {{ slotProps.data.email }}
            </span>
          </template>
        </Column>

        <Column header="Status" style="width: 10rem">
          <template #body="slotProps">
            <Badge v-if="slotProps.data.is_deleted" value="Gelöscht" severity="danger" class="mr-2" />
            <Badge v-else-if="slotProps.data.is_Admin" value="Admin" severity="primary" class="mr-2" />
            <Badge v-else value="User" severity="secondary" class="mr-2" />
          </template>
        </Column>

        <Column header="Bei Eskalation benachrichtigen" style="width: 15rem; text-align: center">
          <template #body="slotProps">
            <ToggleSwitch 
              :modelValue="slotProps.data.is_ToNotify" 
              @change="handleToggleNotify(slotProps.data.id, slotProps.data.is_ToNotify)"
              :disabled="slotProps.data.is_deleted"
            />
          </template>
        </Column>

        <Column header="Ist Admin" style="width: 10rem; text-align: center">
          <template #body="slotProps">
            <ToggleSwitch 
              :modelValue="slotProps.data.is_Admin" 
              @change="handleToggleAdmin(slotProps.data.id, slotProps.data.is_Admin)" 
              :disabled="slotProps.data.is_deleted"
            />
          </template>
        </Column>

        <Column header="Aktionen" style="width: 8rem; text-align: center">
          <template #body="slotProps">
            <Button 
              v-if="!slotProps.data.is_deleted"
              icon="pi pi-trash" 
              severity="danger" 
              text 
              rounded
              title="Benutzer löschen"
              @click="handleDelete(slotProps.data.id)" 
            />
            <Button 
              v-else
              icon="pi pi-undo" 
              severity="secondary" 
              text 
              rounded
              title="Benutzer wiederherstellen"
              @click="handleRestore(slotProps.data.id)" 
            />
          </template>
        </Column>
      </DataTable>
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

.card {
  background-color: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  overflow: hidden;
}
</style>
