import { Link } from "react-router"
import { Ghost } from "lucide-react"

import { Button } from "@/components/ui/button"

export function NotFoundPage() {
  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center gap-4 overflow-hidden p-4 text-center">
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="bg-grid absolute inset-0 opacity-50 [mask-image:radial-gradient(ellipse_50%_50%_at_50%_40%,black,transparent)]" />
        <div className="absolute top-1/3 left-1/2 h-56 w-96 -translate-x-1/2 rounded-full bg-primary/15 blur-[100px]" />
      </div>
      <div className="animate-fade-up flex size-16 items-center justify-center rounded-2xl bg-muted">
        <Ghost className="size-8 text-muted-foreground" />
      </div>
      <p className="text-display text-gradient animate-fade-up text-7xl font-bold tracking-tight sm:text-8xl stagger-1">404</p>
      <p className="animate-fade-up max-w-sm text-sm leading-relaxed text-muted-foreground stagger-2">
        The page you're looking for drifted off stage. Let's get you back to the show.
      </p>
      <Button asChild className="btn-press animate-fade-up mt-2 rounded-xl stagger-3">
        <Link to="/">Back to discover</Link>
      </Button>
    </div>
  )
}
