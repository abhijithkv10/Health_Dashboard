export interface InstanceStatus {
  instanceId: string
  name: string
  status: 'OK' | 'Warning' | 'Critical'
  cpuPercent: number | null
  memoryPercent: number | null
  diskPercent: number | null
  cpuStatus: string
  memoryStatus: string
  diskStatus: string
  lastUpdated: string
  alertMessage: string | null
}

export interface MetricSnapshot {
  instanceId: string
  timestamp: string
  cpuPercent: number | null
  memoryPercent: number | null
  diskPercent: number | null
}

export interface MetricsResponse {
  instanceId: string
  latest: MetricSnapshot | null
  history: MetricSnapshot[]
  count: number
}
