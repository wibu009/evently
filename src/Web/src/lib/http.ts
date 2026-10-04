import { getValidAccessToken, handleUnauthorizedResponse } from "@/lib/session"
import { config } from "@/lib/config"

export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly detail: string
  readonly errors: Record<string, string[]>

  constructor(status: number, title: string, detail: string, errors?: Record<string, string[]>) {
    super(detail || title)
    this.name = "ApiError"
    this.status = status
    this.title = title
    this.detail = detail
    this.errors = errors ?? {}
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE"
  body?: unknown
  signal?: AbortSignal
}

async function request<TResponse>(baseUrl: string, path: string, options: RequestOptions = {}): Promise<TResponse> {
  const accessToken = await getValidAccessToken()

  const headers: Record<string, string> = {
    Accept: "application/json"
  }

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json"
  }

  if (accessToken) {
    headers["Authorization"] = `Bearer ${accessToken}`
  }

  const response = await fetch(`${baseUrl}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    signal: options.signal
  })

  if (response.status === 401) {
    handleUnauthorizedResponse()
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  if (!response.ok) {
    let problem: ProblemDetails = {}
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Non-JSON error body (e.g. raw gateway errors)
    }

    throw new ApiError(
      response.status,
      problem.title ?? response.statusText,
      problem.detail ?? problem.title ?? response.statusText,
      problem.errors
    )
  }

  return (await response.json()) as TResponse
}

export const api = {
  /** Core API host (Events, Users, Attendance modules). */
  get: <T>(path: string, signal?: AbortSignal) => request<T>(config.apiBaseUrl, path, { method: "GET", signal }),
  post: <T>(path: string, body?: unknown) => request<T>(config.apiBaseUrl, path, { method: "POST", body }),
  put: <T>(path: string, body?: unknown) => request<T>(config.apiBaseUrl, path, { method: "PUT", body }),
  delete: <T>(path: string) => request<T>(config.apiBaseUrl, path, { method: "DELETE" }),

  /** Ticketing API host (Carts, Orders, Payments, Tickets, Waiting List, Promo Codes). */
  ticketing: {
    get: <T>(path: string, signal?: AbortSignal) =>
      request<T>(config.ticketingBaseUrl, path, { method: "GET", signal }),
    post: <T>(path: string, body?: unknown) => request<T>(config.ticketingBaseUrl, path, { method: "POST", body }),
    put: <T>(path: string, body?: unknown) => request<T>(config.ticketingBaseUrl, path, { method: "PUT", body }),
    delete: <T>(path: string) => request<T>(config.ticketingBaseUrl, path, { method: "DELETE" })
  }
}
