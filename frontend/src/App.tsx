import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Dashboard from './pages/Dashboard'
import ServerDetail from './pages/ServerDetail'

function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen">
        <header className="bg-gray-800 border-b border-gray-700 px-6 py-4">
          <h1 className="text-xl font-bold text-white">
            <a href="/" className="hover:text-blue-400 transition-colors">
              AWS Server Monitor
            </a>
          </h1>
        </header>
        <main className="p-6">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/instance/:instanceId" element={<ServerDetail />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}

export default App
