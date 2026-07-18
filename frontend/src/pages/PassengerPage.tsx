import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { PassengerForm } from '../components/PassengerForm'
import { Summary } from '../components/Summary'
import { EmptyState, ErrorBanner } from '../components/ui'
import { api, ApiError } from '../services/api'
import { useBooking } from '../store/booking'
import { brDateToIso, onlyDigits } from '../lib/validation'
import { rememberReservation } from '../lib/myReservations'
import type { PassengerForm as PassengerData } from '../lib/validation'

export function PassengerPage() {
  const navigate = useNavigate()
  const { trip, seat, reset } = useBooking()
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const redirectTimer = useRef<number | undefined>(undefined)

  useEffect(() => {
    return () => {
      if (redirectTimer.current !== undefined) {
        window.clearTimeout(redirectTimer.current)
      }
    }
  }, [])

  if (!trip || seat === null) {
    return (
      <EmptyState>
        Sua selecao expirou. <Link to="/">Voltar para a busca</Link>.
      </EmptyState>
    )
  }

  const confirm = async (data: PassengerData) => {
    setSubmitting(true)
    setError('')
    try {
      const reservation = await api.createReservation({
        tripId: trip.id,
        name: data.name.trim(),
        document: onlyDigits(data.document),
        email: data.email.trim(),
        birthDate: brDateToIso(data.birthDate) ?? '',
        seat,
      })
      rememberReservation(reservation.code)
      reset()
      navigate('/reservas/sucesso', { state: { reservation } })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Nao foi possivel concluir a reserva.')
      setSubmitting(false)
      if (err instanceof ApiError && err.code === 'SEAT_TAKEN') {
        redirectTimer.current = window.setTimeout(() => navigate(`/viagens/${trip.id}/assentos`), 1800)
      }
    }
  }

  return (
    <div>
      <h1 className="page-title">Dados do passageiro</h1>
      <p className="page-subtitle">Confira o resumo e informe seus dados.</p>

      <Summary trip={trip} seat={seat} />

      <div className="card" style={{ marginTop: '1rem' }}>
        {error ? <ErrorBanner message={error} /> : null}
        <PassengerForm onSubmit={confirm} submitting={submitting} />
      </div>
    </div>
  )
}
