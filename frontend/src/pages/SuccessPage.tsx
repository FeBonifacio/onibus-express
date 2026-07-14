import { Link, useLocation } from 'react-router-dom'
import { EmptyState } from '../components/ui'
import { formatDateTime, formatMoney } from '../lib/format'
import type { ReservationResponse } from '../types'

export function SuccessPage() {
  const location = useLocation()
  const reservation = (location.state as { reservation?: ReservationResponse } | null)?.reservation

  if (!reservation) {
    return (
      <EmptyState>
        Nenhuma reserva para exibir. <Link to="/">Buscar passagens</Link>.
      </EmptyState>
    )
  }

  return (
    <div className="card" style={{ textAlign: 'center' }}>
      <div style={{ fontSize: '2.5rem' }} aria-hidden="true">
        🎉
      </div>
      <h1 className="page-title">Reserva confirmada!</h1>
      <p className="page-subtitle">Guarde o seu codigo de reserva:</p>
      <div className="code-badge">{reservation.code}</div>

      <div style={{ textAlign: 'left', maxWidth: 360, margin: '0 auto' }}>
        <div className="summary__row">
          <span>Passageiro</span>
          <strong>{reservation.passengerName}</strong>
        </div>
        <div className="summary__row">
          <span>CPF</span>
          <strong>{reservation.documentFormatted}</strong>
        </div>
        <div className="summary__row">
          <span>Assento</span>
          <strong>{reservation.seat}</strong>
        </div>
        <div className="summary__row">
          <span>Partida</span>
          <strong>{formatDateTime(reservation.departureUtc)}</strong>
        </div>
        <div className="summary__row summary__total">
          <span>Total</span>
          <span>{formatMoney(reservation.price)}</span>
        </div>
      </div>

      <div className="actions" style={{ justifyContent: 'center' }}>
        <Link to="/consulta" className="btn btn-ghost">
          Ver minha reserva
        </Link>
        <Link to="/" className="btn btn-primary">
          Nova busca
        </Link>
      </div>
    </div>
  )
}
