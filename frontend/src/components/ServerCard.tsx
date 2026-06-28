import { useNavigate } from 'react-router-dom'
import StatusBadge from './StatusBadge'
import MetricGauge from './MetricGauge'
import type { InstanceStatus } from '../types'

interface Props {
  instance: InstanceStatus
}

export default function ServerCard({ instance }: Props) {
  const navigate = useNavigate()
  const borderColor = instance.status === 'Critical'
    ? 'border-red-500'
    : instance.status === 'Warning'
      ? 'border-yellow-500'
      : 'border-gray-700'

  return (
    <div
      className={`bg-gray-800 rounded-lg border-2 ${borderColor} p-4 cursor-pointer hover:shadow-lg hover:shadow-blue-500/10 transition-all`}
      onClick={() => navigate(`/instance/${instance.instanceId}`)}
    >
      <div className="flex items-center justify-between mb-3">
        <h3 className="font-semibold text-white truncate">{instance.name}</h3>
        <StatusBadge status={instance.status} />
      </div>

      <div className="text-xs text-gray-400 mb-3 truncate">{instance.instanceId}</div>

      <div className="space-y-3">
        <MetricGauge label="CPU" value={instance.cpuPercent} warning={60} critical={80} />
        <MetricGauge label="Memory" value={instance.memoryPercent} warning={70} critical={80} />
        <MetricGauge label="Disk" value={instance.diskPercent} warning={80} critical={90} />
      </div>

      {instance.alertMessage && (
        <div className="mt-3 text-xs text-red-400 bg-red-900/30 rounded px-2 py-1">
          {instance.alertMessage}
        </div>
      )}

      <div className="mt-3 text-xs text-gray-500">
        Updated: {instance.lastUpdated ? new Date(instance.lastUpdated).toLocaleTimeString() : 'Never'}
      </div>
    </div>
  )
}
