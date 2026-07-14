import { onlyDigits } from './validation'

/** Progressive CPF mask: 000.000.000-00. */
export function formatCpf(value: string): string {
  const d = onlyDigits(value).slice(0, 11)
  const parts = [d.slice(0, 3), d.slice(3, 6), d.slice(6, 9), d.slice(9, 11)]
  let out = parts[0]
  if (parts[1]) out += '.' + parts[1]
  if (parts[2]) out += '.' + parts[2]
  if (parts[3]) out += '-' + parts[3]
  return out
}

export function formatMoney(value: number): string {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

/** "06:00:00" -> "6h" or "6h30". */
export function formatDuration(timeSpan: string): string {
  const [h, m] = timeSpan.split(':')
  const hours = Number(h)
  const minutes = Number(m)
  return minutes > 0 ? `${hours}h${m}` : `${hours}h`
}
