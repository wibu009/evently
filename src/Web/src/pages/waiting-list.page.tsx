import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { BellRing, Hourglass, LogOut } from "lucide-react"

import { waitingListApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader, StatusBadge } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/http"
import { formatDateTime, formatRelative } from "@/lib/format"

export function WaitingListPage() {
  const queryClient = useQueryClient()

  const entriesQuery = useQuery({
    queryKey: ["waiting-list"],
    queryFn: ({ signal }) => waitingListApi.myEntries(signal)
  })

  const leaveMutation = useMutation({
    mutationFn: waitingListApi.leave,
    onSuccess: () => {
      toast.info("You left the waiting list")
      void queryClient.invalidateQueries({ queryKey: ["waiting-list"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not leave the waiting list")
  })

  if (entriesQuery.isPending) {
    return (
      <>
        <PageHeader title="Waiting list" eyebrow="Sold-out queue" />
        <LoadingState rows={3} />
      </>
    )
  }

  if (entriesQuery.isError) {
    return (
      <>
        <PageHeader title="Waiting list" eyebrow="Sold-out queue" />
        <ErrorState message={String(entriesQuery.error)} onRetry={() => entriesQuery.refetch()} />
      </>
    )
  }

  const entries = entriesQuery.data

  return (
    <div>
      <PageHeader
        title="Waiting list"
        eyebrow={`Sold-out queue · ${entries.length}`}
        description="Events you're queued for. When inventory opens up, the earliest waiters get notified first."
      />

      {entries.length === 0 ? (
        <EmptyState
          title="You're not waiting for anything"
          description="Sold out? Join the waiting list from the event page and we'll hold your place in line."
          actionLabel="Discover events"
          actionTo="/"
          icon={<Hourglass className="size-5" />}
        />
      ) : (
        <div className="space-y-2.5">
          {entries.map((entry, i) => (
            <Card
              key={entry.id}
              className="animate-fade-up card-lift rounded-2xl border-border/70 bg-card/70"
              style={{ animationDelay: `${Math.min(i, 8) * 50}ms` }}
            >
              <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:p-5">
                <div className="flex min-w-0 items-start gap-3.5">
                  <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-warning/15">
                    <BellRing className="size-5 text-warning" />
                  </div>
                  <div className="min-w-0">
                    <p className="truncate font-mono text-xs text-muted-foreground">{entry.ticketTypeId}</p>
                    <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
                      Queued {formatDateTime(entry.createdAtUtc)} · {formatRelative(entry.createdAtUtc)}
                    </p>
                    {entry.notifiedAtUtc && (
                      <p className="mt-1 inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary">
                        Spot open — notified {formatRelative(entry.notifiedAtUtc)}
                      </p>
                    )}
                  </div>
                </div>
                <div className="flex items-center justify-between gap-3 border-t border-dashed border-border/70 pt-3 sm:justify-end sm:border-0 sm:pt-0">
                  <StatusBadge status={entry.status} />
                  {entry.status === "Waiting" && (
                    <Button
                      variant="outline"
                      size="sm"
                      className="btn-press rounded-xl"
                      disabled={leaveMutation.isPending}
                      onClick={() => leaveMutation.mutate(entry.ticketTypeId)}
                    >
                      <LogOut className="size-3.5" /> Leave
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
