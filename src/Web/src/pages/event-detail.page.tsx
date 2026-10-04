import { useState } from "react"
import { Link, useParams } from "react-router"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ArrowLeft, BarChart3, BellRing, CalendarClock, Check, MapPin, Minus, Plus, ShieldCheck, Ticket, Timer } from "lucide-react"

import { cartApi, eventsApi, statisticsApi, waitingListApi } from "@/api/endpoints"
import type { TicketTypeResponse } from "@/api/types"
import { ErrorState, LoadingState } from "@/components/shared"
import { useSignInPrompt } from "@/components/auth-prompt"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { ApiError } from "@/lib/http"
import { formatEventDate, formatMoney } from "@/lib/format"
import { useHasPermission } from "@/lib/use-auth"
import { useAuthContext } from "@/lib/auth-context"
import { cn } from "@/lib/utils"

function TicketTypeRow({ ticketType, index }: { ticketType: TicketTypeResponse; index: number }) {
  const queryClient = useQueryClient()
  const [quantity, setQuantity] = useState(1)
  const { isAuthenticated } = useAuthContext()
  const { requireAuth, prompt } = useSignInPrompt()

  const soldOut = ticketType.quantity <= 0

  const waitlistQuery = useQuery({
    queryKey: ["waiting-list"],
    queryFn: ({ signal }) => waitingListApi.myEntries(signal),
    enabled: isAuthenticated
  })

  const joined = waitlistQuery.data?.some((entry) => entry.ticketTypeId === ticketType.ticketTypeId)

  const addToCartMutation = useMutation({
    mutationFn: cartApi.addItem,
    onSuccess: () => {
      toast.success(`${quantity} × ${ticketType.name} added to your cart`, {
        description: "Head to your cart to check out within the hold window."
      })
      void queryClient.invalidateQueries({ queryKey: ["cart"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not add to cart")
  })

  const joinMutation = useMutation({
    mutationFn: waitingListApi.join,
    onSuccess: () => {
      toast.success("You're on the waiting list", {
        description: "We'll notify you the moment tickets open up."
      })
      void queryClient.invalidateQueries({ queryKey: ["waiting-list"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not join the waiting list")
  })

  const leaveMutation = useMutation({
    mutationFn: waitingListApi.leave,
    onSuccess: () => {
      toast.info("You left the waiting list")
      void queryClient.invalidateQueries({ queryKey: ["waiting-list"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not leave the waiting list")
  })

  return (
    <div
      className="animate-fade-up group flex flex-col gap-4 rounded-2xl border border-border/70 bg-card/70 p-4 backdrop-blur transition-all duration-300 hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-[0_16px_40px_-16px_var(--primary)/30%] sm:flex-row sm:items-center sm:justify-between sm:p-5"
      style={{ animationDelay: `${index * 70}ms` }}
    >
      <div className="flex min-w-0 items-center gap-3">
        <span
          aria-hidden
          className="flex size-11 shrink-0 items-center justify-center rounded-xl font-display text-sm font-bold text-white"
          style={{
            background: ticketType.backgroundImageUrl
              ? `center/cover url(${ticketType.backgroundImageUrl})`
              : ticketType.color ?? undefined
          }}
        >
          {!ticketType.backgroundImageUrl && !ticketType.color && <Ticket className="size-5 text-primary" />}
          {(ticketType.backgroundImageUrl ?? ticketType.color) && <Ticket className="size-5 text-white drop-shadow" />}
        </span>
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <p className="font-display text-[15px] font-bold tracking-tight">{ticketType.name}</p>
            {soldOut ? (
              <Badge variant="outline" className="rounded-full bg-destructive/10 px-2 py-0.5 text-[10px] font-bold tracking-widest text-destructive uppercase">Sold out</Badge>
            ) : ticketType.quantity <= 10 ? (
              <Badge variant="outline" className="rounded-full bg-warning/15 px-2 py-0.5 text-[10px] font-bold tracking-widest text-warning uppercase">Only {ticketType.quantity} left</Badge>
            ) : (
              <Badge variant="outline" className="rounded-full bg-success/10 px-2 py-0.5 text-[10px] font-bold tracking-widest text-success uppercase">Available</Badge>
            )}
          </div>
          <p className="mt-1.5 flex items-baseline gap-2">
            <span className="font-display text-xl font-bold tabular-nums">{formatMoney(ticketType.price, ticketType.currency)}</span>
            {!soldOut && <span className="text-xs text-muted-foreground tabular-nums">{ticketType.quantity} available</span>}
          </p>
        </div>
      </div>

      {soldOut ? (
        joined ? (
          <div className="flex items-center gap-2.5">
            <Badge className="animate-fade-in rounded-full bg-primary/15 px-3 py-1.5 text-xs font-semibold text-primary">
              <Check className="mr-1 size-3.5" /> Queued
            </Badge>
            <Button variant="outline" size="sm" className="btn-press rounded-xl" disabled={leaveMutation.isPending} onClick={() => leaveMutation.mutate(ticketType.ticketTypeId)}>
              Leave
            </Button>
          </div>
        ) : (
          <Button
            size="sm"
            variant="secondary"
            className="btn-press w-full rounded-xl sm:w-auto"
            disabled={joinMutation.isPending}
            onClick={() => requireAuth(() => joinMutation.mutate(ticketType.ticketTypeId))}
          >
            <BellRing className="size-4" /> Notify me
          </Button>
        )
      ) : (
        <div className="flex items-center gap-2.5">
          <div className="flex items-center rounded-xl border border-border/80 bg-background/60 p-1">
            <Button
              variant="ghost"
              size="icon"
              className="btn-press size-8 rounded-lg"
              aria-label="Decrease quantity"
              disabled={quantity <= 1}
              onClick={() => setQuantity((q) => Math.max(1, q - 1))}
            >
              <Minus className="size-3.5" />
            </Button>
            <span key={quantity} className="animate-pop w-8 text-center font-mono text-sm font-bold tabular-nums">{quantity}</span>
            <Button
              variant="ghost"
              size="icon"
              className="btn-press size-8 rounded-lg"
              aria-label="Increase quantity"
              disabled={quantity >= ticketType.quantity}
              onClick={() => setQuantity((q) => Math.min(ticketType.quantity, q + 1))}
            >
              <Plus className="size-3.5" />
            </Button>
          </div>
          <Button
            size="sm"
            className="btn-press flex-1 rounded-xl px-4 font-semibold shadow-lg shadow-primary/20 transition-shadow hover:shadow-primary/35 sm:flex-none"
            disabled={addToCartMutation.isPending}
            onClick={() =>
              requireAuth(() => addToCartMutation.mutate({ ticketTypeId: ticketType.ticketTypeId, quantity }))
            }
          >
            <Ticket className="size-4" /> Add
          </Button>
        </div>
      )}
      {prompt}
    </div>
  )
}

export function EventDetailPage() {
  const { eventId = "" } = useParams()
  const has = useHasPermission()

  const eventQuery = useQuery({
    queryKey: ["event", eventId],
    queryFn: ({ signal }) => eventsApi.get(eventId, signal)
  })

  const statsQuery = useQuery({
    queryKey: ["event-statistics", eventId],
    queryFn: ({ signal }) => statisticsApi.forEvent(eventId, signal),
    enabled: has("event-statistics:read")
  })

  if (eventQuery.isPending) {
    return <LoadingState rows={3} />
  }

  if (eventQuery.isError) {
    return <ErrorState message={String(eventQuery.error)} onRetry={() => eventQuery.refetch()} />
  }

  const event = eventQuery.data
  const cheapest = event.ticketTypes.length > 0
    ? Math.min(...event.ticketTypes.map((t) => t.price))
    : null
  const currency = event.ticketTypes[0]?.currency ?? "USD"
  const coverImage = event.images.find((i) => i.isCover) ?? event.images[0] ?? null
  const heroImage = event.heroBannerUrl ?? coverImage?.imageUrl ?? null
  const accent = event.accentColor ?? null

  return (
    <div className="animate-fade-in">
      <Button asChild variant="ghost" size="sm" className="btn-press mb-4 -ml-2 rounded-lg text-muted-foreground hover:text-foreground sm:mb-6">
        <Link to="/">
          <ArrowLeft className="size-4" /> Back to discover
        </Link>
      </Button>

      {/* hero */}
      <section className="animate-fade-up relative overflow-hidden rounded-2xl border border-border/70">
        {heroImage ? (
          <img
            src={heroImage}
            alt=""
            className="absolute inset-0 h-full w-full object-cover"
            onError={(e) => {
              e.currentTarget.style.display = "none"
            }}
          />
        ) : (
          <div
            className="absolute inset-0 bg-gradient-to-br from-violet-600 via-fuchsia-600 to-orange-500"
            style={accent ? { background: `linear-gradient(135deg, ${accent}, #7c3aed 55%, #ea580c)` } : undefined}
          />
        )}
        <div className="bg-noise absolute inset-0" />
        <div className="bg-grid absolute inset-0 opacity-30 [mask-image:radial-gradient(ellipse_at_center,black,transparent_80%)]" />
        <div className="absolute inset-x-0 bottom-0 h-24 bg-gradient-to-t from-black/55 to-transparent" />
        <Ticket className="absolute -right-6 -bottom-8 size-44 rotate-[-14deg] text-white/15" />

        <div className="relative p-5 sm:p-8 lg:p-10">
          <p className="label-micro !text-white/70 flex items-center gap-2">
            <MapPin className="size-3.5" /> {event.location}
          </p>
          <h1 className="text-display mt-2 max-w-3xl text-2xl font-bold tracking-tight text-white text-balance sm:text-4xl lg:text-[2.75rem] lg:leading-[1.05]">
            {event.title}
          </h1>
          <p className="mt-2.5 max-w-2xl text-sm leading-relaxed text-white/85 sm:text-[15px]">{event.description}</p>
          <div className="mt-5 flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-black/30 px-3 py-1.5 font-mono text-xs text-white backdrop-blur-md">
              <CalendarClock className="size-3.5" />{formatEventDate(event.startAtUtc, event.endAtUtc)}
            </span>
            {cheapest !== null && (
              <span className="inline-flex items-center rounded-full bg-white px-3 py-1.5 text-xs font-bold text-black">
                From {formatMoney(cheapest, currency)}
              </span>
            )}
          </div>
        </div>
      </section>

      {event.images.length > 1 && (
        <div className="animate-fade-up stagger-1 mt-4 flex gap-2.5 overflow-x-auto pb-1">
          {event.images.map((image) => (
            <div
              key={image.imageId}
              className={cn(
                "relative h-20 w-32 shrink-0 overflow-hidden rounded-xl border transition-all",
                image.isCover ? "border-primary ring-2 ring-primary/30" : "border-border/70"
              )}
            >
              <img src={image.imageUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
              {image.isCover && (
                <span className="absolute bottom-1 left-1 rounded-full bg-black/60 px-1.5 py-0.5 text-[9px] font-bold tracking-widest text-white uppercase">
                  Cover
                </span>
              )}
            </div>
          ))}
        </div>
      )}

      <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_360px] lg:gap-8">
        <div className="min-w-0">
          <div className="flex items-center justify-between">
            <h2 className="label-micro">Choose your tickets</h2>
            <span className="text-xs text-muted-foreground tabular-nums">{event.ticketTypes.length} {event.ticketTypes.length === 1 ? "type" : "types"}</span>
          </div>
          <div className="mt-3 space-y-3">
            {event.ticketTypes.length === 0 ? (
              <div className="rounded-2xl border border-dashed p-8 text-center text-sm text-muted-foreground">
                No ticket types are on sale for this event yet.
              </div>
            ) : (
              event.ticketTypes.map((ticketType, i) => <TicketTypeRow key={ticketType.ticketTypeId} ticketType={ticketType} index={i} />)
            )}
          </div>

          <div className={cn("mt-6 grid gap-3 sm:grid-cols-3")}>
            {[
              { icon: Timer, title: "15-min hold", desc: "Reserved at checkout" },
              { icon: ShieldCheck, title: "Buyer protection", desc: "Refunds on cancel" },
              { icon: Ticket, title: "Mobile tickets", desc: "Scan at the door" }
            ].map((f) => (
              <div key={f.title} className="rounded-2xl border border-border/60 bg-card/50 p-4">
                <f.icon className="size-4 text-primary" />
                <p className="mt-2 text-[13px] font-bold">{f.title}</p>
                <p className="text-xs text-muted-foreground">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>

        <aside className="min-w-0 space-y-4 lg:sticky lg:top-20 lg:self-start">
          <Card className="overflow-hidden rounded-2xl border-border/70">
            <CardHeader className="border-b border-border/60 bg-muted/40 pb-3">
              <CardTitle className="font-display text-[15px]">Good to know</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 p-4 text-sm sm:p-5">
              <div className="flex justify-between gap-3">
                <span className="shrink-0 text-muted-foreground">Starts</span>
                <span className="text-right font-mono text-xs leading-relaxed">{formatEventDate(event.startAtUtc)}</span>
              </div>
              {event.endAtUtc && (
                <div className="flex justify-between gap-3">
                  <span className="shrink-0 text-muted-foreground">Ends</span>
                  <span className="text-right font-mono text-xs leading-relaxed">{formatEventDate(event.endAtUtc)}</span>
                </div>
              )}
              <Separator />
              <p className="text-xs leading-relaxed text-muted-foreground">
                Tickets are reserved at checkout with a 15-minute payment window. Unpaid reservations expire automatically and return to sale.
              </p>
            </CardContent>
          </Card>

          {has("event-statistics:read") && (
            <Card className="rounded-2xl border-border/70">
              <CardHeader className="pb-2">
                <CardTitle className="flex items-center gap-2 font-display text-[15px]">
                  <BarChart3 className="size-4 text-primary" /> Live statistics
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2.5 text-sm">
                {statsQuery.isPending ? (
                  <p className="animate-pulse text-muted-foreground">Loading…</p>
                ) : statsQuery.isError || !statsQuery.data ? (
                  <p className="text-muted-foreground">Statistics unavailable</p>
                ) : (
                  <>
                    <StatRow label="Tickets sold" value={String(statsQuery.data.ticketsSold)} highlight />
                    <StatRow label="Checked in" value={String(statsQuery.data.attendeesCheckedIn)} highlight />
                    <Separator />
                    <StatRow label="Duplicate attempts" value={String(statsQuery.data.duplicateCheckInTickets.length)} />
                    <StatRow label="Invalid attempts" value={String(statsQuery.data.invalidCheckInTickets.length)} />
                  </>
                )}
              </CardContent>
            </Card>
          )}
        </aside>
      </div>
    </div>
  )
}

function StatRow({ label, value, highlight = false }: { label: string; value: string; highlight?: boolean }) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-muted-foreground">{label}</span>
      <span className={cn("font-mono tabular-nums", highlight && "text-[15px] font-bold text-foreground")}>{value}</span>
    </div>
  )
}
