export interface CreateUserDto {
  email: string
  password: string
}

export interface UserAuthorized {
  id: number
  email: string
  authenticated: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
}
