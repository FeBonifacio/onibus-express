import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { SeatMap } from '../components/SeatMap'
import { ErrorBanner, Spinner } from '../components/ui'
import { api } from '../services/api'
import { useBooking } from '../store/booking'
import { formatDateTime, formatMoney } from '../lib/format'
import type { TripDetail } from '../types'

export function SeatSelectionPage() {
  const { tripId = '' } = useParams()
  const navigate = useNavigate()
  const select = useBooking((state) => state.select)

  const [trip, setTrip] = useState<TripDetail | null>(null)
  const [seat, setSeat] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    setLoading(true)
    api
      .getTrip(tripId)
      .then((result) => {
        if (active) {
          setTrip(result)
          setLoading(false)
        }
      })
      .catch((err) => {
        if (active) {
          setError(err instanceof Error ? err.message : 'Erro ao carregar a viagem.')
          setLoading(false)
        }
      })
    return () => {
      active = false
    }
  }, [tripId])

  const proceed = () => {
    if (trip && seat !== null) {
      select(trip, seat)
      navigate(`/viagens/${tripId}/passageiro`)
    }
  }

  if (loading) return <Spinner label="Carregando viagem..." />
  if (error) return <ErrorBanner message={error} />
  if (!trip) return null

  return (
    <div>
      <h1 className="page-title">Escolha o assento</h1>
      <p className="page-subtitle">
        {trip.origin} → {trip.destination} · {formatDateTime(trip.departureUtc)} · {formatMoney(trip.basePrice)}
      </p>

      <div className="card">
        <SeatMap totalSeats={trip.totalSeats} takenSeats={trip.takenSeats} selectedSeat={seat} onSelect={setSeat} />
        <div className="actions">
          <button type="button" className="btn btn-ghost" onClick={() => navigate('/')}>
            Voltar
          </button>
          <button type="button" className="btn btn-primary" disabled={seat === null} onClick={proceed}>
            {seat === null ? 'Selecione um assento' : `Prosseguir com o assento ${seat}`}
          </button>
        </div>
      </div>
    </div>
  )
}
