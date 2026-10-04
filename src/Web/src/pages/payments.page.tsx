import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { CreditCard, Undo2 } from "lucide-react"

import { paymentsApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader, PaginationControls, StatusBadge } from "@/components/shared"
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
import { formatDateTime, formatMoney } from "@/lib/format"
import { useHasPermission } from "@/lib/use-auth"

function RefundDialog({ paymentId, currency, maxAmount }: { paymentId: string; currency: string; maxAmount: number }) {
  const queryClient = useQueryClient()
  const [open, setOpen] = useState(false)
  const [amount, setAmount] = useState("")

  const refundMutation = useMutation({
    mutationFn: () => paymentsApi.refund(paymentId, amount.trim() ? Number(amount) : undefined),
    onSuccess: () => {
      toast.success("Refund recorded", { description: "The gateway refund runs through the outbox." })
      setOpen(false)
      setAmount("")
      void queryClient.invalidateQueries({ queryKey: ["payments"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not refund the payment")
  })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <Button variant="outline" size="sm" className="btn-press rounded-xl" onClick={() => setOpen(true)}>
        <Undo2 className="size-3.5" /> Refund
      </Button>
      <DialogContent className="rounded-2xl sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="font-display text-lg">Refund payment</DialogTitle>
          <DialogDescription className="leading-relaxed">
            Leave empty for a full refund of {formatMoney(maxAmount, currency)}. Partial refunds keep the order paid until fully refunded.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-1.5">
          <Label>Amount ({currency})</Label>
          <Input
            type="number"
            step="0.01"
            min="0"
            max={maxAmount}
            placeholder={String(maxAmount)}
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            className="h-11 rounded-xl font-mono"
          />
        </div>
        <DialogFooter className="flex-col gap-2 sm:flex-row">
          <Button variant="outline" className="btn-press rounded-xl" onClick={() => setOpen(false)}>
            Close
          </Button>
          <Button variant="destructive" className="btn-press rounded-xl" disabled={refundMutation.isPending} onClick={() => refundMutation.mutate()}>
            {refundMutation.isPending ? "Refunding…" : "Refund"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export function PaymentsPage() {
  const [page, setPage] = useState(1)
  const has = useHasPermission()

  const paymentsQuery = useQuery({
    queryKey: ["payments", page],
    queryFn: ({ signal }) => paymentsApi.list(page, 10, signal)
  })

  if (paymentsQuery.isPending) {
    return (
      <>
        <PageHeader title="Payments" eyebrow="Ledger" description="Your payment ledger — charges, statuses and refunds." />
        <LoadingState rows={4} />
      </>
    )
  }

  if (paymentsQuery.isError) {
    return (
      <>
        <PageHeader title="Payments" eyebrow="Ledger" />
        <ErrorState message={String(paymentsQuery.error)} onRetry={() => paymentsQuery.refetch()} />
      </>
    )
  }

  const data = paymentsQuery.data

  return (
    <div>
      <PageHeader
        title="Payments"
        eyebrow={`Ledger · ${data.totalCount} total`}
        description="Charges, gateway references, statuses and refunds — most recent first."
      />

      {data.payments.length === 0 ? (
        <EmptyState
          title="No payments yet"
          description="Payments appear here once you check out. Your money trail lives here."
          actionLabel="Discover events"
          actionTo="/"
          icon={<CreditCard className="size-5" />}
        />
      ) : (
        <>
          <div className="space-y-2.5">
            {data.payments.map((payment, i) => (
              <Card
                key={payment.id}
                className="animate-fade-up card-lift rounded-2xl border-border/70 bg-card/70"
                style={{ animationDelay: `${Math.min(i, 8) * 50}ms` }}
              >
                <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:p-5">
                  <div className="flex min-w-0 items-start gap-3.5">
                    <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                      <CreditCard className="size-5 text-primary" />
                    </div>
                    <div className="min-w-0">
                      <p className="truncate font-mono text-xs text-muted-foreground">{payment.id}</p>
                      <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
                        {formatDateTime(payment.createdAtUtc)}
                        {payment.paidAtUtc ? ` · paid ${formatDateTime(payment.paidAtUtc)}` : ""}
                        {payment.refundedAtUtc ? ` · refunded ${formatDateTime(payment.refundedAtUtc)}` : ""}
                      </p>
                      {payment.transactionReference && (
                        <p className="mt-0.5 truncate font-mono text-[11px] text-primary/80">↳ {payment.transactionReference}</p>
                      )}
                      {payment.failureReason && <p className="mt-1 text-xs font-medium text-destructive">{payment.failureReason}</p>}
                    </div>
                  </div>
                  <div className="flex items-center justify-between gap-3 border-t border-dashed border-border/70 pt-3 sm:justify-end sm:border-0 sm:pt-0">
                    <StatusBadge status={payment.status} />
                    <div className="text-right sm:min-w-24">
                      <p className="font-display text-base font-bold tabular-nums">{formatMoney(payment.amount, payment.currency)}</p>
                      {(payment.amountRefunded ?? 0) > 0 && (
                        <p className="text-xs font-medium text-primary tabular-nums">−{formatMoney(payment.amountRefunded ?? 0, payment.currency)} refunded</p>
                      )}
                    </div>
                    {has("payments:refund") && payment.status === "Succeeded" && (
                      <RefundDialog
                        paymentId={payment.id}
                        currency={payment.currency}
                        maxAmount={payment.amount - (payment.amountRefunded ?? 0)}
                      />
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          <PaginationControls page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
