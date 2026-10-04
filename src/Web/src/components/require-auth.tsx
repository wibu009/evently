import { Navigate, Outlet, useLocation } from "react-router"

import { useAuthContext } from "@/lib/auth-context"
import { usePermissions } from "@/lib/permissions"
import { Spinner } from "@/components/ui/spinner"

export function RequireAuth() {
  const { isLoading, isAuthenticated } = useAuthContext()
  const location = useLocation()

  if (isLoading) {
    return (
      <div className="flex min-h-svh items-center justify-center">
        <Spinner className="size-8 text-primary" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}

export function RequirePermission({ permission, children }: { permission: string; children: React.ReactNode }) {
  const permissions = usePermissions()
  const hasPermission = permissions(permission)

  if (!hasPermission) {
    return (
      <div className="container flex min-h-[60svh] flex-col items-center justify-center gap-2 text-center">
        <p className="text-display text-4xl font-bold">403</p>
        <p className="text-muted-foreground">
          You need the <code className="text-primary">{permission}</code> permission to view this area.
        </p>
      </div>
    )
  }

  return children
}
