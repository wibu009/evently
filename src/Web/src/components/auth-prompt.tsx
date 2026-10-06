import { useCallback, useState } from "react"
import { LogIn, Ticket } from "lucide-react"

import { useAuthContext } from "@/lib/auth-context"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog"

/**
 * Guards an action behind authentication. When the visitor is anonymous,
 * a branded sign-in prompt opens instead of firing the request into a 401.
 * Signing in redirects to Keycloak and the callback route returns the user
 * to the page they came from.
 */
export function useSignInPrompt() {
  const { login, isAuthenticated } = useAuthContext()
  const [open, setOpen] = useState(false)

  const requireAuth = useCallback(
    (action: () => void) => {
      if (isAuthenticated) {
        action()
      } else {
        setOpen(true)
      }
    },
    [isAuthenticated]
  )

  const prompt = (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className="rounded-2xl sm:max-w-md">
        <DialogHeader className="items-center text-center sm:text-center">
          <div className="mx-auto mb-2 flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-primary via-fuchsia-500 to-orange-400 shadow-lg shadow-primary/30">
            <Ticket className="size-6 text-white" />
          </div>
          <DialogTitle className="font-display text-xl">Sign in to book tickets</DialogTitle>
          <DialogDescription className="leading-relaxed">
            You need an account to add tickets to your cart or join a waiting list. It takes less than a minute — and browsing stays free forever.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="flex-col gap-2 sm:flex-col">
          <Button
            className="btn-press h-11 w-full rounded-xl font-bold"
            onClick={() => {
              setOpen(false)
              void login()
            }}
          >
            <LogIn className="size-4" /> Sign in
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )

  return { requireAuth, prompt }
}
