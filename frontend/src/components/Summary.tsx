import type { TripDetail } from '../types'
import { formatDateTime, formatMoney } from '../lib/format'

export function Summary({ trip, seat }: { trip: TripDetail; seat: number }) {
  return (
    <div className="card">
      <h2 className="page-title" style={{ fontSize: '1.15rem' }}>
        Resumo da compra
      </h2>
      <div className="summary__row">
        <span>Rota</span>
        <strong>
          {trip.origin} → {trip.destination}
        </strong>
      </div>
      <div className="summary__row">
        <span>Partida</span>
        <strong>{formatDateTime(trip.departureUtc)}</strong>
      </div>
      <div className="summary__row">
        <span>Assento</span>
        <strong>{seat}</strong>
      </div>
      <div className="summary__row summary__total">
        <span>Total</span>
        <span>{formatMoney(trip.basePrice)}</span>
      </div>
    </div>
  )
}
