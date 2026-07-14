interface SeatMapProps {
  totalSeats: number
  takenSeats: number[]
  selectedSeat: number | null
  onSelect: (seat: number) => void
}

export function SeatMap({ totalSeats, takenSeats, selectedSeat, onSelect }: SeatMapProps) {
  const taken = new Set(takenSeats)
  const seats = Array.from({ length: totalSeats }, (_, index) => index + 1)

  return (
    <div>
      <div className="seat-legend">
        <span className="seat-legend__item">
          <span className="seat-legend__swatch" style={{ background: 'var(--free)', borderColor: 'var(--free-border)' }} />
          Livre
        </span>
        <span className="seat-legend__item">
          <span className="seat-legend__swatch" style={{ background: 'var(--taken)' }} />
          Ocupado
        </span>
        <span className="seat-legend__item">
          <span className="seat-legend__swatch" style={{ background: 'var(--selected)' }} />
          Selecionado
        </span>
      </div>

      <div className="seat-map" role="group" aria-label="Mapa de assentos">
        {seats.map((seat) => {
          const isTaken = taken.has(seat)
          const isSelected = seat === selectedSeat
          const status = isTaken ? 'ocupado' : isSelected ? 'selecionado' : 'livre'
          const className = isTaken
            ? 'seat seat--taken'
            : isSelected
              ? 'seat seat--selected'
              : 'seat seat--free'

          return (
            <button
              key={seat}
              type="button"
              className={className}
              disabled={isTaken}
              aria-pressed={isSelected}
              aria-label={`Assento ${seat}, ${status}`}
              onClick={() => onSelect(seat)}
            >
              {seat}
            </button>
          )
        })}
      </div>
    </div>
  )
}
