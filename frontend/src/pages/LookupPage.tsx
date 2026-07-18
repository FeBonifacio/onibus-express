import { useEffect, useState, type FormEvent } from 'react'
import { api, ApiError } from '../services/api'
import { ErrorBanner, Spinner } from '../components/ui'
import { formatDateTime, formatMoney } from '../lib/format'
import { getMyReservationCodes, rememberReservation } from '../lib/myReservations'
import type { ReservationResponse } from '../types'

export function LookupPage() {
  const [reservations, setReservations] = useState<ReservationResponse[]>([])
  const [code, setCode] = useState('')
  const [loading, setLoading] = useState(true)
  const [searching, setSearching] = useState(false)
  const [cancellingCode, setCancellingCode] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  // Auto-load the reservations saved in this browser — appears without any manual search.
  useEffect(() => {
    let active = true
    const codes = getMyReservationCodes()
    if (codes.length === 0) {
      setLoading(false)
      return
    }
    Promise.allSettled(codes.map((c) => api.getReservation(c))).then((results) => {
      if (!active) return
      setReservations(
        results
          .filter((r): r is PromiseFulfilledResult<ReservationResponse> => r.status === 'fulfilled')
          .map((r) => r.value),
      )
      setLoading(false)
    })
    return () => {
      active = false
    }
  }, [])

  const upsert = (reservation: ReservationResponse) =>
    setReservations((list) => [reservation, ...list.filter((r) => r.code !== reservation.code)])

  const search = async (event: FormEvent) => {
    event.preventDefault()
    setSearching(true)
    setError('')
    setNotice('')
    try {
      const found = await api.getReservation(code.trim().toUpperCase())
      rememberReservation(found.code)
      upsert(found)
      setCode('')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao consultar a reserva.')
    } finally {
      setSearching(false)
    }
  }

  const cancel = async (reservation: ReservationResponse) => {
    if (cancellingCode) return
    setCancellingCode(reservation.code)
    setError('')
    setNotice('')
    try {
      await api.cancelReservation(reservation.code)
      upsert({ ...reservation, status: 'Cancelled' })
      setNotice(`Reserva ${reservation.code} cancelada com sucesso.`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao cancelar a reserva.')
    } finally {
      setCancellingCode(null)
    }
  }

  const hasReservations = reservations.length > 0

  return (
    <div>
      <h1 className="page-title">Minhas reservas</h1>
      <p className="page-subtitle">
        Suas reservas feitas neste navegador aparecem abaixo. Tambem da para consultar por codigo (ex.: ABC-12345).
      </p>

      <div className="card">
        <form onSubmit={search} className="form-grid" aria-label="Consulta de reserva" style={{ gridTemplateColumns: '1fr auto' }}>
          <div className="field">
            <label htmlFor="codigo">Codigo da reserva</label>
            <input id="codigo" value={code} placeholder="ABC-12345" onChange={(e) => setCode(e.target.value)} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={searching || !code.trim()}>
            Consultar
          </button>
        </form>
      </div>

      <div style={{ marginTop: '1.25rem' }}>
        {loading ? <Spinner label="Carregando suas reservas..." /> : null}
        {error ? <ErrorBanner message={error} /> : null}
        {notice ? (
          <div className="banner banner--ok" role="status">
            {notice}
          </div>
        ) : null}

        {hasReservations ? (
          <>
            <h2 className="page-title" style={{ fontSize: '1.15rem', marginTop: '0.5rem' }}>
              Suas reservas
            </h2>
            {reservations.map((reservation) => {
              const active = reservation.status === 'Active'
              return (
                <div className="card" key={reservation.code}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div className="code-badge" style={{ fontSize: '1.3rem', margin: 0 }}>
                      {reservation.code}
                    </div>
                    <span className={active ? 'badge badge--active' : 'badge badge--cancelled'}>
                      {active ? 'Ativa' : 'Cancelada'}
                    </span>
                  </div>
                  <div style={{ marginTop: '1rem' }}>
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
                  {active ? (
                    <div className="actions">
                      <button
                        type="button"
                        className="btn btn-danger"
                        onClick={() => cancel(reservation)}
                        disabled={cancellingCode === reservation.code}
                      >
                        {cancellingCode === reservation.code ? 'Cancelando...' : 'Cancelar reserva'}
                      </button>
                    </div>
                  ) : null}
                </div>
              )
            })}
          </>
        ) : null}

        {!loading && !hasReservations && !error ? (
          <p className="muted" style={{ marginTop: '0.5rem' }}>
            Voce ainda nao tem reservas neste navegador. Compre uma passagem ou consulte pelo codigo.
          </p>
        ) : null}
      </div>
    </div>
  )
}
