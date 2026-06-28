import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { adminLogin } from '../api'
import { useState } from 'react'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setError('')
      const data = await adminLogin(username, password)
      login(data.token, { email: data.email, name: data.name, picture: data.picture })
      navigate('/admin')
    } catch {
      setError('Invalid username or password.')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="bg-gray-800 rounded-lg p-8 shadow-lg max-w-sm w-full">
        <h2 className="text-xl font-bold text-white mb-2 text-center">Admin Sign In</h2>
        {error && <p className="text-red-400 mb-4 text-sm text-center">{error}</p>}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-400 mb-1">Username</label>
            <input value={username} onChange={e => setUsername(e.target.value)}
              className="w-full bg-gray-700 rounded px-3 py-2 text-white" required />
          </div>
          <div>
            <label className="block text-sm text-gray-400 mb-1">Password</label>
            <input type="password" value={password} onChange={e => setPassword(e.target.value)}
              className="w-full bg-gray-700 rounded px-3 py-2 text-white" required />
          </div>
          <button type="submit"
            className="w-full bg-blue-600 hover:bg-blue-500 text-white py-2 rounded transition-colors">
            Sign In
          </button>
        </form>
      </div>
    </div>
  )
}
