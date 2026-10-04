import { createContext, useContext } from "react"
import { useQuery } from "@tanstack/react-query"

import { useAuthContext } from "@/lib/auth-context"
import { usersApi } from "@/api/endpoints"

interface PermissionsContextValue {
  permissions: Set<string> | null
}

const PermissionsContext = createContext<PermissionsContextValue>({
  permissions: null
})

/**
 * Permission claims are resolved server-side and are not embedded in the access token,
 * so this provider fetches the current user's permission set once after sign-in
 * (GET /users/my-permissions) and exposes it to permission-gated navigation.
 */
export function PermissionsProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthContext()

  const permissionsQuery = useQuery({
    queryKey: ["my-permissions"],
    queryFn: ({ signal }) => usersApi.myPermissions(signal),
    enabled: isAuthenticated,
    staleTime: 5 * 60_000
  })

  const permissions = permissionsQuery.data ? new Set(permissionsQuery.data.permissions) : null

  return <PermissionsContext.Provider value={{ permissions }}>{children}</PermissionsContext.Provider>
}

export function usePermissions(): (permission: string) => boolean {
  const { permissions } = useContext(PermissionsContext)

  return (permission: string) => permissions?.has(permission) ?? false
}
