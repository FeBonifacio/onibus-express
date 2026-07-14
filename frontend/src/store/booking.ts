import { create } from 'zustand'
import type { TripDetail } from '../types'

// Holds the in-progress booking selection across the flow (search -> seat -> passenger).
interface BookingState {
  trip: TripDetail | null
  seat: number | null
  select: (trip: TripDetail, seat: number) => void
  reset: () => void
}

export const useBooking = create<BookingState>((set) => ({
  trip: null,
  seat: null,
  select: (trip, seat) => set({ trip, seat }),
  reset: () => set({ trip: null, seat: null }),
}))
