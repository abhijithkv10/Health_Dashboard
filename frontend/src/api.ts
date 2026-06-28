import axios from 'axios'
import type { InstanceStatus, MetricsResponse } from './types'

const api = axios.create({ baseURL: '/api' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export async function fetchInstances(): Promise<InstanceStatus[]> {
  const { data } = await api.get('/instances')
  return data
}

export async function fetchMetrics(instanceId: string, minutes = 60): Promise<MetricsResponse> {
  const { data } = await api.get(`/metrics/${instanceId}`, { params: { minutes } })
  return data
}

export async function googleLogin(idToken: string) {
  const { data } = await api.post('/auth/google', { idToken })
  return data as { token: string; email: string; name: string; picture: string }
}

export interface AdminInstance {
  id: number
  instanceId: string
  name: string
  region: string
}

export async function adminGetInstances(): Promise<AdminInstance[]> {
  const { data } = await api.get('/admin/instances')
  return data
}

export async function adminAddInstance(instance: { instanceId: string; name: string; region: string }) {
  const { data } = await api.post('/admin/instances', instance)
  return data as AdminInstance
}

export async function adminUpdateInstance(id: number, instance: { instanceId: string; name: string; region: string }) {
  const { data } = await api.put(`/admin/instances/${id}`, instance)
  return data
}

export async function adminDeleteInstance(id: number) {
  await api.delete(`/admin/instances/${id}`)
}
