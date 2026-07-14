import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { SearchForm } from '../components/SearchForm'
import { TripCard } from '../components/TripCard'
import { EmptyState, ErrorBanner, Spinner } from '../components/ui'
import { api } from '../services/api'
import type { SearchTripsQuery, TripSummary } from '../types'

type Status = 'idle' | 'loading' | 'done' | 'error'

export function SearchPage() {
  const navigate = useNavigate()
  const [trips, setTrips] = useState<TripSummary[]>([])
  const [status, setStatus] = useState<Status>('idle')
  const [error, setError] = useState('')

  const search = async (query: SearchTripsQuery) => {
    setStatus('loading')
    setError('')
    setTrips([])
    try {
      const result = await api.searchTrips(query)
      setTrips(result)
      setStatus('done')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao buscar viagens.')
      setStatus('error')
    }
  }

  return (
    <div>
      <h1 className="page-title">Buscar passagens</h1>
      <p className="page-subtitle">Encontre a viagem ideal e reserve seu assento.</p>

      <div className="card">
        <SearchForm onSearch={search} loading={status === 'loading'} />
      </div>

      <div style={{ marginTop: '1.25rem' }}>
        {status === 'loading' ? <Spinner label="Buscando viagens..." /> : null}
        {status === 'error' ? <ErrorBanner message={error} /> : null}
        {status === 'done' && trips.length === 0 ? (
          <EmptyState>Nenhuma viagem encontrada para esses filtros.</EmptyState>
        ) : null}
        {status === 'done'
          ? trips.map((trip) => (
              <TripCard key={trip.id} trip={trip} onSelect={(selected) => navigate(`/viagens/${selected.id}/assentos`)} />
            ))
          : null}
      </div>
    </div>
  )
}
