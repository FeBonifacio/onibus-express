import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { SearchPage } from './SearchPage'
import { api } from '../services/api'
import type { TripSummary } from '../types'

vi.mock('../services/api', () => ({
  api: { searchTrips: vi.fn() },
  ApiError: class ApiError extends Error {},
}))

const mockedApi = vi.mocked(api)

function renderPage() {
  return render(
    <MemoryRouter>
      <SearchPage />
    </MemoryRouter>,
  )
}

const sampleTrip: TripSummary = {
  id: 't1',
  origin: 'Sao Paulo',
  destination: 'Rio de Janeiro',
  departureUtc: '2030-01-01T12:00:00Z',
  basePrice: 120,
  totalSeats: 44,
  availableSeats: 40,
}

describe('SearchPage', () => {
  it('searches and lists the returned trips', async () => {
    const user = userEvent.setup()
    mockedApi.searchTrips.mockResolvedValue([sampleTrip])
    renderPage()

    await user.type(screen.getByLabelText('Origem'), 'Sao Paulo')
    await user.click(screen.getByRole('button', { name: 'Buscar' }))

    expect(await screen.findByText('Sao Paulo → Rio de Janeiro')).toBeInTheDocument()
    expect(mockedApi.searchTrips).toHaveBeenCalledWith({ origem: 'Sao Paulo', destino: undefined, data: undefined })
  })

  it('shows an empty state when there are no results', async () => {
    const user = userEvent.setup()
    mockedApi.searchTrips.mockResolvedValue([])
    renderPage()

    await user.click(screen.getByRole('button', { name: 'Buscar' }))

    expect(await screen.findByText(/Nenhuma viagem encontrada/)).toBeInTheDocument()
  })

  it('shows an error banner when the search fails', async () => {
    const user = userEvent.setup()
    mockedApi.searchTrips.mockRejectedValue(new Error('Falha na busca.'))
    renderPage()

    await user.click(screen.getByRole('button', { name: 'Buscar' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Falha na busca.')
  })
})
