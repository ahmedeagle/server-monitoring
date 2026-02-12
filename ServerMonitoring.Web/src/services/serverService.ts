import api from './api'

export interface Server {
  id: number
  name: string
  hostname: string
  ipAddress: string
  port: number
  operatingSystem: string
  isActive: boolean
  lastCheckTime?: string
  status?: string
}

export interface Metric {
  id: number
  serverId: number
  cpuUsage: number
  memoryUsage: number
  diskUsage: number
  networkInbound: number
  networkOutbound: number
  responseTime: number
  recordedAt: string
}

export const serverService = {
  getAll: async () => {
    const response = await api.get<Server[]>('/v1/servers')
    return response.data
  },

  getById: async (id: number) => {
    const response = await api.get<Server>(`/v1/servers/${id}`)
    return response.data
  },

  create: async (data: Partial<Server>) => {
    const response = await api.post<Server>('/v1/servers', data)
    return response.data
  },

  update: async (id: number, data: Partial<Server>) => {
    const response = await api.put<Server>(`/v1/servers/${id}`, data)
    return response.data
  },

  delete: async (id: number) => {
    await api.delete(`/v1/servers/${id}`)
  },

  getMetrics: async (serverId: number, limit = 100) => {
    const response = await api.get<Metric[]>(`/v1/metrics/server/${serverId}`, {
      params: { limit },
    })
    return response.data
  },
}
