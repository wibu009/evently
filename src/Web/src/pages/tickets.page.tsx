import { useState } from "react"
import { Link } from "react-router"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ArrowUpRight, QrCode, Send, Ticket as TicketIcon } from "lucide-react"

import { ordersApi, ticketsApi } from "@/api/endpoints"
import { TicketQr } from "@/components/ticket-qr"
import { EmptyState, ErrorState, LoadingState, PageHeader } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/http"
import { formatDateTime } from "@/lib/format"

function TransferDialog({ ticketId, onDone }: { ticketId: string; onDone: () => void }) {
  const [open, setOpen] = useState(false)
  const [toCustomerId, setToCustomerId] = useState("")

  const transferMutation = useMutation({
    mutationFn: () => ticketsApi.transfer(ticketId, toCustomerId.trim()),
    onSuccess: () => {
      toast.success("Ticket transferred", { description: "The new owner can now check in with it." })
      setOpen(false)
      setToCustomerId("")
      onDone()
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not transfer the ticket")
  })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <Button variant="outline" size="sm" className="btn-press rounded-xl" onClick={() => setOpen(true)}>
        <Send className="size-3.5" /> Transfer
      </Button>
      <DialogContent className="rounded-2xl sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="font-display text-lg">Transfer ticket</DialogTitle>
          <DialogDescription className="leading-relaxed">
            Paste the recipient's customer ID (they’ll find it on their profile page). You lose access immediately.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-1.5">
          <Label>Recipient customer ID</Label>
          <Input
            placeholder="01J…"
            value={toCustomerId}
            onChange={(e) => setToCustomerId(e.target.value)}
            className="h-11 rounded-xl font-mono text-xs"
          />
        </div>
        <DialogFooter className="flex-col gap-2 sm:flex-row">
          <Button variant="outline" className="btn-press rounded-xl" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button
            className="btn-press rounded-xl"
            disabled={transferMutation.isPending || toCustomerId.trim().length === 0}
            onClick={() => transferMutation.mutate()}
          >
            {transferMutation.isPending ? "Transferring…" : "Transfer"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

interface TicketWithOrder {
  ticketId: string
  code: string
  createdAtUtc: string
  orderId: string
  orderStatus: string
}

export function TicketsPage() {
  const queryClient = useQueryClient()

  const ordersQuery = useQuery({
    queryKey: ["orders", 1],
    queryFn: ({ signal }) => ordersApi.list(1, 25, signal)
  })

  const orderIds = ordersQuery.data?.orders.map((order) => order.id) ?? []

  const ticketQueries = useQuery({
    queryKey: ["tickets", "all", orderIds.join(",")],
    queryFn: async ({ signal }) => {
      const results = await Promise.all(orderIds.map((orderId) => ticketsApi.forOrder(orderId, signal)))
      const orders = ordersQuery.data?.orders ?? []
      const statusByOrder = new Map(orders.map((order) => [order.id, order.status]))

      return results
        .flat()
        .map<TicketWithOrder>((ticket) => ({
          ticketId: ticket.id,
          code: ticket.code,
          createdAtUtc: ticket.createdAtUtc,
          orderId: ticket.orderId,
          orderStatus: statusByOrder.get(ticket.orderId) ?? "Unknown"
        }))
    },
    enabled: orderIds.length > 0
  })

  if (ordersQuery.isPending) {
    return (
      <>
        <PageHeader title="Tickets" eyebrow="Your passes" />
        <LoadingState rows={6} />
      </>
    )
  }

  if (ordersQuery.isError) {
    return (
      <>
        <PageHeader title="Tickets" eyebrow="Your passes" />
        <ErrorState message={String(ordersQuery.error)} onRetry={() => ordersQuery.refetch()} />
      </>
    )
  }

  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: ["tickets"] })
    void queryClient.invalidateQueries({ queryKey: ["orders"] })
  }

  return (
    <div>
      <PageHeader
        title="Tickets"
        eyebrow={`Your passes · ${ticketQueries.data?.length ?? 0}`}
        description="Issued tickets live here. Show the code at the door — or transfer it to a friend."
      />

      {orderIds.length === 0 ? (
        <EmptyState
          title="No tickets yet"
          description="Place an order to receive your first tickets."
          actionLabel="Discover events"
          actionTo="/"
          icon={<TicketIcon className="size-5" />}
        />
      ) : ticketQueries.isPending ? (
        <LoadingState rows={6} />
      ) : ticketQueries.isError ? (
        <ErrorState message={String(ticketQueries.error)} onRetry={refresh} />
      ) : ticketQueries.data.length === 0 ? (
        <EmptyState title="No tickets yet" description="Tickets are issued when an order is paid and fulfilled — usually within seconds." icon={<QrCode className="size-5" />} />
      ) : (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {ticketQueries.data.map((ticket, i) => (
            <Card
              key={ticket.ticketId}
              className="animate-fade-up card-lift group relative overflow-hidden rounded-2xl border-border/70"
              style={{ animationDelay: `${Math.min(i, 8) * 55}ms` }}
            >
              <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-primary via-fuchsia-500 to-orange-400" />
              <CardContent className="p-4 sm:p-5">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0 flex-1">
                    <p className="label-micro">Ticket</p>
                    <p className="mt-1.5 truncate rounded-lg bg-muted/60 px-2.5 py-2 font-mono text-[13px] font-semibold tracking-tight">{ticket.code}</p>
                    <p className="mt-2 text-xs text-muted-foreground">Issued {formatDateTime(ticket.createdAtUtc)}</p>
                  </div>
                  <TicketQr code={ticket.code} className="shrink-0" />
                </div>
                <div className="mt-4 flex items-center justify-between border-t border-dashed border-border/70 pt-3">
                  <Link to={`/orders/${ticket.orderId}`} className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline">
                    View order <ArrowUpRight className="size-3" />
                  </Link>
                  <TransferDialog ticketId={ticket.ticketId} onDone={refresh} />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
