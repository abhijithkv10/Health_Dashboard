import axios from 'axios'
import type { InstanceStatus, MetricsResponse } from './types'

const api = axios.create({ baseURL: '/api' })

export async function fetchInstances(): Promise<InstanceStatus[]> {
  const { data } = await api.get('/instances')
  return data
}

export async function fetchMetrics(instanceId: string, minutes = 60): Promise<MetricsResponse> {
  const { data } = await api.get(`/metrics/${instanceId}`, { params: { minutes } })
  return data
}
