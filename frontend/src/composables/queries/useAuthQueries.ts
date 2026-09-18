import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { identityApi } from '@/api'
import { useAuthStore } from '@/stores/auth'
import { queryKeys } from './queryKeys'
import type { CreateUserDto } from '@/types'

export function useCurrentUserQuery() {
  const authStore = useAuthStore()

  return useQuery({
    queryKey: queryKeys.auth.me(),
    queryFn: async () => {
      const user = await identityApi.getMe()
      authStore.setCurrentUser(user)
      try {
        const details = await identityApi.getUserDetails(user.id)
        authStore.setUserDetails(details)
      } catch {
        // Nicht kritisch, falls Details nicht direkt abrufbar sind
      }
      return user
    },
    enabled: () => authStore.isAuthenticated,
    staleTime: 1000 * 60 * 5, // 5 Minuten Cache
  })
}

export function useLoginMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (dto: CreateUserDto) => {
      const tokenResponse = await identityApi.login(dto)
      authStore.setTokens(tokenResponse.accessToken, tokenResponse.refreshToken)

      // Benutzerdaten direkt nach erfolgreichem Login laden
      const me = await identityApi.getMe()
      authStore.setCurrentUser(me)

      try {
        const details = await identityApi.getUserDetails(me.id)
        authStore.setUserDetails(details)
      } catch {
        // Falls Admin/User Details noch nicht verfügbar sind
      }

      return { tokenResponse, me }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.auth.all })
    },
  })
}

export function useLogoutMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async () => {
      const token = authStore.refreshToken
      if (token) {
        try {
          await identityApi.logout({ refreshToken: token })
        } catch {
          // Ignorieren bei Token-Ablauf
        }
      }
      authStore.clearAuth()
    },
    onSuccess: () => {
      queryClient.clear()
    },
  })
}
