import { useState } from "react"
import { Link, useParams } from "react-router"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ArrowLeft, ReceiptText, Ticket } from "lucide-react"

import { ordersApi, ticketsApi } from "@/api/endpoints"
import { ErrorState, LoadingState, StatusBadge } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from "@/components/ui/dialog"
import { Separator } from "@/components/ui/separator"
import { Textarea } from "@/components/ui/textarea"
import { ApiError } from "@/lib/http"
import { formatDateTime, formatMoney, formatRelative } from "@/lib/format"

export function OrderDetailPage() {
  const { orderId = "" } = useParams()
  const queryClient = useQueryClient()
  const [cancelOpen, setCancelOpen] = useState(false)
  const [reason, setReason] = useState("")

  const orderQuery = useQuery({
    queryKey: ["order", orderId],
    queryFn: ({ signal }) => ordersApi.get(orderId, signal)
  })

  const ticketsQuery = useQuery({
    queryKey: ["tickets", "order", orderId],
    queryFn: ({ signal }) => ticketsApi.forOrder(orderId, signal)
  })

  const cancelMutation = useMutation({
    mutationFn: () => ordersApi.cancel(orderId, reason.trim() || undefined),
    onSuccess: () => {
      toast.success(orderQuery.data?.status === "Paid" ? "Order refunded — tickets invalidated" : "Order canceled — inventory released")
      setCancelOpen(false)
      setReason("")
      void queryClient.invalidateQueries({ queryKey: ["order", orderId] })
      void queryClient.invalidateQueries({ queryKey: ["orders"] })
      void queryClient.invalidateQueries({ queryKey: ["tickets", "order", orderId] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not cancel the order")
  })

  if (orderQuery.isPending) {
    return <LoadingState rows={2} />
  }

  if (orderQuery.isError) {
    return <ErrorState message={String(orderQuery.error)} onRetry={() => orderQuery.refetch()} />
  }

  const order = orderQuery.data
  const canCancel = order.status === "Pending" || order.status === "Paid"
  const net = order.totalPrice - order.discountAmount

  return (
    <div className="animate-fade-in">
      <Button asChild variant="ghost" size="sm" className="btn-press mb-4 -ml-2 rounded-lg text-muted-foreground hover:text-foreground sm:mb-6">
        <Link to="/orders">
          <ArrowLeft className="size-4" /> All orders
        </Link>
      </Button>

      {/* hero summary */}
      <section className="animate-fade-up relative overflow-hidden rounded-2xl border border-border/70 bg-card/60 p-5 sm:p-7">
        <div aria-hidden className="pointer-events-none absolute inset-0">
          <div className="absolute -top-16 -right-10 h-44 w-44 rounded-full bg-primary/15 blur-[80px]" />
        </div>
        <div className="relative flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="min-w-0">
            <p className="label-micro flex items-center gap-2"><ReceiptText className="size-3.5" /> Order · {formatRelative(order.createdAtUtc)}</p>
            <p className="mt-1.5 truncate font-mono text-xs text-muted-foreground">{order.id}</p>
            <p className="text-display mt-2 text-3xl font-bold tracking-tight tabular-nums sm:text-4xl">
              {formatMoney(net, order.currency)}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2.5">
            <StatusBadge status={order.status} />
            {canCancel && (
              <Dialog open={cancelOpen} onOpenChange={setCancelOpen}>
                <DialogTrigger asChild>
                  <Button variant="outline" size="sm" className="btn-press rounded-xl">
                    Cancel order
                  </Button>
                </DialogTrigger>
                <DialogContent className="rounded-2xl sm:max-w-md">
                  <DialogHeader>
                    <DialogTitle className="font-display text-lg">Cancel this order?</DialogTitle>
                    <DialogDescription className="leading-relaxed">
                      {order.status === "Paid"
                        ? "The payment will be refunded, the tickets invalidated and the inventory released back to sale."
                        : "The reserved inventory will be released immediately. This cannot be undone."}
                    </DialogDescription>
                  </DialogHeader>
                  <Textarea
                    className="min-h-20 rounded-xl"
                    placeholder="Reason (optional — shown on the refund notification)"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                  />
                  <DialogFooter className="flex-col gap-2 sm:flex-row">
                    <Button variant="outline" className="btn-press rounded-xl" onClick={() => setCancelOpen(false)}>
                      Keep order
                    </Button>
                    <Button variant="destructive" className="btn-press rounded-xl" disabled={cancelMutation.isPending} onClick={() => cancelMutation.mutate()}>
                      {cancelMutation.isPending ? "Canceling…" : "Cancel order"}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            )}
          </div>
        </div>
      </section>

      <div className="mt-6 grid gap-4 sm:gap-6 lg:grid-cols-2">
        <Card className="animate-fade-up rounded-2xl border-border/70 stagger-1">
          <CardHeader className="border-b border-border/60 bg-muted/30 pb-3">
            <CardTitle className="font-display text-[15px]">Payment summary</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2.5 p-4 text-sm sm:p-5">
            <div className="flex justify-between gap-3">
              <span className="text-muted-foreground">Placed</span>
              <span className="text-right font-mono text-xs">{formatDateTime(order.createdAtUtc)}</span>
            </div>
            <div className="flex justify-between gap-3">
              <span className="text-muted-foreground">Subtotal</span>
              <span className="font-mono tabular-nums">{formatMoney(order.totalPrice, order.currency)}</span>
            </div>
            <div className="flex justify-between gap-3">
              <span className="text-muted-foreground">Discount</span>
              <span className="font-mono text-primary tabular-nums">−{formatMoney(order.discountAmount, order.currency)}</span>
            </div>
            <Separator />
            <div className="flex items-baseline justify-between font-semibold">
              <span>Net paid</span>
              <span className="font-display text-lg tabular-nums">{formatMoney(net, order.currency)}</span>
            </div>
          </CardContent>
        </Card>

        <Card className="animate-fade-up rounded-2xl border-border/70 stagger-2">
          <CardHeader className="border-b border-border/60 bg-muted/30 pb-3">
            <CardTitle className="font-display text-[15px]">Items · {(order.orderItems ?? []).length}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2.5 p-4 text-sm sm:p-5">
            {(order.orderItems ?? []).length === 0 && (
              <p className="text-muted-foreground">No items on this order.</p>
            )}
            {(order.orderItems ?? []).map((item) => (
              <div key={item.orderItemId} className="flex items-center justify-between gap-3 rounded-xl bg-muted/40 px-3 py-2.5">
                <div className="min-w-0">
                  <p className="font-semibold tabular-nums">{item.quantity} × ticket</p>
                  <p className="truncate font-mono text-[11px] text-muted-foreground">{item.ticketTypeId}</p>
                </div>
                <span className="shrink-0 font-mono text-[13px] tabular-nums">
                  {formatMoney(item.price, item.currency)}
                </span>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      <h2 className="label-micro mt-8 mb-3 flex items-center gap-2">
        <Ticket className="size-3.5" /> Tickets · {ticketsQuery.data?.length ?? 0}
      </h2>
      {ticketsQuery.isPending ? (
        <p className="animate-pulse text-sm text-muted-foreground">Loading tickets…</p>
      ) : ticketsQuery.data && ticketsQuery.data.length > 0 ? (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {ticketsQuery.data.map((ticket, i) => (
            <Card
              key={ticket.id}
              className="animate-fade-up card-lift relative overflow-hidden rounded-2xl border-dashed"
              style={{ animationDelay: `${Math.min(i, 6) * 60}ms` }}
            >
              <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-primary via-fuchsia-500 to-orange-400" />
              <CardContent className="p-4 sm:p-5">
                <div className="flex items-center justify-between">
                  <p className="flex items-center gap-2 font-display text-sm font-bold">
                    <span className="flex size-8 items-center justify-center rounded-lg bg-primary/10"><Ticket className="size-4 text-primary" /></span>
                    Ticket
                  </p>
                  <span className="rounded-full bg-muted px-2 py-1 font-mono text-[10px] font-bold tracking-widest">
                    {ticket.code.slice(-6).toUpperCase()}
                  </span>
                </div>
                <p className="mt-3 truncate rounded-lg bg-muted/50 px-2.5 py-2 font-mono text-xs">{ticket.code}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="rounded-2xl border border-dashed p-8 text-center text-sm leading-relaxed text-muted-foreground">
          Tickets appear here once the order is paid and fulfilled — usually within seconds of checkout.
        </div>
      )}
    </div>
  )
}
