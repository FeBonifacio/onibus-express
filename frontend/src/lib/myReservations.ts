// Remembers the reservation codes created/looked up in THIS browser (localStorage),
// so the lookup screen can show them automatically without a manual search.

const KEY = 'onibus.reservations'
const MAX = 20

export function getMyReservationCodes(): string[] {
  try {
    const raw = localStorage.getItem(KEY)
    if (!raw) return []
    const parsed: unknown = JSON.parse(raw)
    return Array.isArray(parsed) ? parsed.filter((c): c is string => typeof c === 'string') : []
  } catch {
    return []
  }
}

export function rememberReservation(code: string): void {
  try {
    const next = [code, ...getMyReservationCodes().filter((c) => c !== code)].slice(0, MAX)
    localStorage.setItem(KEY, JSON.stringify(next))
  } catch {
    // storage unavailable (private mode / disabled) — degrade silently
  }
}
