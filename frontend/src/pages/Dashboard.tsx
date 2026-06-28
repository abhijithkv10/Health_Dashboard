import { useEffect, useState } from 'react'
import ServerCard from '../components/ServerCard'
import { fetchInstances } from '../api'
import type { InstanceStatus } from '../types'

export default function Dashboard() {
  const [instances, setInstances] = useState<InstanceStatus[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      try {
        const data = await fetchInstances()
        setInstances(data)
        setError(null)
      } catch (err) {
        setError('Failed to fetch instances. Ensure the API is running.')
      } finally {
        setLoading(false)
      }
    }

    load()
    const interval = setInterval(load, 30000)
    return () => clearInterval(interval)
  }, [])

  const critical = instances.filter(i => i.status === 'Critical')
  const warning = instances.filter(i => i.status === 'Warning')
  const ok = instances.filter(i => i.status === 'OK')

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-400 text-lg">Loading instances...</div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-red-400 bg-red-900/20 rounded-lg px-6 py-4">{error}</div>
      </div>
    )
  }

  return (
    <div>
      <div className="flex gap-4 mb-6">
        <div className="bg-red-900/30 border border-red-700 rounded-lg px-4 py-2">
          <div className="text-xs text-red-400">Critical</div>
          <div className="text-2xl font-bold text-red-400">{critical.length}</div>
        </div>
        <div className="bg-yellow-900/30 border border-yellow-700 rounded-lg px-4 py-2">
          <div className="text-xs text-yellow-400">Warning</div>
          <div className="text-2xl font-bold text-yellow-400">{warning.length}</div>
        </div>
        <div className="bg-green-900/30 border border-green-700 rounded-lg px-4 py-2">
          <div className="text-xs text-green-400">OK</div>
          <div className="text-2xl font-bold text-green-400">{ok.length}</div>
        </div>
      </div>

      {instances.length === 0 ? (
        <div className="text-gray-500 text-center py-16">
          <p className="text-lg">No instances configured</p>
          <p className="text-sm mt-2">Add EC2 instances to <code className="bg-gray-800 px-2 py-0.5 rounded">appsettings.json</code></p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {instances.map(instance => (
            <ServerCard key={instance.instanceId} instance={instance} />
          ))}
        </div>
      )}
    </div>
  )
}
