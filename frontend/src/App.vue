<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { useCurrentUserQuery } from '@/composables/queries/useAuthQueries'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/layouts/AuthLayout.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

const authStore = useAuthStore()
const route = useRoute()

if (authStore.isAuthenticated) {
  useCurrentUserQuery()
}

const currentLayout = computed(() => {
  if (route.meta.layout) return route.meta.layout
  // Für Routen ohne meta.layout (wie /upload) dynamisch entscheiden
  return authStore.isAuthenticated ? AppLayout : AuthLayout
})
</script>

<template>
  <Toast />
  <ConfirmDialog />
  <component :is="currentLayout">
    <RouterView />
  </component>
</template>

<style>
/* Global resets handled in main.css */
</style>
