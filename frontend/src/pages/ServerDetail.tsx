import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import StatusBadge from '../components/StatusBadge'
import MetricGauge from '../components/MetricGauge'
import { fetchInstances, fetchMetrics } from '../api'
import type { InstanceStatus, MetricSnapshot } from '../types'

export default function ServerDetail() {
  const { instanceId } = useParams<{ instanceId: string }>()
  const navigate = useNavigate()
  const [instance, setInstance] = useState<InstanceStatus | null>(null)
  const [history, setHistory] = useState<MetricSnapshot[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const load = async () => {
      if (!instanceId) return
      try {
        const [instances, metrics] = await Promise.all([
          fetchInstances(),
          fetchMetrics(instanceId, 60)
        ])
        const found = instances.find(i => i.instanceId === instanceId)
        setInstance(found || null)
        setHistory(metrics.history)
      } catch {
        // silent
      } finally {
        setLoading(false)
      }
    }

    load()
    const interval = setInterval(load, 30000)
    return () => clearInterval(interval)
  }, [instanceId])

  if (loading) {
    return <div className="text-gray-400 text-center py-16">Loading...</div>
  }

  if (!instance) {
    return (
      <div className="text-center py-16">
        <p className="text-gray-400">Instance not found</p>
        <button onClick={() => navigate('/')} className="mt-4 text-blue-400 hover:text-blue-300">
          Back to Dashboard
        </button>
      </div>
    )
  }

  const chartData = history
    .filter(m => m.cpuPercent !== null)
    .map(m => ({
      time: new Date(m.timestamp).toLocaleTimeString(),
      CPU: m.cpuPercent,
      Memory: m.memoryPercent,
      Disk: m.diskPercent,
    }))

  const latestCpu = history.filter(m => m.cpuPercent !== null).pop()
  const latestMemory = history.filter(m => m.memoryPercent !== null).pop()
  const latestDisk = history.filter(m => m.diskPercent !== null).pop()

  return (
    <div>
      <button
        onClick={() => navigate('/')}
        className="text-blue-400 hover:text-blue-300 mb-4 block"
      >
        &larr; Back to Dashboard
      </button>

      <div className="bg-gray-800 rounded-lg border border-gray-700 p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-xl font-bold text-white">{instance.name}</h2>
            <p className="text-sm text-gray-400">{instance.instanceId}</p>
          </div>
          <StatusBadge status={instance.status} />
        </div>

        {instance.alertMessage && (
          <div className="bg-red-900/30 border border-red-700 rounded px-3 py-2 text-sm text-red-400 mb-4">
            {instance.alertMessage}
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="bg-gray-900/50 rounded-lg p-4">
            <MetricGauge label="CPU" value={latestCpu?.cpuPercent ?? null} warning={60} critical={80} />
          </div>
          <div className="bg-gray-900/50 rounded-lg p-4">
            <MetricGauge label="Memory" value={latestMemory?.memoryPercent ?? null} warning={70} critical={80} />
          </div>
          <div className="bg-gray-900/50 rounded-lg p-4">
            <MetricGauge label="Disk" value={latestDisk?.diskPercent ?? null} warning={80} critical={90} />
          </div>
        </div>
      </div>

      <div className="bg-gray-800 rounded-lg border border-gray-700 p-6">
        <h3 className="text-lg font-semibold text-white mb-4">CPU History (Last 60 min)</h3>
        {chartData.length === 0 ? (
          <p className="text-gray-500 text-center py-8">No CPU data available yet</p>
        ) : (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis dataKey="time" stroke="#9CA3AF" fontSize={12} />
              <YAxis domain={[0, 100]} stroke="#9CA3AF" fontSize={12} />
              <Tooltip
                contentStyle={{ backgroundColor: '#1F2937', border: '1px solid #374151', borderRadius: '8px' }}
                labelStyle={{ color: '#F3F4F6' }}
              />
              <Line type="monotone" dataKey="CPU" stroke="#3B82F6" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="Memory" stroke="#10B981" strokeWidth={2} dot={false} />
              <Line type="monotone" dataKey="Disk" stroke="#F59E0B" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        )}
      </div>
    </div>
  )
}
