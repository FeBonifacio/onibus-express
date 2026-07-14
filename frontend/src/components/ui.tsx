import type { ReactNode } from 'react'

export function Spinner({ label = 'Carregando...' }: { label?: string }) {
  return (
    <div className="state" role="status">
      <div className="spinner" aria-hidden="true" />
      {label}
    </div>
  )
}

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div className="banner banner--error" role="alert">
      {message}
    </div>
  )
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <div className="state">{children}</div>
}

interface FieldProps {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  error?: string
  type?: string
  placeholder?: string
  inputMode?: 'text' | 'email' | 'numeric'
  maxLength?: number
}

export function Field({
  id,
  label,
  value,
  onChange,
  error,
  type = 'text',
  placeholder,
  inputMode,
  maxLength,
}: FieldProps) {
  const errorId = `${id}-error`
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        type={type}
        value={value}
        placeholder={placeholder}
        inputMode={inputMode}
        maxLength={maxLength}
        aria-invalid={error ? 'true' : undefined}
        aria-describedby={error ? errorId : undefined}
        onChange={(event) => onChange(event.target.value)}
      />
      {error ? (
        <span className="field-error" id={errorId} role="alert">
          {error}
        </span>
      ) : null}
    </div>
  )
}
