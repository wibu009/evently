import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react"
import { useQueryClient } from "@tanstack/react-query"

import {
  clearSession,
  decodeJwtPayload,
  getValidAccessToken,
  loginWithPassword,
  logoutSession,
  onUnauthorized
} from "@/lib/session"

export interface AuthUser {
  id: string
  email: string
  name: string
}

interface AuthContextValue {
  user: AuthUser | null
  isLoading: boolean
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  isLoading: true,
  isAuthenticated: false,
  login: () => Promise.resolve(),
  logout: () => Promise.resolve()
})

function userFromToken(accessToken: string): AuthUser | null {
  try {
    const claims = decodeJwtPayload(accessToken)

    const sub = claims["sub"]
    if (typeof sub !== "string" || sub.length === 0) {
      return null
    }

    const email = typeof claims["email"] === "string" ? claims["email"] : ""
    const name =
      (typeof claims["name"] === "string" && claims["name"]) ||
      (typeof claims["preferred_username"] === "string" && claims["preferred_username"]) ||
      email ||
      "You"

    return { id: sub, email, name }
  } catch {
    return null
  }
}

/**
 * Branded in-app authentication. Keycloak remains the identity provider under the hood
 * (password + refresh grants against the public client), but users never leave the app.
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient()
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const refreshTimer = useRef<number | null>(null)

  const clearRefreshTimer = () => {
    if (refreshTimer.current !== null) {
      window.clearTimeout(refreshTimer.current)
      refreshTimer.current = null
    }
  }

  const applySession = useCallback((accessToken: string | null) => {
    clearRefreshTimer()
    setUser(accessToken ? userFromToken(accessToken) : null)

    if (accessToken) {
      try {
        const claims = decodeJwtPayload(accessToken)
        const exp = typeof claims["exp"] === "number" ? claims["exp"] * 1000 : null
        if (exp) {
          const delay = Math.max(exp - Date.now() - 60_000, 5_000)
          refreshTimer.current = window.setTimeout(() => {
            void getValidAccessToken().then((token) => {
              if (token) {
                applySession(token)
                void queryClient.invalidateQueries({ queryKey: ["my-permissions"] })
              }
            })
          }, delay)
        }
      } catch {
        // Token is already validated by use — a failed decode just skips the timer.
      }
    }
  }, [queryClient])

  useEffect(() => {
    let cancelled = false

    void getValidAccessToken()
      .then((token) => {
        if (!cancelled) {
          applySession(token)
          setIsLoading(false)
        }
      })
      .catch(() => {
        if (!cancelled) {
          applySession(null)
          setIsLoading(false)
        }
      })

    const unsubscribe = onUnauthorized(() => {
      applySession(null)
      queryClient.clear()
    })

    return () => {
      cancelled = true
      unsubscribe()
      if (refreshTimer.current !== null) {
        window.clearTimeout(refreshTimer.current)
      }
    }
  }, [applySession, queryClient])

  const login = useCallback(
    async (email: string, password: string) => {
      const session = await loginWithPassword(email, password)
      applySession(session.accessToken)
      await queryClient.invalidateQueries({ queryKey: ["my-permissions"] })
    },
    [applySession, queryClient]
  )

  const logout = useCallback(async () => {
    await logoutSession()
    clearSession()
    applySession(null)
    queryClient.clear()
  }, [applySession, queryClient])

  const value = useMemo<AuthContextValue>(
    () => ({ user, isLoading, isAuthenticated: user !== null, login, logout }),
    [user, isLoading, login, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuthContext() {
  return useContext(AuthContext)
}
