import { config } from "@/lib/config"

export class AuthError extends Error {
  readonly code: "invalid-credentials" | "unavailable" | "unknown"

  constructor(code: AuthError["code"], message: string) {
    super(message)
    this.name = "AuthError"
    this.code = code
  }
}

interface StoredSession {
  accessToken: string
  refreshToken: string | null
  idToken: string | null
  expiresAt: number
}

interface TokenResponse {
  access_token: string
  refresh_token?: string
  id_token?: string
  expires_in: number
}

const STORAGE_KEY = "evently-session"
const REFRESH_SKEW_MS = 60_000

type UnauthorizedListener = () => void
const unauthorizedListeners = new Set<UnauthorizedListener>()

export function onUnauthorized(listener: UnauthorizedListener): () => void {
  unauthorizedListeners.add(listener)

  return () => {
    unauthorizedListeners.delete(listener)
  }
}

function notifyUnauthorized() {
  unauthorizedListeners.forEach((listener) => listener())
}

function tokenEndpoint(): string {
  return `${config.oidc.authority}/protocol/openid-connect/token`
}

function loadSession(): StoredSession | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    if (!raw) {
      return null
    }

    return JSON.parse(raw) as StoredSession
  } catch {
    return null
  }
}

function saveSession(session: StoredSession) {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession() {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function decodeJwtPayload(token: string): Record<string, unknown> {
  const payload = token.split(".")[1]
  const normalized = payload.replace(/-/g, "+").replace(/_/g, "/")
  const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), "=")

  return JSON.parse(atob(padded)) as Record<string, unknown>
}

function toStoredSession(response: TokenResponse): StoredSession {
  return {
    accessToken: response.access_token,
    refreshToken: response.refresh_token ?? null,
    idToken: response.id_token ?? null,
    expiresAt: Date.now() + response.expires_in * 1000
  }
}

async function requestTokens(params: Record<string, string>): Promise<TokenResponse> {
  let response: Response
  try {
    response = await fetch(tokenEndpoint(), {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({ client_id: config.oidc.clientId, ...params })
    })
  } catch {
    throw new AuthError("unavailable", "The identity provider is unreachable. Please try again.")
  }

  if (response.status === 400 || response.status === 401) {
    throw new AuthError("invalid-credentials", "Invalid email or password.")
  }

  if (!response.ok) {
    throw new AuthError("unknown", "Sign-in failed. Please try again.")
  }

  return (await response.json()) as TokenResponse
}

/**
 * Signs in with an email + password against Keycloak (direct access grant),
 * keeping the branded in-app login page while Keycloak stays the identity provider.
 */
export async function loginWithPassword(email: string, password: string): Promise<StoredSession> {
  const tokens = await requestTokens({
    grant_type: "password",
    username: email.trim(),
    password,
    scope: "openid profile email"
  })

  const session = toStoredSession(tokens)
  saveSession(session)

  return session
}

let refreshInFlight: Promise<StoredSession> | null = null

async function refreshSession(session: StoredSession): Promise<StoredSession> {
  if (!session.refreshToken) {
    throw new AuthError("unknown", "Session expired. Please sign in again.")
  }

  if (!refreshInFlight) {
    refreshInFlight = requestTokens({
      grant_type: "refresh_token",
      refresh_token: session.refreshToken
    })
      .then((tokens) => {
        const next: StoredSession = {
          ...toStoredSession(tokens),
          refreshToken: tokens.refresh_token ?? session.refreshToken
        }
        saveSession(next)

        return next
      })
      .finally(() => {
        refreshInFlight = null
      })
  }

  return refreshInFlight
}

/**
 * Returns a valid access token, refreshing it when it is expired or close to it.
 * Returns null when there is no session.
 */
export async function getValidAccessToken(): Promise<string | null> {
  const session = loadSession()
  if (!session) {
    return null
  }

  if (session.expiresAt - Date.now() > REFRESH_SKEW_MS) {
    return session.accessToken
  }

  try {
    const refreshed = await refreshSession(session)

    return refreshed.accessToken
  } catch {
    clearSession()
    notifyUnauthorized()

    return null
  }
}

export function getStoredSession(): StoredSession | null {
  return loadSession()
}

/**
 * Ends the Keycloak session (best effort) and clears the local session.
 */
export async function logoutSession(): Promise<void> {
  const session = loadSession()
  clearSession()

  if (session?.idToken) {
    const logoutUrl =
      `${config.oidc.authority}/protocol/openid-connect/logout` +
      `?post_logout_redirect_uri=${encodeURIComponent(window.location.origin)}` +
      `&id_token_hint=${encodeURIComponent(session.idToken)}`

    try {
      await fetch(logoutUrl, { mode: "no-cors" })
    } catch {
      // Logout is best effort — the local session is already cleared.
    }
  }
}

export function handleUnauthorizedResponse() {
  clearSession()
  notifyUnauthorized()
}
