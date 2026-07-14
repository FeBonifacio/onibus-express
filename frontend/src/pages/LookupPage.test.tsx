import { render, screen } from '@testing-library/react'
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
  it('looks up a reservation by code (uppercased)', async () => {
    const user = userEvent.setup()
    mockedApi.getReservation.mockResolvedValue(reservation)
    render(<LookupPage />)

    await user.type(screen.getByLabelText('Codigo da reserva'), 'abc-12345')
    await user.click(screen.getByRole('button', { name: 'Consultar' }))

    expect(await screen.findByText('Maria Silva')).toBeInTheDocument()
    expect(mockedApi.getReservation).toHaveBeenCalledWith('ABC-12345')
    expect(screen.getByText('Ativa')).toBeInTheDocument()
  })

  it('cancels an active reservation', async () => {
    const user = userEvent.setup()
    mockedApi.getReservation.mockResolvedValue(reservation)
    mockedApi.cancelReservation.mockResolvedValue(undefined)
    render(<LookupPage />)

    await user.type(screen.getByLabelText('Codigo da reserva'), 'ABC-12345')
    await user.click(screen.getByRole('button', { name: 'Consultar' }))
    await screen.findByText('Maria Silva')

    await user.click(screen.getByRole('button', { name: 'Cancelar reserva' }))

    expect(mockedApi.cancelReservation).toHaveBeenCalledWith('ABC-12345')
    expect(await screen.findByText('Cancelada')).toBeInTheDocument()
    expect(screen.getByText('Reserva cancelada com sucesso.')).toBeInTheDocument()
  })
})
