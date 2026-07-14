import { useState, type FormEvent } from 'react'
import { api, ApiError } from '../services/api'
import { ErrorBanner, Spinner } from '../components/ui'
import { formatDateTime, formatMoney } from '../lib/format'
import type { ReservationResponse } from '../types'

export function LookupPage() {
  const [code, setCode] = useState('')
  const [reservation, setReservation] = useState<ReservationResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [cancelling, setCancelling] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')

  const search = async (event: FormEvent) => {
    event.preventDefault()
    setLoading(true)
    setError('')
    setNotice('')
    setReservation(null)
    try {
      const found = await api.getReservation(code.trim().toUpperCase())
      setReservation(found)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao consultar a reserva.')
    } finally {
      setLoading(false)
    }
  }

  const cancel = async () => {
    if (!reservation || cancelling) return
    setCancelling(true)
    setError('')
    setNotice('')
    try {
      await api.cancelReservation(reservation.code)
      setReservation({ ...reservation, status: 'Cancelled' })
      setNotice('Reserva cancelada com sucesso.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao cancelar a reserva.')
    } finally {
      setCancelling(false)
    }
  }

  const active = reservation?.status === 'Active'

  return (
    <div>
      <h1 className="page-title">Consultar reserva</h1>
      <p className="page-subtitle">Digite o codigo gerado na compra (ex.: ABC-12345).</p>

      <div className="card">
        <form onSubmit={search} className="form-grid" aria-label="Consulta de reserva" style={{ gridTemplateColumns: '1fr auto' }}>
          <div className="field">
            <label htmlFor="codigo">Codigo da reserva</label>
            <input id="codigo" value={code} placeholder="ABC-12345" onChange={(e) => setCode(e.target.value)} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={loading || !code.trim()}>
            Consultar
          </button>
        </form>
      </div>

      <div style={{ marginTop: '1.25rem' }}>
        {loading ? <Spinner label="Consultando..." /> : null}
        {error ? <ErrorBanner message={error} /> : null}
        {notice ? (
          <div className="banner banner--ok" role="status">
            {notice}
          </div>
        ) : null}

        {reservation ? (
          <div className="card">
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
                <button type="button" className="btn btn-danger" onClick={cancel} disabled={cancelling}>
                  {cancelling ? 'Cancelando...' : 'Cancelar reserva'}
                </button>
              </div>
            ) : null}
          </div>
        ) : null}
      </div>
    </div>
  )
}
