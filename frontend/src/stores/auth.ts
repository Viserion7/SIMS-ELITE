import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserAuthorized } from '@/types/auth'
import type { User } from '@/types/identity'

const ACCESS_TOKEN_KEY = 'sims_access_token'
const REFRESH_TOKEN_KEY = 'sims_refresh_token'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY))
  const refreshToken = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
  const currentUser = ref<UserAuthorized | null>(null)
  const userDetails = ref<User | null>(null)

  const isAuthenticated = computed(() => !!accessToken.value)
  const isAdmin = computed(() => !!userDetails.value?.is_Admin)

  function setTokens(access: string, refresh: string) {
    accessToken.value = access
    refreshToken.value = refresh
    localStorage.setItem(ACCESS_TOKEN_KEY, access)
    localStorage.setItem(REFRESH_TOKEN_KEY, refresh)
  }

  function setCurrentUser(user: UserAuthorized | null) {
    currentUser.value = user
  }

  function setUserDetails(details: User | null) {
    userDetails.value = details
  }

  function clearAuth() {
    accessToken.value = null
    refreshToken.value = null
    currentUser.value = null
    userDetails.value = null
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  }

  return {
    accessToken,
    refreshToken,
    currentUser,
    userDetails,
    isAuthenticated,
    isAdmin,
    setTokens,
    setCurrentUser,
    setUserDetails,
    clearAuth,
  }
})
