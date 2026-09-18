export interface Category {
  id: number
  name: string
  description: string
  levelId: number
}

export interface Level {
  id: number
  name: string
  categorys?: Category[]
}

export interface User {
  id: number
  email: string
  password_hash?: string
  is_deleted: boolean
  is_Admin: boolean
  is_ToNotify: boolean
  levels?: Level[]
}

export interface UserDto {
  id: number
  email: string
  is_deleted: boolean
  is_Admin: boolean
  is_ToNotify: boolean
}

export interface UpdateUserDto {
  email?: string | null
  password?: string | null
  is_deleted?: boolean | null
  is_Admin?: boolean | null
  is_ToNotify?: boolean | null
}

export interface UsersToNotify {
  id: number
  email: string
}
