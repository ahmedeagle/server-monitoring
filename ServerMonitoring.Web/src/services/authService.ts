import api from './api'

export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  user: {
    username: string
    email: string
    roles: string[]
  }
}

export const authService = {
  login: async (data: LoginRequest) => {
    const response = await api.post<AuthResponse>('/v1/auth/login', data)
    return response.data
  },

  register: async (data: RegisterRequest) => {
    const response = await api.post<AuthResponse>('/v1/auth/register', data)
    return response.data
  },

  getCurrentUser: async () => {
    const response = await api.get('/v1/auth/me')
    return response.data
  },
}
