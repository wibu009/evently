import { useState } from "react"
import { Link } from "react-router"
import { useQuery } from "@tanstack/react-query"
import { ArrowUpRight, CalendarX2, MapPin, Search, Sparkles, Ticket, X } from "lucide-react"

import { categoriesApi, eventsApi } from "@/api/endpoints"
import type { EventSummary } from "@/api/types"
import { EmptyState, ErrorState, LoadingState, PaginationControls } from "@/components/shared"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select"
import { formatEventDate } from "@/lib/format"
import { cn } from "@/lib/utils"

const coverGradients = [
  "from-violet-600 via-fuchsia-500 to-orange-400",
  "from-cyan-500 via-blue-600 to-violet-700",
  "from-lime-400 via-emerald-500 to-teal-700",
  "from-rose-500 via-red-500 to-orange-500",
  "from-indigo-500 via-purple-500 to-pink-500",
  "from-amber-400 via-orange-500 to-rose-600"
]

function gradientFor(id: string) {
  let hash = 0
  for (let i = 0; i < id.length; i++) hash = (hash * 31 + id.charCodeAt(i)) >>> 0
  return coverGradients[hash % coverGradients.length]
}

function EventCard({ event, index }: { event: EventSummary; index: number }) {
  const city = event.location.split(",").pop()?.trim() ?? "Venue TBA"

  return (
    <Link
      to={`/events/${event.id}`}
      className="group animate-fade-up block focus-visible:outline-none"
      style={{ animationDelay: `${Math.min(index, 7) * 60}ms` }}
    >
      <Card className="card-lift h-full overflow-hidden rounded-2xl border-border/70 bg-card/80 py-0 backdrop-blur">
        <div className={cn("relative h-28 overflow-hidden bg-gradient-to-br", !event.coverImageUrl && gradientFor(event.id))}>
          {event.coverImageUrl ? (
            <img
              src={event.coverImageUrl}
              alt=""
              loading="lazy"
              className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
              onError={(e) => {
                e.currentTarget.style.display = "none"
              }}
            />
          ) : (
            <>
              <div className="bg-noise absolute inset-0" />
              <div className="bg-grid absolute inset-0 opacity-40 [mask-image:radial-gradient(ellipse_at_center,black,transparent_75%)]" />
            </>
          )}
          <div className="absolute inset-x-0 bottom-0 h-10 bg-gradient-to-t from-black/45 to-transparent" />
          <Badge className="absolute top-3 left-3 rounded-full border-white/20 bg-black/35 px-2.5 py-1 text-[10px] font-semibold tracking-widest text-white uppercase backdrop-blur-md">
            <MapPin className="mr-1 size-3" />{city}
          </Badge>
          <Ticket className="absolute right-3 -bottom-2 size-14 rotate-[-12deg] text-white/25 transition-transform duration-300 group-hover:rotate-[-4deg] group-hover:scale-110" />
        </div>
        <CardContent className="p-4 sm:p-5">
          <h3 className="text-display text-[17px] leading-snug font-bold tracking-tight transition-colors group-hover:text-primary">
            {event.title}
          </h3>
          <p className="mt-1.5 line-clamp-2 text-[13px] leading-relaxed text-muted-foreground">{event.description}</p>
          <div className="mt-4 flex items-center justify-between gap-2 border-t border-dashed border-border/80 pt-3">
            <p className="font-mono text-[11px] text-muted-foreground">{formatEventDate(event.startAtUtc, event.endAtUtc)}</p>
            <span className="inline-flex size-7 items-center justify-center rounded-full bg-primary/10 text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
              <ArrowUpRight className="size-3.5 transition-transform duration-300 group-hover:rotate-45" />
            </span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}

export function CatalogPage() {
  const [search, setSearch] = useState("")
  const [query, setQuery] = useState("")
  const [categoryId, setCategoryId] = useState<string>("all")
  const [page, setPage] = useState(1)

  const categoriesQuery = useQuery({
    queryKey: ["categories"],
    queryFn: ({ signal }) => categoriesApi.list(signal)
  })

  const eventsQuery = useQuery({
    queryKey: ["catalog", query, categoryId, page],
    queryFn: ({ signal }) =>
      eventsApi.search(
        {
          search: query || undefined,
          categoryId: categoryId === "all" ? undefined : categoryId,
          page,
          pageSize: 12
        },
        signal
      )
  })

  const activeCategories = categoriesQuery.data?.filter((c) => !c.isArchived) ?? []
  const hasFilters = query !== "" || categoryId !== "all"

  return (
    <div>
      {/* hero */}
      <section className="animate-fade-up relative overflow-hidden rounded-2xl border border-border/70 bg-card/60 px-5 py-8 sm:px-8 sm:py-10 lg:px-10">
        <div aria-hidden className="pointer-events-none absolute inset-0">
          <div className="bg-grid absolute inset-0 opacity-60 [mask-image:radial-gradient(ellipse_60%_80%_at_20%_20%,black,transparent)]" />
          <div className="absolute -top-20 -right-16 h-56 w-56 rounded-full bg-primary/20 blur-[90px]" />
          <div className="absolute -bottom-24 -left-10 h-48 w-48 rounded-full bg-fuchsia-500/15 blur-[90px]" />
        </div>
        <div className="relative">
          <p className="label-micro flex items-center gap-2">
            <Sparkles className="size-3.5 text-primary" /> Live catalog
          </p>
          <h1 className="text-display mt-2 max-w-2xl text-3xl font-bold tracking-tight text-balance sm:text-4xl lg:text-[2.75rem] lg:leading-[1.05]">
            Find your next <span className="text-gradient">night out</span>
          </h1>
          <p className="mt-2.5 max-w-xl text-sm leading-relaxed text-muted-foreground sm:text-[15px]">
            Search the public catalog of live events — reserve in seconds, pay within 15 minutes, scan at the door.
          </p>

          <form
            className="mt-6 flex flex-col gap-2 sm:flex-row sm:items-center"
            onSubmit={(e) => {
              e.preventDefault()
              setPage(1)
              setQuery(search.trim())
            }}
          >
            <div className="relative flex-1">
              <Search className="absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search events, venues, artists…"
                className="h-11 rounded-xl bg-background/80 pr-9 pl-10 text-[15px] shadow-sm transition-shadow focus-visible:shadow-[0_0_0_3px_var(--ring)/25%]"
              />
              {search && (
                <button
                  type="button"
                  aria-label="Clear search"
                  onClick={() => { setSearch(""); setQuery(""); setPage(1) }}
                  className="absolute top-1/2 right-3 -translate-y-1/2 rounded-full p-0.5 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                >
                  <X className="size-4" />
                </button>
              )}
            </div>
            <div className="flex gap-2">
              <Select
                value={categoryId}
                onValueChange={(value) => {
                  setPage(1)
                  setCategoryId(value)
                }}
              >
                <SelectTrigger className="h-11 flex-1 rounded-xl sm:w-48 sm:flex-none">
                  <SelectValue placeholder="Category" />
                </SelectTrigger>
                <SelectContent className="rounded-xl">
                  <SelectItem value="all">All categories</SelectItem>
                  {activeCategories.map((category) => (
                    <SelectItem key={category.id} value={category.id}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button type="submit" className="btn-press h-11 rounded-xl px-5 font-semibold">Search</Button>
            </div>
          </form>

          {activeCategories.length > 0 && (
            <div className="mt-4 flex flex-wrap gap-1.5">
              <button
                onClick={() => { setCategoryId("all"); setPage(1) }}
                className={cn(
                  "btn-press rounded-full border px-3 py-1 text-xs font-medium transition-all",
                  categoryId === "all"
                    ? "border-primary bg-primary text-primary-foreground shadow-md shadow-primary/25"
                    : "border-border bg-background/60 text-muted-foreground hover:border-primary/40 hover:text-foreground"
                )}
              >
                All
              </button>
              {activeCategories.slice(0, 6).map((c) => (
                <button
                  key={c.id}
                  onClick={() => { setCategoryId(c.id); setPage(1) }}
                  className={cn(
                    "btn-press rounded-full border px-3 py-1 text-xs font-medium transition-all",
                    categoryId === c.id
                      ? "border-primary bg-primary text-primary-foreground shadow-md shadow-primary/25"
                      : "border-border bg-background/60 text-muted-foreground hover:border-primary/40 hover:text-foreground"
                  )}
                >
                  {c.name}
                </button>
              ))}
            </div>
          )}
        </div>
      </section>

      {/* results */}
      <div className="mt-6 flex items-center justify-between">
        <p className="text-sm text-muted-foreground tabular-nums">
          {eventsQuery.data ? (
            <><span className="font-semibold text-foreground">{eventsQuery.data.totalCount}</span> {eventsQuery.data.totalCount === 1 ? "event" : "events"}{hasFilters ? " matching your filters" : " live right now"}</>
          ) : "Searching…"}
        </p>
        {hasFilters && (
          <Button
            variant="ghost"
            size="sm"
            className="h-8 rounded-lg text-xs"
            onClick={() => { setSearch(""); setQuery(""); setCategoryId("all"); setPage(1) }}
          >
            <X className="size-3.5" /> Clear filters
          </Button>
        )}
      </div>

      <div className="mt-4">
        {eventsQuery.isPending ? (
          <LoadingState rows={8} />
        ) : eventsQuery.isError ? (
          <ErrorState message={String(eventsQuery.error)} onRetry={() => eventsQuery.refetch()} />
        ) : eventsQuery.data.events.length === 0 ? (
          <EmptyState
            title="No events found"
            description="Try a different search term, pick another category, or clear the filters to see everything."
            icon={<CalendarX2 className="size-5" />}
          />
        ) : (
          <>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {eventsQuery.data.events.map((event, i) => (
                <EventCard key={event.id} event={event} index={i} />
              ))}
            </div>
            <PaginationControls
              page={eventsQuery.data.page}
              pageSize={eventsQuery.data.pageSize}
              totalCount={eventsQuery.data.totalCount}
              onPageChange={setPage}
            />
          </>
        )}
      </div>
    </div>
  )
}
