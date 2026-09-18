import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AuthLayout from '@/layouts/AuthLayout.vue'
import AppLayout from '@/layouts/AppLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/incidents',
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { layout: AuthLayout, requiresAuth: false },
    },
    {
      path: '/upload',
      name: 'upload',
      component: () => import('@/views/StixUploadView.vue'),
      meta: { layout: AuthLayout, requiresAuth: false },
    },
    {
      path: '/incidents',
      name: 'incidents',
      component: () => import('@/views/MyIncidentsView.vue'),
      meta: { layout: AppLayout, requiresAuth: true },
    },
    {
      path: '/search',
      name: 'search',
      component: () => import('@/views/SearchView.vue'),
      meta: { layout: AppLayout, requiresAuth: true },
    },
    {
      path: '/incidents/all',
      name: 'incidents-all',
      component: () => import('@/views/AllIncidentsView.vue'),
      meta: { layout: AppLayout, requiresAuth: true },
    },
    {
      path: '/incidents/:id',
      name: 'incident-detail',
      component: () => import('@/views/IncidentDetailView.vue'),
      meta: { layout: AppLayout, requiresAuth: true },
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: () => import('@/views/admin/AdminUsersView.vue'),
      meta: { layout: AppLayout, requiresAuth: true, requiresAdmin: true },
    },
    {
      path: '/admin/assignments',
      name: 'admin-assignments',
      component: () => import('@/views/admin/AdminAssignmentsView.vue'),
      meta: { layout: AppLayout, requiresAuth: true, requiresAdmin: true },
    },
  ],
})

import { identityApi } from '@/api'

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next('/login')
  }

  // Lade User-Details, falls wir eingeloggt sind aber noch keine Details haben
  if (authStore.isAuthenticated && !authStore.userDetails) {
    try {
      const user = await identityApi.getMe()
      authStore.setCurrentUser(user)
      const details = await identityApi.getUserDetails(user.id)
      authStore.setUserDetails(details)
    } catch {
      // Fehler beim Laden ignorieren, wird dann halt als nicht-Admin behandelt
    }
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return next('/incidents')
  }

  if (authStore.isAuthenticated && to.path === '/login') {
    return next('/incidents')
  }

  next()
})

export default router
