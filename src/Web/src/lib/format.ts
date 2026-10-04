import { format, formatDistanceToNowStrict } from "date-fns"

export function formatMoney(amount: number, currency: string): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
    currencyDisplay: "narrowSymbol"
  }).format(amount)
}

export function formatEventDate(startAtUtc: string, endAtUtc?: string | null): string {
  const start = new Date(startAtUtc)

  if (!endAtUtc) {
    return format(start, "EEE, d MMM yyyy · HH:mm")
  }

  const end = new Date(endAtUtc)

  if (start.toDateString() === end.toDateString()) {
    return `${format(start, "EEE, d MMM yyyy · HH:mm")} — ${format(end, "HH:mm")}`
  }

  return `${format(start, "d MMM yyyy HH:mm")} — ${format(end, "d MMM yyyy HH:mm")}`
}

export function formatRelative(utc: string): string {
  return formatDistanceToNowStrict(new Date(utc), { addSuffix: true })
}

export function formatDateTime(utc: string): string {
  return format(new Date(utc), "d MMM yyyy · HH:mm")
}

export function formatDateTimeInput(utc: string): string {
  return format(new Date(utc), "yyyy-MM-dd'T'HH:mm")
}

export function normalizeDateTimeInput(value: string): string {
  return new Date(value).toISOString()
}
