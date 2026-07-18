import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { LookupPage } from './LookupPage'
import { api } from '../services/api'
import type { ReservationResponse } from '../types'

vi.mock('../services/api', () => ({
  api: { getReservation: vi.fn(), cancelReservation: vi.fn() },
  ApiError: class ApiError extends Error {},
}))

const mockedApi = vi.mocked(api)

const reservation: ReservationResponse = {
  code: 'ABC-12345',
  tripId: 't1',
  passengerName: 'Maria Silva',
  documentFormatted: '529.982.247-25',
  seat: 10,
  status: 'Active',
  departureUtc: '2030-01-01T12:00:00Z',
  price: 120,
}

describe('LookupPage', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('auto-loads reservations saved in this browser, without searching', async () => {
    localStorage.setItem('onibus.reservations', JSON.stringify(['ABC-12345']))
    mockedApi.getReservation.mockResolvedValue(reservation)

    render(<LookupPage />)

    expect(await screen.findByText('Maria Silva')).toBeInTheDocument()
    expect(screen.getByText('Suas reservas')).toBeInTheDocument()
    expect(mockedApi.getReservation).toHaveBeenCalledWith('ABC-12345')
  })

  it('shows no reservations section when the browser has none saved', async () => {
    render(<LookupPage />)

    await waitFor(() => expect(mockedApi.getReservation).not.toHaveBeenCalled())
    expect(screen.queryByText('Suas reservas')).not.toBeInTheDocument()
  })

  it('looks up a reservation by code (uppercased) and lists it', async () => {
    const user = userEvent.setup()
    mockedApi.getReservation.mockResolvedValue(reservation)

    render(<LookupPage />)

    await user.type(screen.getByLabelText('Codigo da reserva'), 'abc-12345')
    await user.click(screen.getByRole('button', { name: 'Consultar' }))

    expect(await screen.findByText('Maria Silva')).toBeInTheDocument()
    expect(mockedApi.getReservation).toHaveBeenCalledWith('ABC-12345')
  })

  it('cancels an active reservation', async () => {
    const user = userEvent.setup()
    localStorage.setItem('onibus.reservations', JSON.stringify(['ABC-12345']))
    mockedApi.getReservation.mockResolvedValue(reservation)
    mockedApi.cancelReservation.mockResolvedValue(undefined)

    render(<LookupPage />)
    await screen.findByText('Maria Silva')

    await user.click(screen.getByRole('button', { name: 'Cancelar reserva' }))

    expect(mockedApi.cancelReservation).toHaveBeenCalledWith('ABC-12345')
    expect(await screen.findByText('Cancelada')).toBeInTheDocument()
  })
})
