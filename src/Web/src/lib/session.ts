import { UserManager, WebStorageStateStore } from "oidc-client-ts"

import { config } from "@/lib/config"

/**
 * Authentication speaks the Authorization Code Flow with PKCE (S256) — the only
 * browser-appropriate flow under OAuth 2.1 (RFC 7636) and the browser-app BCP
 * (RFC 10017). Keycloak owns all credential entry; this module never sees a
 * password, and tokens live in sessionStorage owned by oidc-client-ts.
 */
export class AuthError extends Error {
  readonly code: "invalid-credentials" | "unavailable" | "unknown"

  constructor(code: AuthError["code"], message: string) {
    super(message)
    this.name = "AuthError"
    this.code = code
  }
}

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

export const userManager = new UserManager({
  authority: config.oidc.authority,
  client_id: config.oidc.clientId,
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: "code",
  scope: "openid profile email",
  // Same storage surface the manual session layer used: sessionStorage, not
  // localStorage, so tokens die with the tab. (Phase 5 BFF removes browser
  // token exposure entirely.)
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  // Renewal uses the refresh-token grant (Keycloak rotates refresh tokens,
  // max reuse 0) — no iframe and no third-party-cookie dependency.
  automaticSilentRenew: true,
  accessTokenExpiringNotificationTimeInSeconds: 60
})

userManager.events.addUserLoaded(() => {
  // The silent renew produced a fresh access token; nothing else to do here.
})

userManager.events.addUserUnloaded(() => {
  notifyUnauthorized()
})

userManager.events.addSilentRenewError(() => {
  handleUnauthorizedResponse()
})

/**
 * Starts the Authorization Code + PKCE flow against Keycloak.
 * `from` is carried through the flow's state and honored by the callback route.
 */
export async function redirectToSignIn(from?: string): Promise<void> {
  await userManager.signinRedirect({ state: { from: from ?? "/" } })
}

/** Callback route handler: exchanges the code (verifier vs Keycloak's challenge). */
export async function completeSignIn(): Promise<{ from: string } | null> {
  const user = await userManager.signinRedirectCallback()
  const state = user.state as { from?: string } | undefined

  sessionStorage.removeItem(STEP_UP_ATTEMPTED_AT)

  return { from: state?.from ?? "/" }
}

/**
 * Returns a valid access token, renewing it when expired (refresh grant).
 * Returns null when there is no session.
 */
export async function getValidAccessToken(): Promise<string | null> {
  try {
    let user = await userManager.getUser()

    if (!user || user.expired || !user.access_token) {
      user = await userManager.signinSilent()
    }

    return user?.access_token ?? null
  } catch {
    handleUnauthorizedResponse()

    return null
  }
}

/** Clears the local session state without ending the Keycloak session. */
export async function clearSession(): Promise<void> {
  await userManager.removeUser()
}

/**
 * RP-initiated logout (OIDC): sends id_token_hint + post_logout_redirect_uri
 * so Keycloak ends the SSO session and returns the user to the storefront.
 */
export async function logoutSession(): Promise<void> {
  try {
    await userManager.signoutRedirect()
  } catch {
    // The SSO session may already be gone; the local state is cleared below.
    await clearSession()
    notifyUnauthorized()
  }
}

export function handleUnauthorizedResponse() {
  void clearSession().then(notifyUnauthorized)
}

/**
 * RFC 10005 step-up: sensitive endpoints answer insufficient LoA with
 * 401 + `WWW-Authenticate: Bearer error="insufficient_user_authentication"`.
 */
const STEP_UP_CHALLENGE = /error="?insufficient_user_authentication"?/i
const STEP_UP_ATTEMPTED_AT = "evently:step-up-attempted-at"
const STEP_UP_AC_VALUES = "2"
const STEP_UP_ATTEMPT_WINDOW_MS = 60_000

/** True when the response is a step-up challenge rather than a plain 401. */
export function isStepUpChallenge(response: Response): boolean {
  const authHeader = response.headers.get("www-authenticate") ?? ""

  return STEP_UP_CHALLENGE.test(authHeader)
}

/**
 * Re-runs the authorization request with `acr_values=2` so Keycloak forces a
 * second-factor step (TOTP / passkey) before the operation. Redirects the
 * browser; only attempts once per window to avoid challenge loops when the
 * IdP cannot satisfy the requested LoA.
 */
export async function requestStepUpAuthentication(): Promise<void> {
  const attemptedAt = sessionStorage.getItem(STEP_UP_ATTEMPTED_AT)
  if (attemptedAt && Date.now() - Number(attemptedAt) < STEP_UP_ATTEMPT_WINDOW_MS) {
    handleUnauthorizedResponse()

    return
  }

  sessionStorage.setItem(STEP_UP_ATTEMPTED_AT, String(Date.now()))

  // Re-runs the authorization request; Keycloak's LoA condition escalates the
  // session (TOTP passkey step) before returning a token with the higher `acr`.
  await userManager.signinRedirect({
    state: { from: window.location.pathname },
    extraQueryParams: { acr_values: STEP_UP_AC_VALUES }
  })
}


export function decodeJwtPayload(token: string): Record<string, unknown> {
  const payload = token.split(".")[1]
  const normalized = payload.replace(/-/g, "+").replace(/_/g, "/")
  const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), "=")
  const bytes = Uint8Array.from(atob(padded), (c) => c.charCodeAt(0))
  const json = new TextDecoder("utf-8").decode(bytes)

  return JSON.parse(json) as Record<string, unknown>
}
