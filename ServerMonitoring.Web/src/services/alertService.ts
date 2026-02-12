import api from './api'

export interface Alert {
  id: number
  serverId: number
  serverName: string
  type: string
  severity: string
  title: string
  message: string
  thresholdValue?: number
  actualValue?: number
  isAcknowledged: boolean
  acknowledgedAt?: string
  acknowledgedBy?: string
  isResolved: boolean
  resolvedAt?: string
  createdAt: string
}

export const alertService = {
  getAll: async (unacknowledgedOnly = false) => {
    const response = await api.get<Alert[]>('/v1/alerts', {
      params: { unacknowledgedOnly },
    })
    return response.data
  },

  getByServer: async (serverId: number) => {
    const response = await api.get<Alert[]>(`/v1/alerts/server/${serverId}`)
    return response.data
  },

  acknowledge: async (id: number) => {
    await api.post(`/v1/alerts/${id}/acknowledge`)
  },

  resolve: async (id: number) => {
    await api.post(`/v1/alerts/${id}/resolve`)
  },
}
