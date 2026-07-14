// API contracts (mirror the backend DTOs, camelCase JSON).

export interface RouteDto {
  id: string
  origin: string
  destination: string
  estimatedDuration: string // TimeSpan, e.g. "06:00:00"
}

export interface TripSummary {
  id: string
  origin: string
  destination: string
  departureUtc: string
  basePrice: number
  totalSeats: number
  availableSeats: number
}

export interface TripDetail extends TripSummary {
  takenSeats: number[]
  freeSeats: number[]
}

export interface ReservationResponse {
  code: string
  tripId: string
  passengerName: string
  documentFormatted: string
  seat: number
  status: string
  departureUtc: string
  price: number
}

export interface CreateReservationRequest {
  tripId: string
  name: string
  document: string
  email: string
  birthDate: string // yyyy-MM-dd
  seat: number
}

export interface SearchTripsQuery {
  origem?: string
  destino?: string
  data?: string
}

export interface ProblemDetails {
  code?: string
  title?: string
  detail?: string
  status?: number
  traceId?: string
}
