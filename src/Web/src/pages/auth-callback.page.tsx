import { useEffect, useRef, useState } from "react"
import { useNavigate } from "react-router"
import { CheckCircle2, XCircle } from "lucide-react"

import { completeSignIn, redirectToSignIn } from "@/lib/session"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"

/** The Keycloak authorization-code callback error keys users may actually hit. */
const friendlyMessages: Record<string, string> = {
  access_denied: "Sign-in was canceled. You can try again at any time.",
  login_required: "Your Keycloak session ended — start again to continue.",
  interaction_required: "Multi-factor authentication is required — start again to continue."
}

/**
 * Authorization Code + PKCE callback: oidc-client-ts exchanges the code against
 * Keycloak's token endpoint (verifier vs the challenge sent at sign-in), then we
 * return the user to wherever the flow started.
 */
export function AuthCallbackPage() {
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const completed = useRef(false)

  useEffect(() => {
    if (completed.current) {
      return
    }

    completed.current = true

    void completeSignIn()
      .then((state) => {
        void navigate(state?.from ?? "/", { replace: true })
      })
      .catch((unknown: unknown) => {
        const code = unknown instanceof Error && "error" in unknown ? String((unknown as { error?: string }).error) : null
        setError(code && friendlyMessages[code] ? friendlyMessages[code] : "Sign-in could not be completed. Please try again.")
      })
  }, [navigate])

  const retry = () => {
    void redirectToSignIn("/")
  }

  return (
    <div className="container flex min-h-[70svh] items-center justify-center">
      <Card className="w-full max-w-md rounded-2xl border-border/70">
        <CardContent className="flex flex-col items-center gap-3 p-8 text-center">
          {error === null ? (
            <>
              <CheckCircle2 className="size-10 text-primary" />
              <p className="font-display text-lg font-bold">Signing you in…</p>
              <p className="text-sm text-muted-foreground">Finishing the secure sign-in with Evently Identity.</p>
            </>
          ) : (
            <>
              <XCircle className="size-10 text-destructive" />
              <p className="font-display text-lg font-bold">Sign-in failed</p>
              <p className="text-sm leading-relaxed text-muted-foreground">{error}</p>
              <Button className="btn-press mt-2 rounded-xl font-semibold" onClick={retry}>
                Try again
              </Button>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}