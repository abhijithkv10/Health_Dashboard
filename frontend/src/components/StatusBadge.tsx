interface Props {
  status: string
}

const colors: Record<string, string> = {
  OK: 'bg-green-600 text-green-100',
  Warning: 'bg-yellow-600 text-yellow-100',
  Critical: 'bg-red-600 text-red-100',
}

export default function StatusBadge({ status }: Props) {
  return (
    <span className={`inline-block px-2 py-0.5 rounded text-xs font-semibold ${colors[status] || 'bg-gray-600 text-gray-100'}`}>
      {status}
    </span>
  )
}
