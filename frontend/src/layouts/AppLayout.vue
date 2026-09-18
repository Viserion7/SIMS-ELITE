<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useLogoutMutation } from '@/composables/queries/useAuthQueries'
import Menu from 'primevue/menu'
import Button from 'primevue/button'

const router = useRouter()
const authStore = useAuthStore()
const logoutMutation = useLogoutMutation()

const handleLogout = async () => {
  await logoutMutation.mutateAsync()
  router.push('/login')
}

const menuItems = computed(() => {
  const items = [
    {
      label: 'Main',
      items: [
        { label: 'Meine Incidents', icon: 'pi pi-ticket', command: () => router.push('/incidents') },
        { label: 'Alle Incidents', icon: 'pi pi-list', command: () => router.push('/incidents/all') },
        { label: 'STIX Upload', icon: 'pi pi-upload', command: () => router.push('/upload') },
      ],
    },
  ]

  if (authStore.isAdmin) {
    items.push({
      label: 'Admin',
      items: [
        { label: 'Benutzer', icon: 'pi pi-users', command: () => router.push('/admin/users') },
        { label: 'Eskalationsstufen', icon: 'pi pi-sitemap', command: () => router.push('/admin/assignments') },
      ],
    })
  }

  return items
})
</script>

<template>
  <div class="app-layout">
    <aside class="sidebar">
      <div class="sidebar-header">
        <span class="logo-text">SIMS-ELITE</span>
      </div>
      
      <div class="sidebar-content">
        <Menu :model="menuItems" class="nav-menu" />
      </div>

      <div class="sidebar-footer">
        <div class="user-info">
          <div class="user-email">{{ authStore.currentUser?.email }}</div>
          <div v-if="authStore.isAdmin" class="user-badge">Admin</div>
        </div>
        <Button 
          icon="pi pi-sign-out" 
          label="Abmelden" 
          severity="secondary" 
          text 
          class="w-full justify-content-start" 
          @click="handleLogout" 
        />
      </div>
    </aside>

    <main class="main-content">
      <slot></slot>
    </main>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--bg-color);
}

.sidebar {
  width: 260px;
  background-color: var(--surface-color);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  position: fixed;
  height: 100vh;
  left: 0;
  top: 0;
}

.sidebar-header {
  padding: 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

.logo-text {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--primary-color);
  letter-spacing: 1px;
}

.sidebar-content {
  flex: 1;
  padding: 1rem;
  overflow-y: auto;
}

:deep(.nav-menu) {
  border: none;
  background: transparent;
  padding: 0;
}

.sidebar-footer {
  padding: 1rem;
  border-top: 1px solid var(--border-color);
}

.user-info {
  margin-bottom: 1rem;
  padding: 0 0.5rem;
}

.user-email {
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 0.25rem;
  word-break: break-all;
}

.user-badge {
  display: inline-block;
  background-color: var(--primary-color);
  color: white;
  font-size: 0.75rem;
  padding: 0.1rem 0.5rem;
  border-radius: 12px;
  font-weight: 600;
}

.main-content {
  flex: 1;
  margin-left: 260px;
  min-height: 100vh;
}
</style>
