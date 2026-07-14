import type { TripSummary } from '../types'
import { formatDateTime, formatMoney } from '../lib/format'

interface TripCardProps {
  trip: TripSummary
  onSelect: (trip: TripSummary) => void
}

export function TripCard({ trip, onSelect }: TripCardProps) {
  const soldOut = trip.availableSeats === 0
  const vacancyClass = soldOut
    ? 'vacancies vacancies--none'
    : trip.availableSeats <= 5
      ? 'vacancies vacancies--low'
      : 'vacancies'

  return (
    <div className="card trip-card">
      <div>
        <div className="trip-card__route">
          {trip.origin} → {trip.destination}
        </div>
        <div className="trip-card__meta">
          <span>🗓 {formatDateTime(trip.departureUtc)}</span>
          <span className={vacancyClass}>
            {soldOut ? 'Esgotado' : `${trip.availableSeats} vaga(s)`}
          </span>
        </div>
      </div>
      <div className="trip-card__aside">
        <div className="trip-card__price">{formatMoney(trip.basePrice)}</div>
        <button type="button" className="btn btn-primary" disabled={soldOut} onClick={() => onSelect(trip)}>
          Selecionar
        </button>
      </div>
    </div>
  )
}
