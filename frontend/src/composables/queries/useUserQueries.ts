import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { identityApi } from '@/api'
import { useAuthStore } from '@/stores/auth'
import { queryKeys } from './queryKeys'
import type { CreateUserDto, UpdateUserDto } from '@/types'

export function useUsersQuery() {
  const authStore = useAuthStore()

  return useQuery({
    queryKey: queryKeys.users.list(),
    queryFn: () => identityApi.getUsers(),
    enabled: () => authStore.isAuthenticated,
  })
}

export function useUserDetailsQuery(userId: MaybeRefOrGetter<number>) {
  const authStore = useAuthStore()

  return useQuery({
    queryKey: computed(() => queryKeys.users.detail(toValue(userId))),
    queryFn: () => identityApi.getUserDetails(toValue(userId)),
    enabled: () => authStore.isAuthenticated && toValue(userId) > 0,
  })
}

export function useUsersToNotifyQuery() {
  return useQuery({
    queryKey: queryKeys.users.toNotify(),
    queryFn: () => identityApi.getUsersToNotify(),
  })
}

export function useCategoriesQuery() {
  const authStore = useAuthStore()

  return useQuery({
    queryKey: queryKeys.categories.list(),
    queryFn: () => identityApi.getCategories(),
    enabled: () => authStore.isAuthenticated,
  })
}

export function useLevelsQuery() {
  const authStore = useAuthStore()

  return useQuery({
    queryKey: queryKeys.levels.list(),
    queryFn: () => identityApi.getLevels(),
    enabled: () => authStore.isAuthenticated,
  })
}

export function useCreateUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dto: CreateUserDto) => identityApi.createUser(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: UpdateUserDto }) =>
      identityApi.updateUser(id, dto),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(id) })
    },
  })
}

export function useDeleteUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => identityApi.deleteUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useAssignLevelMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, levelId }: { userId: number; levelId: number }) =>
      identityApi.assignLevel(userId, levelId),
    onSuccess: (_, { userId }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(userId) })
    },
  })
}

export function useRemoveLevelMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, levelId }: { userId: number; levelId: number }) =>
      identityApi.removeLevel(userId, levelId),
    onSuccess: (_, { userId }) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(userId) })
    },
  })
}
