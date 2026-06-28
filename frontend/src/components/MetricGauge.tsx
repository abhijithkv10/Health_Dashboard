interface Props {
  label: string
  value: number | null
  unit?: string
  warning: number
  critical: number
}

export default function MetricGauge({ label, value, unit = '%', warning, critical }: Props) {
  if (value === null || value === undefined) {
    return (
      <div>
        <div className="text-xs text-gray-400 mb-1">{label}</div>
        <div className="text-sm text-gray-500">No data</div>
      </div>
    )
  }

  const barColor = value >= critical
    ? 'bg-red-500'
    : value >= warning
      ? 'bg-yellow-500'
      : 'bg-green-500'

  return (
    <div>
      <div className="flex justify-between text-xs mb-1">
        <span className="text-gray-400">{label}</span>
        <span className="font-medium">{value}{unit}</span>
      </div>
      <div className="w-full bg-gray-700 rounded-full h-2">
        <div
          className={`h-2 rounded-full transition-all duration-500 ${barColor}`}
          style={{ width: `${Math.min(value, 100)}%` }}
        />
      </div>
    </div>
  )
}
