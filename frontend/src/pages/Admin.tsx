import { useEffect, useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import {
  adminGetInstances, adminAddInstance, adminUpdateInstance, adminDeleteInstance,
  type AdminInstance
} from '../api'

interface InstanceForm {
  instanceId: string
  name: string
  region: string
}

const emptyForm = (): InstanceForm => ({ instanceId: '', name: '', region: 'us-east-1' })

export default function Admin() {
  const { user, logout } = useAuth()
  const [instances, setInstances] = useState<AdminInstance[]>([])
  const [form, setForm] = useState<InstanceForm>(emptyForm())
  const [editingId, setEditingId] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    const data = await adminGetInstances()
    setInstances(data)
    setLoading(false)
  }

  useEffect(() => { load() }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (editingId !== null) {
      await adminUpdateInstance(editingId, form)
    } else {
      await adminAddInstance(form)
    }
    setForm(emptyForm())
    setEditingId(null)
    load()
  }

  const handleEdit = (inst: AdminInstance) => {
    setForm({ instanceId: inst.instanceId, name: inst.name, region: inst.region })
    setEditingId(inst.id)
  }

  const handleDelete = async (id: number, name: string) => {
    if (!confirm(`Remove instance "${name}"?`)) return
    await adminDeleteInstance(id)
    load()
  }

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-white">Admin Panel</h2>
        <div className="flex items-center gap-4">
          {user && (
            <div className="flex items-center gap-2 text-sm text-gray-400">
              {user.picture && <img src={user.picture} alt="" className="w-6 h-6 rounded-full" />}
              {user.email}
            </div>
          )}
          <button onClick={() => { logout(); window.location.href = '/' }}
            className="text-sm text-red-400 hover:text-red-300 transition-colors">
            Sign Out
          </button>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="bg-gray-800 rounded-lg p-4 mb-6 flex gap-3 items-end">
        <div className="flex-1">
          <label className="block text-xs text-gray-400 mb-1">Instance ID (i-xxxx)</label>
          <input value={form.instanceId} onChange={e => setForm({ ...form, instanceId: e.target.value })}
            className="w-full bg-gray-700 rounded px-3 py-2 text-white text-sm" required />
        </div>
        <div className="flex-1">
          <label className="block text-xs text-gray-400 mb-1">Name</label>
          <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}
            className="w-full bg-gray-700 rounded px-3 py-2 text-white text-sm" required />
        </div>
        <div className="w-32">
          <label className="block text-xs text-gray-400 mb-1">Region</label>
          <input value={form.region} onChange={e => setForm({ ...form, region: e.target.value })}
            className="w-full bg-gray-700 rounded px-3 py-2 text-white text-sm" />
        </div>
        <button type="submit" className="bg-blue-600 hover:bg-blue-500 text-white px-4 py-2 rounded text-sm">
          {editingId !== null ? 'Update' : 'Add'}
        </button>
        {editingId !== null && (
          <button type="button" onClick={() => { setForm(emptyForm()); setEditingId(null) }}
            className="text-gray-400 hover:text-white text-sm px-2">
            Cancel
          </button>
        )}
      </form>

      {loading ? (
        <p className="text-gray-400 text-center">Loading...</p>
      ) : instances.length === 0 ? (
        <p className="text-gray-400 text-center">No instances configured yet. Add one above.</p>
      ) : (
        <div className="bg-gray-800 rounded-lg overflow-hidden">
          <table className="w-full text-left text-sm">
            <thead className="bg-gray-700 text-gray-300">
              <tr>
                <th className="px-4 py-2">Instance ID</th>
                <th className="px-4 py-2">Name</th>
                <th className="px-4 py-2">Region</th>
                <th className="px-4 py-2">Actions</th>
              </tr>
            </thead>
            <tbody className="text-gray-300">
              {instances.map(inst => (
                <tr key={inst.id} className="border-t border-gray-700">
                  <td className="px-4 py-2 font-mono text-xs">{inst.instanceId}</td>
                  <td className="px-4 py-2">{inst.name}</td>
                  <td className="px-4 py-2">{inst.region}</td>
                  <td className="px-4 py-2 flex gap-3">
                    <button onClick={() => handleEdit(inst)}
                      className="text-blue-400 hover:text-blue-300 text-xs">Edit</button>
                    <button onClick={() => handleDelete(inst.id, inst.name)}
                      className="text-red-400 hover:text-red-300 text-xs">Remove</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
