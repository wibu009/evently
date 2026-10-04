import { useAuthContext, type AuthUser } from "@/lib/auth-context"
import { usePermissions } from "@/lib/permissions"

export function useCurrentUser(): { user: AuthUser | null; isLoading: boolean; isAuthenticated: boolean } {
  const { user, isLoading, isAuthenticated } = useAuthContext()

  return { user, isLoading, isAuthenticated }
}

export function useHasPermission(): (permission: string) => boolean {
  return usePermissions()
}
