import api from './api'

export interface Report {
  id: number
  title: string
  description: string
  type: string
  status: string
  startDate: string
  endDate: string
  filePath?: string
  fileFormat?: string
  fileSizeBytes?: number
  generatedAt?: string
  errorMessage?: string
}

export const reportService = {
  getAll: async () => {
    const response = await api.get<Report[]>('/v1/reports')
    return response.data
  },

  getById: async (id: number) => {
    const response = await api.get<Report>(`/v1/reports/${id}`)
    return response.data
  },

  generate: async (data: { title: string; description: string; type: string; startDate: string; endDate: string }) => {
    const response = await api.post<Report>('/v1/reports/generate', data)
    return response.data
  },

  download: async (id: number) => {
    const response = await api.get(`/v1/reports/${id}/download`, {
      responseType: 'blob',
    })
    return response.data
  },
}
