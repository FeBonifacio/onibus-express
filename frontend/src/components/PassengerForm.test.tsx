import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { PassengerForm } from './PassengerForm'

async function fillValid(user: ReturnType<typeof userEvent.setup>, cpf: string) {
  await user.type(screen.getByLabelText('Nome completo'), 'Maria Silva')
  await user.type(screen.getByLabelText('CPF'), cpf)
  await user.type(screen.getByLabelText('E-mail'), 'maria@exemplo.com')
  await user.type(screen.getByLabelText('Data de nascimento'), '1990-05-20')
}

describe('PassengerForm', () => {
  it('shows errors and does not submit when empty', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<PassengerForm onSubmit={onSubmit} />)

    await user.click(screen.getByRole('button', { name: 'Confirmar compra' }))

    expect(await screen.findByText('CPF invalido.')).toBeInTheDocument()
    expect(screen.getByText('E-mail invalido.')).toBeInTheDocument()
    expect(screen.getByText('Informe o nome completo.')).toBeInTheDocument()
    expect(screen.getByText('Informe a data de nascimento.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('rejects a CPF with a wrong check digit (well-formed but invalid)', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<PassengerForm onSubmit={onSubmit} />)

    // valid length, not a repeated sequence, but the last check digit is wrong (should be 5)
    await fillValid(user, '529.982.247-24')
    await user.click(screen.getByRole('button', { name: 'Confirmar compra' }))

    expect(screen.getByText('CPF invalido.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('clears a field error once the user fixes it', async () => {
    const user = userEvent.setup()
    render(<PassengerForm onSubmit={vi.fn()} />)

    await user.click(screen.getByRole('button', { name: 'Confirmar compra' }))
    expect(screen.getByText('E-mail invalido.')).toBeInTheDocument()

    await user.type(screen.getByLabelText('E-mail'), 'ana@exemplo.com')
    expect(screen.queryByText('E-mail invalido.')).not.toBeInTheDocument()
  })

  it('submits valid data with a masked CPF', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<PassengerForm onSubmit={onSubmit} />)

    await fillValid(user, '52998224725')
    await user.click(screen.getByRole('button', { name: 'Confirmar compra' }))

    expect(onSubmit).toHaveBeenCalledTimes(1)
    expect(onSubmit.mock.calls[0][0]).toMatchObject({
      name: 'Maria Silva',
      document: '529.982.247-25',
      email: 'maria@exemplo.com',
      birthDate: '1990-05-20',
    })
  })
})
