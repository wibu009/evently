import { useState } from "react"
import { Link } from "react-router"
import { useQuery } from "@tanstack/react-query"
import { CalendarPlus, ChevronRight, ShieldCheck } from "lucide-react"

import { eventsApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader, PaginationControls, StatusBadge } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { formatEventDate } from "@/lib/format"

export function EventsAdminPage() {
  const [page, setPage] = useState(1)

  const eventsQuery = useQuery({
    queryKey: ["admin-events", page],
    queryFn: ({ signal }) => eventsApi.listAll(page, 12, signal)
  })

  if (eventsQuery.isPending) {
    return (
      <>
        <PageHeader title="Events" eyebrow="Admin" />
        <LoadingState rows={4} />
      </>
    )
  }

  if (eventsQuery.isError) {
    return (
      <>
        <PageHeader title="Events" eyebrow="Admin" />
        <ErrorState message={String(eventsQuery.error)} onRetry={() => eventsQuery.refetch()} />
      </>
    )
  }

  const data = eventsQuery.data

  return (
    <div>
      <PageHeader
        title="Events"
        eyebrow={`Admin · ${data.totalCount} total`}
        description="Every event in every state — drafts, live, completed, and canceled. Open one to manage its lifecycle."
        actions={
          <Button asChild size="sm" className="btn-press rounded-xl shadow-lg shadow-primary/20">
            <Link to="/admin/events/new">
              <CalendarPlus className="size-4" /> New event
            </Link>
          </Button>
        }
      />

      {data.events.length === 0 ? (
        <EmptyState
          title="No events yet"
          description="Create your first draft — add ticket types, then publish it to the catalog."
          icon={<ShieldCheck className="size-5" />}
        />
      ) : (
        <>
          <div className="space-y-2.5">
            {data.events.map((event, i) => (
              <Link
                key={event.id}
                to={`/admin/events/${event.id}/edit`}
                className="group animate-fade-up block focus-visible:outline-none"
                style={{ animationDelay: `${Math.min(i, 8) * 50}ms` }}
              >
                <Card className="card-lift rounded-2xl border-border/70 bg-card/70">
                  <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:p-5">
                    <div className="min-w-0">
                      <p className="truncate font-display text-[15px] font-bold tracking-tight transition-colors group-hover:text-primary">{event.title}</p>
                      <p className="mt-1 truncate font-mono text-xs text-muted-foreground">
                        {formatEventDate(event.startAtUtc, event.endAtUtc)} · {event.location}
                      </p>
                    </div>
                    <div className="flex items-center justify-between gap-3 border-t border-dashed border-border/70 pt-3 sm:justify-end sm:border-0 sm:pt-0">
                      <StatusBadge status={event.status} />
                      <ChevronRight className="size-4 text-muted-foreground transition-transform duration-300 group-hover:translate-x-1 group-hover:text-primary" />
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>
          <PaginationControls page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}

