import { Link } from "react-router"
import { AlertTriangle, Inbox, RotateCcw } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { Spinner } from "@/components/ui/spinner"
import { cn } from "@/lib/utils"

export function PageHeader({
  title,
  description,
  actions,
  eyebrow
}: {
  title: string
  description?: string
  actions?: React.ReactNode
  eyebrow?: string
}) {
  return (
    <div className="animate-fade-up mb-6 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:flex-wrap sm:items-end sm:justify-between">
      <div className="min-w-0">
        {eyebrow && <p className="label-micro mb-2 flex items-center gap-2"><span className="inline-block size-1.5 rounded-full bg-primary" />{eyebrow}</p>}
        <h1 className="text-display text-2xl font-bold tracking-tight text-balance sm:text-3xl lg:text-[2rem]">{title}</h1>
        {description && <p className="mt-1.5 max-w-xl text-sm leading-relaxed text-muted-foreground">{description}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  )
}

export function EmptyState({
  title,
  description,
  actionLabel,
  actionTo,
  icon
}: {
  title: string
  description?: string
  actionLabel?: string
  actionTo?: string
  icon?: React.ReactNode
}) {
  return (
    <div className="animate-fade-up flex min-h-56 flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-border/80 bg-card/40 px-6 py-12 text-center">
      <div className="flex size-11 items-center justify-center rounded-xl bg-primary/10 text-primary">
        {icon ?? <Inbox className="size-5" />}
      </div>
      <p className="font-display text-lg font-semibold tracking-tight">{title}</p>
      {description && <p className="max-w-sm text-sm leading-relaxed text-muted-foreground">{description}</p>}
      {actionLabel && actionTo && (
        <Button asChild className="btn-press mt-2 rounded-lg">
          <Link to={actionTo}>{actionLabel}</Link>
        </Button>
      )}
    </div>
  )
}

export function LoadingState({ rows = 0 }: { rows?: number }) {
  if (rows > 0) {
    return (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className="rounded-xl border p-4">
            <Skeleton className="h-4 w-1/3" />
            <Skeleton className="mt-3 h-6 w-3/4" />
            <Skeleton className="mt-2 h-4 w-full" />
            <Skeleton className="mt-4 h-4 w-1/2" />
          </div>
        ))}
      </div>
    )
  }

  return (
    <div className="flex min-h-48 flex-col items-center justify-center gap-3">
      <Spinner className="size-6 text-primary" />
      <p className="label-micro animate-pulse">Loading</p>
    </div>
  )
}

export function ErrorState({ message, onRetry }: { message: string; onRetry?: () => void }) {
  const friendly = message.includes("ApiError:")
    ? message.replace(/^.*ApiError:\s*/, "")
    : message

  return (
    <div className="animate-fade-in flex min-h-48 flex-col items-center justify-center gap-3 rounded-xl border border-destructive/25 bg-destructive/[0.04] px-6 py-10 text-center">
      <div className="flex size-11 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
        <AlertTriangle className="size-5" />
      </div>
      <p className="font-display font-semibold">Something went wrong</p>
      <p className="max-w-md text-sm leading-relaxed text-muted-foreground">{friendly}</p>
      {onRetry && (
        <Button variant="outline" size="sm" onClick={onRetry} className="btn-press mt-1 rounded-lg">
          <RotateCcw className="size-3.5" /> Try again
        </Button>
      )}
    </div>
  )
}

const statusConfig: Record<string, { tone: string; pulse?: boolean }> = {
  Draft: { tone: "bg-muted text-muted-foreground" },
  Published: { tone: "bg-primary/15 text-primary", pulse: true },
  Completed: { tone: "bg-success/15 text-success" },
  Canceled: { tone: "bg-destructive/10 text-destructive" },
  Pending: { tone: "bg-warning/15 text-warning", pulse: true },
  Paid: { tone: "bg-success/15 text-success" },
  Refunded: { tone: "bg-primary/15 text-primary" },
  Expired: { tone: "bg-muted text-muted-foreground" },
  Succeeded: { tone: "bg-success/15 text-success" },
  Failed: { tone: "bg-destructive/10 text-destructive" },
  Waiting: { tone: "bg-warning/15 text-warning", pulse: true },
  Notified: { tone: "bg-primary/15 text-primary" }
}

export function StatusBadge({ status, className }: { status: string; className?: string }) {
  const config = statusConfig[status] ?? { tone: "bg-muted text-muted-foreground" }

  return (
    <Badge
      variant="outline"
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[11px] font-semibold tracking-wide",
        config.tone,
        className
      )}
    >
      <span
        className={cn(
          "size-1.5 rounded-full bg-current",
          config.pulse && "animate-[pulse-dot_1.6s_ease-in-out_infinite]"
        )}
      />
      {status}
    </Badge>
  )
}

export function PaginationControls({
  page,
  pageSize,
  totalCount,
  onPageChange
}: {
  page: number
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
}) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  if (totalCount === 0) {
    return null
  }

  return (
    <div className="mt-6 flex flex-col items-center justify-between gap-3 text-sm text-muted-foreground sm:flex-row">
      <p className="tabular-nums">
        Page <span className="font-semibold text-foreground">{page}</span> of {totalPages} · {totalCount} total
      </p>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" className="btn-press rounded-lg" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          Previous
        </Button>
        <Button variant="outline" size="sm" className="btn-press rounded-lg" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>
          Next
        </Button>
      </div>
    </div>
  )
}
