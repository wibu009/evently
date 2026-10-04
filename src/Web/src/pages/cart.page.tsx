import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useNavigate } from "react-router"
import { toast } from "sonner"
import { ArrowRight, ShieldCheck, ShoppingCart, Tag, Timer, Trash2 } from "lucide-react"

import { cartApi, ordersApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Separator } from "@/components/ui/separator"
import { ApiError } from "@/lib/http"
import { formatMoney } from "@/lib/format"

export function CartPage() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [promoCode, setPromoCode] = useState("")

  const cartQuery = useQuery({
    queryKey: ["cart"],
    queryFn: ({ signal }) => cartApi.get(signal)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ["cart"] })
  }

  const removeMutation = useMutation({
    mutationFn: cartApi.removeItem,
    onSuccess: invalidate,
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not remove the item")
  })

  const clearMutation = useMutation({
    mutationFn: cartApi.clear,
    onSuccess: () => {
      toast.info("Cart cleared")
      invalidate()
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not clear the cart")
  })

  const checkoutMutation = useMutation({
    mutationFn: () => ordersApi.checkout(promoCode.trim() || undefined),
    onSuccess: (orderId) => {
      toast.success("Order created — tickets reserved for 15 minutes", {
        description: "Complete payment before the hold expires."
      })
      void queryClient.invalidateQueries({ queryKey: ["cart"] })
      void queryClient.invalidateQueries({ queryKey: ["orders"] })
      void navigate(`/orders/${orderId}`)
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Checkout failed")
  })

  if (cartQuery.isPending) {
    return (
      <>
        <PageHeader title="Your cart" eyebrow="Checkout" />
        <LoadingState rows={3} />
      </>
    )
  }

  if (cartQuery.isError) {
    return (
      <>
        <PageHeader title="Your cart" eyebrow="Checkout" />
        <ErrorState message={String(cartQuery.error)} onRetry={() => cartQuery.refetch()} />
      </>
    )
  }

  const cart = cartQuery.data
  const total = cart.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  const currency = cart.items[0]?.currency ?? "USD"
  const count = cart.items.reduce((s, i) => s + i.quantity, 0)

  return (
    <div>
      <PageHeader
        title="Your cart"
        eyebrow={`Checkout · ${count} ${count === 1 ? "ticket" : "tickets"}`}
        description="Reserved tickets are held for 15 minutes once you check out."
        actions={
          cart.items.length > 0 ? (
            <Button variant="outline" size="sm" className="btn-press rounded-xl" disabled={clearMutation.isPending} onClick={() => clearMutation.mutate()}>
              <Trash2 className="size-4" /> Clear
            </Button>
          ) : undefined
        }
      />

      {cart.items.length === 0 ? (
        <EmptyState
          title="Your cart is empty"
          description="Browse the catalog and grab some tickets — they’ll show up here."
          actionLabel="Discover events"
          actionTo="/"
          icon={<ShoppingCart className="size-5" />}
        />
      ) : (
        <div className="grid gap-6 lg:grid-cols-[1fr_360px] lg:gap-8">
          <div className="min-w-0 space-y-3">
            {cart.items.map((item, i) => (
              <Card
                key={item.ticketTypeId}
                className="animate-fade-up card-lift rounded-2xl border-border/70 bg-card/70"
                style={{ animationDelay: `${i * 60}ms` }}
              >
                <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:p-5">
                  <div className="flex items-center gap-3.5">
                    <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-primary/25 to-fuchsia-500/15 font-display text-sm font-bold text-primary">
                      {item.quantity}×
                    </div>
                    <div className="min-w-0">
                      <p className="truncate font-display text-[15px] font-bold tracking-tight">General admission ticket</p>
                      <p className="mt-0.5 truncate font-mono text-[11px] text-muted-foreground">{item.ticketTypeId}</p>
                      <p className="mt-0.5 text-xs text-muted-foreground tabular-nums">
                        {formatMoney(item.price, item.currency)} each
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center justify-between gap-3 border-t border-dashed border-border/70 pt-3 sm:justify-end sm:border-0 sm:pt-0">
                    <p className="font-display text-base font-bold tabular-nums sm:text-right">{formatMoney(item.price * item.quantity, item.currency)}</p>
                    <Button
                      variant="ghost"
                      size="icon"
                      aria-label="Remove item"
                      className="btn-press rounded-lg text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                      disabled={removeMutation.isPending}
                      onClick={() => removeMutation.mutate(item.ticketTypeId)}
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}

            <div className="grid gap-3 sm:grid-cols-2">
              <div className="flex items-center gap-2.5 rounded-2xl border border-border/60 bg-card/40 p-3.5 text-xs text-muted-foreground">
                <Timer className="size-4 shrink-0 text-warning" />
                <span>15-minute hold starts at checkout — not before.</span>
              </div>
              <div className="flex items-center gap-2.5 rounded-2xl border border-border/60 bg-card/40 p-3.5 text-xs text-muted-foreground">
                <ShieldCheck className="size-4 shrink-0 text-success" />
                <span>Full refund if the event is canceled.</span>
              </div>
            </div>
          </div>

          <Card className="animate-fade-up h-fit rounded-2xl border-border/70 shadow-xl shadow-primary/[0.06] lg:sticky lg:top-20 stagger-2">
            <CardHeader className="border-b border-border/60 bg-muted/30 pb-4">
              <CardTitle className="font-display text-base">Order summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 p-4 sm:p-5">
              <div>
                <label htmlFor="promo" className="label-micro mb-1.5 flex items-center gap-1.5">
                  <Tag className="size-3" /> Promo code
                </label>
                <Input
                  id="promo"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, ""))}
                  placeholder="SUMMER10"
                  maxLength={20}
                  className="h-11 rounded-xl font-mono tracking-widest uppercase"
                />
              </div>
              <Separator />
              <div className="space-y-1.5 text-sm">
                <div className="flex justify-between text-muted-foreground">
                  <span>Subtotal</span>
                  <span className="font-mono tabular-nums">{formatMoney(total, currency)}</span>
                </div>
                <div className="flex justify-between text-muted-foreground">
                  <span>Fees</span>
                  <span className="font-mono tabular-nums">$0.00</span>
                </div>
                <div className="flex items-baseline justify-between pt-1">
                  <span className="font-semibold">Total</span>
                  <span className="font-display text-2xl font-bold tabular-nums">{formatMoney(total, currency)}</span>
                </div>
              </div>
            </CardContent>
            <CardFooter className="p-4 pt-0 sm:p-5 sm:pt-0">
              <Button
                className="btn-press h-11 w-full rounded-xl text-[15px] font-bold shadow-lg shadow-primary/25"
                disabled={checkoutMutation.isPending}
                onClick={() => checkoutMutation.mutate()}
              >
                {checkoutMutation.isPending ? "Reserving…" : (<>Check out <ArrowRight className="size-4" /></>)}
              </Button>
            </CardFooter>
          </Card>
        </div>
      )}
    </div>
  )
}
