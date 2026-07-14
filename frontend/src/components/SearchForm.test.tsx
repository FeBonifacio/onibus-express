import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { SearchForm } from './SearchForm'

describe('SearchForm', () => {
  it('submits the typed origin and destination', async () => {
    const user = userEvent.setup()
    const onSearch = vi.fn()
    render(<SearchForm onSearch={onSearch} />)

    await user.type(screen.getByLabelText('Origem'), 'Sao Paulo')
    await user.type(screen.getByLabelText('Destino'), 'Rio de Janeiro')
    await user.click(screen.getByRole('button', { name: 'Buscar' }))

    expect(onSearch).toHaveBeenCalledWith({
      origem: 'Sao Paulo',
      destino: 'Rio de Janeiro',
      data: undefined,
    })
  })

  it('disables the button while loading', () => {
    render(<SearchForm onSearch={vi.fn()} loading />)
    expect(screen.getByRole('button', { name: 'Buscando...' })).toBeDisabled()
  })
})
