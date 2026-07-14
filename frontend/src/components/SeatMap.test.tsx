import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { SeatMap } from './SeatMap'

describe('SeatMap', () => {
  it('selects a free seat', async () => {
    const user = userEvent.setup()
    const onSelect = vi.fn()
    render(<SeatMap totalSeats={6} takenSeats={[2, 4]} selectedSeat={null} onSelect={onSelect} />)

    await user.click(screen.getByRole('button', { name: 'Assento 5, livre' }))

    expect(onSelect).toHaveBeenCalledWith(5)
  })

  it('blocks occupied seats (disabled, no selection)', async () => {
    const user = userEvent.setup()
    const onSelect = vi.fn()
    render(<SeatMap totalSeats={6} takenSeats={[2, 4]} selectedSeat={null} onSelect={onSelect} />)

    const occupied = screen.getByRole('button', { name: 'Assento 2, ocupado' })
    expect(occupied).toBeDisabled()

    await user.click(occupied)
    expect(onSelect).not.toHaveBeenCalled()
  })

  it('marks the selected seat as pressed', () => {
    render(<SeatMap totalSeats={6} takenSeats={[]} selectedSeat={3} onSelect={vi.fn()} />)
    expect(screen.getByRole('button', { name: 'Assento 3, selecionado' })).toHaveAttribute('aria-pressed', 'true')
  })
})
