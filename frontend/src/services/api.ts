import type {
  CreateReservationRequest,
  ProblemDetails,
  ReservationResponse,
  RouteDto,
  SearchTripsQuery,
  TripDetail,
  TripSummary,
} from '../types'

// Base URL: VITE_API_URL if set, otherwise the same-origin `/api` proxy (dev: Vite, prod: Nginx).
const BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? '/api'

/** Carries the backend's stable error `code` and user-facing (Portuguese) message. */
export class ApiError extends Error {
  readonly code: string
  readonly status: number

  constructor(status: number, code: string, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${BASE}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...init,
    })
  } catch {
    throw new ApiError(0, 'NETWORK', 'Nao foi possivel conectar ao servidor.')
  }

  if (!response.ok) {
    let code = 'ERRO'
    let message = `Erro ${response.status}.`
    try {
      const problem = (await response.json()) as ProblemDetails
      code = problem.code ?? code
      message = problem.detail ?? problem.title ?? message
    } catch {
      // response had no JSON body
    }
    throw new ApiError(response.status, code, message)
  }

  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}

export const api = {
  listRoutes: () => request<RouteDto[]>('/rotas'),

  searchTrips: (query: SearchTripsQuery) => {
    const params = new URLSearchParams()
    if (query.origem) params.set('origem', query.origem)
    if (query.destino) params.set('destino', query.destino)
    if (query.data) params.set('data', query.data)
    const qs = params.toString()
    return request<TripSummary[]>(`/viagens${qs ? `?${qs}` : ''}`)
  },

  getTrip: (id: string) => request<TripDetail>(`/viagens/${id}`),

  createReservation: (body: CreateReservationRequest) =>
    request<ReservationResponse>('/reservas', { method: 'POST', body: JSON.stringify(body) }),

  getReservation: (code: string) => request<ReservationResponse>(`/reservas/${encodeURIComponent(code)}`),

  cancelReservation: (code: string) =>
    request<void>(`/reservas/${encodeURIComponent(code)}`, { method: 'DELETE' }),
}
