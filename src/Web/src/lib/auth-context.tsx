import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useQueryClient } from "@tanstack/react-query"
import type { User } from "oidc-client-ts"

import { userManager } from "@/lib/session"

export interface AuthUser {
  id: string
  email: string
  name: string
}

interface AuthContextValue {
  user: AuthUser | null
  isLoading: boolean
  isAuthenticated: boolean
  /** Starts the Authorization Code + PKCE redirect to Keycloak. */
  login: () => Promise<void>
  /** RP-initiated logout (id_token_hint + post_logout_redirect_uri). */
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  isLoading: true,
  isAuthenticated: false,
  login: () => Promise.resolve(),
  logout: () => Promise.resolve()
})

function userFromToken(user: User): AuthUser | null {
  const sub = typeof user.profile.sub === "string" && user.profile.sub.length > 0 ? user.profile.sub : null

  if (!sub) {
    return null
  }

  const email = typeof user.profile.email === "string" ? user.profile.email : ""
  const name =
    (typeof user.profile.name === "string" && user.profile.name) ||
    (typeof user.profile.preferred_username === "string" && user.profile.preferred_username) ||
    email ||
    "You"

  return { id: sub ?? "", email, name }
}

/**
 * Keycloak owns authentication; the storefront only observes the oidc-client-ts
 * session (Authorization Code + PKCE). Credential entry never happens here.
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient()
  const [authUser, setAuthUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let cancelled = false

    const apply = (user: User | null) => {
      setAuthUser(user ? userFromToken(user) : null)
      if (!cancelled) {
        setIsLoading(false)
      }
    }

    void userManager.getUser().then(apply)

    const changed = () =>
      void userManager.getUser().then((user) => {
        apply(user)
        // A renewed token rotates roles/permissions server-side, not in the JWT,
        // but the *session* changed — refresh anything keyed to the user.
        void queryClient.invalidateQueries({ queryKey: ["my-permissions"] })
      })

    userManager.events.addUserLoaded(changed)
    userManager.events.addUserUnloaded(() => {
      apply(null)
      queryClient.clear()
    })
    // The silent renew failed (refresh token revoked/expired) — treat as logged out.
    userManager.events.addSilentRenewError(() => {
      void userManager.removeUser().then(() => {
        apply(null)
        queryClient.clear()
      })
    })

    return () => {
      cancelled = true
      userManager.events.removeUserLoaded(changed)
      userManager.events.removeUserUnloaded(changed)
      userManager.events.removeSilentRenewError(changed)
    }
  }, [queryClient])

  const login = useCallback(async () => {
    await userManager.signinRedirect()
  }, [])

  const logout = useCallback(async () => {
    await userManager.signoutRedirect()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: authUser,
      isLoading,
      isAuthenticated: authUser !== null,
      login,
      logout
    }),
    [authUser, isLoading, login, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuthContext() {
  return useContext(AuthContext)
}
