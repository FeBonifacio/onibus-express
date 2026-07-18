import { useState, type FormEvent } from 'react'
import { formatDateBr } from '../lib/format'
import { brDateToIso } from '../lib/validation'
import type { SearchTripsQuery } from '../types'

interface SearchFormProps {
  onSearch: (query: SearchTripsQuery) => void
  loading?: boolean
}

export function SearchForm({ onSearch, loading }: SearchFormProps) {
  const [origem, setOrigem] = useState('')
  const [destino, setDestino] = useState('')
  const [data, setData] = useState('')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    onSearch({
      origem: origem.trim() || undefined,
      destino: destino.trim() || undefined,
      data: brDateToIso(data) ?? undefined,
    })
  }

  return (
    <form className="form-grid" onSubmit={submit} aria-label="Busca de passagens">
      <div className="field">
        <label htmlFor="origem">Origem</label>
        <input id="origem" value={origem} placeholder="Sao Paulo" onChange={(e) => setOrigem(e.target.value)} />
      </div>
      <div className="field">
        <label htmlFor="destino">Destino</label>
        <input id="destino" value={destino} placeholder="Rio de Janeiro" onChange={(e) => setDestino(e.target.value)} />
      </div>
      <div className="field">
        <label htmlFor="data">Data de ida</label>
        <input
          id="data"
          value={data}
          placeholder="dd/mm/aaaa"
          inputMode="numeric"
          maxLength={10}
          onChange={(e) => setData(formatDateBr(e.target.value))}
        />
      </div>
      <button type="submit" className="btn btn-primary" disabled={loading}>
        {loading ? 'Buscando...' : 'Buscar'}
      </button>
    </form>
  )
}
