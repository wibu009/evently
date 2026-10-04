import type { AuthUser } from "@/lib/auth-context"

export type { AuthUser }

/**
 * Compatibility helper for permission-gated UI. Permissions are resolved
 * server-side (GET /users/my-permissions), not from the token.
 */
export function hasPermission(user: AuthUser | null, permission: string): boolean {
  void user
  void permission

  return false
}
