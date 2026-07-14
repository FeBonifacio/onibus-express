import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { Layout } from './components/Layout'
import { SearchPage } from './pages/SearchPage'
import { SeatSelectionPage } from './pages/SeatSelectionPage'
import { PassengerPage } from './pages/PassengerPage'
import { SuccessPage } from './pages/SuccessPage'
import { LookupPage } from './pages/LookupPage'

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true, element: <SearchPage /> },
      { path: 'viagens/:tripId/assentos', element: <SeatSelectionPage /> },
      { path: 'viagens/:tripId/passageiro', element: <PassengerPage /> },
      { path: 'reservas/sucesso', element: <SuccessPage /> },
      { path: 'consulta', element: <LookupPage /> },
    ],
  },
])

export function App() {
  return <RouterProvider router={router} />
}
