import { identityApiClient } from './client'
import type {
  CreateUserDto,
  UserAuthorized,
  RefreshTokenRequest,
  TokenResponse,
  User,
  UserDto,
  UpdateUserDto,
  UsersToNotify,
  Category,
} from '@/types'

export const identityApi = {
  // Auth Endpoints
  login: async (dto: CreateUserDto): Promise<TokenResponse> => {
    const res = await identityApiClient<TokenResponse>('/api/v1/Auth/login', {
      method: 'POST',
      body: dto,
      skipAuth: true,
    })
    return res!
  },

  getMe: async (): Promise<UserAuthorized> => {
    const res = await identityApiClient<UserAuthorized>('/api/v1/Auth/me', {
      method: 'GET',
    })
    return res!
  },

  refreshToken: async (dto: RefreshTokenRequest): Promise<TokenResponse> => {
    const res = await identityApiClient<TokenResponse>('/api/v1/Auth/refresh', {
      method: 'POST',
      body: dto,
      skipAuth: true,
    })
    return res!
  },

  logout: async (dto: RefreshTokenRequest): Promise<void> => {
    await identityApiClient<void>('/api/v1/Auth/logout', {
      method: 'POST',
      body: dto,
    })
  },

  // User Management
  getUsers: async (): Promise<User[]> => {
    const res = await identityApiClient<User[]>('/api/v1/User', {
      method: 'GET',
    })
    return res ?? []
  },

  createUser: async (dto: CreateUserDto): Promise<void> => {
    await identityApiClient<void>('/api/v1/User', {
      method: 'POST',
      body: dto,
    })
  },

  getUserById: async (id: number): Promise<UserDto> => {
    const res = await identityApiClient<UserDto>(`/api/v1/User/${id}`, {
      method: 'GET',
    })
    return res!
  },

  getUserDetails: async (id: number): Promise<User> => {
    const res = await identityApiClient<User>(`/api/v1/User/${id}/details`, {
      method: 'GET',
    })
    return res!
  },

  updateUser: async (id: number, dto: UpdateUserDto): Promise<void> => {
    await identityApiClient<void>(`/api/v1/User/${id}`, {
      method: 'PUT',
      body: dto,
    })
  },

  deleteUser: async (id: number): Promise<void> => {
    await identityApiClient<void>(`/api/v1/User/${id}`, {
      method: 'DELETE',
    })
  },

  getUsersToNotify: async (): Promise<UsersToNotify[]> => {
    const res = await identityApiClient<UsersToNotify[]>('/api/v1/User/toNotify', {
      method: 'GET',
      skipAuth: true,
    })
    return res ?? []
  },

  // Categories & Assignments
  getCategories: async (): Promise<Category[]> => {
    const res = await identityApiClient<Category[]>('/api/v1/Categories/categories', {
      method: 'GET',
    })
    return res ?? []
  },

  assignLevel: async (userId: number, levelId: number): Promise<User> => {
    const res = await identityApiClient<User>(
      `/api/v1/Assignment/user/${userId}/level/${levelId}`,
      {
        method: 'POST',
      },
    )
    return res!
  },

  removeLevel: async (userId: number, levelId: number): Promise<User> => {
    const res = await identityApiClient<User>(
      `/api/v1/Assignment/user/${userId}/level/${levelId}`,
      {
        method: 'DELETE',
      },
    )
    return res!
  },
}
