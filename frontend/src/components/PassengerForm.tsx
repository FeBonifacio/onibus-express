import { useState, type FormEvent } from 'react'
import { Field } from './ui'
import { formatCpf } from '../lib/format'
import { validatePassenger, type PassengerErrors, type PassengerForm as PassengerData } from '../lib/validation'

interface PassengerFormProps {
  onSubmit: (data: PassengerData) => void
  submitting?: boolean
}

export function PassengerForm({ onSubmit, submitting }: PassengerFormProps) {
  const [form, setForm] = useState<PassengerData>({ name: '', document: '', email: '', birthDate: '' })
  const [errors, setErrors] = useState<PassengerErrors>({})

  const set = (field: keyof PassengerData) => (value: string) => {
    setForm((current) => ({ ...current, [field]: value }))
    setErrors((current) => (current[field] ? { ...current, [field]: undefined } : current))
  }

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const found = validatePassenger(form)
    setErrors(found)
    if (Object.keys(found).length === 0) {
      onSubmit(form)
    }
  }

  return (
    <form onSubmit={submit} aria-label="Dados do passageiro" noValidate>
      <Field id="name" label="Nome completo" value={form.name} onChange={set('name')} error={errors.name} />
      <Field
        id="document"
        label="CPF"
        value={form.document}
        onChange={(value) => set('document')(formatCpf(value))}
        error={errors.document}
        inputMode="numeric"
        placeholder="000.000.000-00"
        maxLength={14}
      />
      <Field
        id="email"
        label="E-mail"
        type="email"
        inputMode="email"
        value={form.email}
        onChange={set('email')}
        error={errors.email}
        placeholder="voce@exemplo.com"
      />
      <Field
        id="birthDate"
        label="Data de nascimento"
        type="date"
        value={form.birthDate}
        onChange={set('birthDate')}
        error={errors.birthDate}
      />
      <button type="submit" className="btn btn-primary btn-block" disabled={submitting}>
        {submitting ? 'Confirmando...' : 'Confirmar compra'}
      </button>
    </form>
  )
}
