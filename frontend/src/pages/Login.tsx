import { GoogleLogin, type CredentialResponse } from '@react-oauth/google'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { googleLogin } from '../api'
import { useState } from 'react'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')

  const onSuccess = async (response: CredentialResponse) => {
    try {
      setError('')
      const data = await googleLogin(response.credential!)
      login(data.token, { email: data.email, name: data.name, picture: data.picture })
      navigate('/admin')
    } catch {
      setError('Authentication failed. Make sure you use an allowed email domain.')
    }
  }

  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="bg-gray-800 rounded-lg p-8 shadow-lg text-center max-w-sm w-full">
        <h2 className="text-xl font-bold text-white mb-2">Admin Sign In</h2>
        <p className="text-gray-400 mb-6">Sign in with your Google account</p>
        {error && <p className="text-red-400 mb-4 text-sm">{error}</p>}
        <div className="flex justify-center">
          <GoogleLogin
            onSuccess={onSuccess}
            onError={() => setError('Google sign-in failed. Try again.')}
            theme="filled_black"
            shape="pill"
          />
        </div>
      </div>
    </div>
  )
}
