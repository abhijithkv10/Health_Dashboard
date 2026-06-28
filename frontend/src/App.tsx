import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import { useAuth } from './contexts/AuthContext'
import Dashboard from './pages/Dashboard'
import ServerDetail from './pages/ServerDetail'
import Login from './pages/Login'
import Admin from './pages/Admin'
import ProtectedRoute from './components/ProtectedRoute'

function App() {
  const { isAuthenticated } = useAuth()

  return (
    <BrowserRouter>
      <div className="min-h-screen">
        <header className="bg-gray-800 border-b border-gray-700 px-6 py-4 flex items-center justify-between">
          <h1 className="text-xl font-bold text-white">
            <Link to="/" className="hover:text-blue-400 transition-colors">
              AWS Server Monitor
            </Link>
          </h1>
          <nav className="flex items-center gap-4 text-sm">
            {isAuthenticated && (
              <Link to="/admin" className="text-gray-400 hover:text-white transition-colors">
                Admin
              </Link>
            )}
            {!isAuthenticated && (
              <Link to="/login" className="text-gray-400 hover:text-white transition-colors">
                Sign In
              </Link>
            )}
          </nav>
        </header>
        <main className="p-6">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/instance/:instanceId" element={<ServerDetail />} />
            <Route path="/login" element={<Login />} />
            <Route path="/admin" element={<ProtectedRoute><Admin /></ProtectedRoute>} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}

export default App
