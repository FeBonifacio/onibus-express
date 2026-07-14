// Client-side validation mirroring the backend rules (feedback to the user is in Portuguese).

export function onlyDigits(value: string): string {
  return (value ?? '').replace(/\D/g, '')
}

/** CPF check-digit validation (module 11), same algorithm as the backend Document VO. */
export function isValidCpf(value: string): boolean {
  const digits = onlyDigits(value)
  if (digits.length !== 11) return false
  if (/^(\d)\1{10}$/.test(digits)) return false // repeated sequence

  const checkDigit = (count: number): number => {
    let sum = 0
    for (let i = 0; i < count; i++) {
      sum += Number(digits[i]) * (count + 1 - i)
    }
    const rest = sum % 11
    return rest < 2 ? 0 : 11 - rest
  }

  return checkDigit(9) === Number(digits[9]) && checkDigit(10) === Number(digits[10])
}

export function isValidEmail(value: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test((value ?? '').trim())
}

export interface PassengerForm {
  name: string
  document: string
  email: string
  birthDate: string
}

export type PassengerErrors = Partial<Record<keyof PassengerForm, string>>

/** Returns a map of field -> error message (empty map = valid). */
export function validatePassenger(form: PassengerForm): PassengerErrors {
  const errors: PassengerErrors = {}
  if (form.name.trim().length < 3) {
    errors.name = 'Informe o nome completo.'
  }
  if (!isValidCpf(form.document)) {
    errors.document = 'CPF invalido.'
  }
  if (!isValidEmail(form.email)) {
    errors.email = 'E-mail invalido.'
  }
  if (!form.birthDate) {
    errors.birthDate = 'Informe a data de nascimento.'
  } else if (new Date(form.birthDate) > new Date()) {
    errors.birthDate = 'Data de nascimento nao pode ser futura.'
  }
  return errors
}
