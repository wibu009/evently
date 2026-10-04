import { useState } from "react"
import { Link } from "react-router"
import { useQuery } from "@tanstack/react-query"
import { ChevronRight, ReceiptText } from "lucide-react"

import { ordersApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader, PaginationControls, StatusBadge } from "@/components/shared"
import { Card, CardContent } from "@/components/ui/card"
import { formatMoney, formatRelative } from "@/lib/format"

export function OrdersPage() {
  const [page, setPage] = useState(1)

  const ordersQuery = useQuery({
    queryKey: ["orders", page],
    queryFn: ({ signal }) => ordersApi.list(page, 10, signal)
  })

  if (ordersQuery.isPending) {
    return (
      <>
        <PageHeader title="Orders" eyebrow="Purchase history" description="Every order you placed, most recent first." />
        <LoadingState rows={4} />
      </>
    )
  }

  if (ordersQuery.isError) {
    return (
      <>
        <PageHeader title="Orders" eyebrow="Purchase history" />
        <ErrorState message={String(ordersQuery.error)} onRetry={() => ordersQuery.refetch()} />
      </>
    )
  }

  const data = ordersQuery.data

  return (
    <div>
      <PageHeader
        title="Orders"
        eyebrow={`Purchase history · ${data.totalCount} total`}
        description="Every order you placed, most recent first. Open one for items, tickets and cancellation."
      />

      {data.orders.length === 0 ? (
        <EmptyState
          title="No orders yet"
          description="Check out a cart to place your first order — it takes under a minute."
          actionLabel="Discover events"
          actionTo="/"
          icon={<ReceiptText className="size-5" />}
        />
      ) : (
        <>
          <div className="space-y-2.5">
            {data.orders.map((order, i) => (
              <Link
                key={order.id}
                to={`/orders/${order.id}`}
                className="group animate-fade-up block focus-visible:outline-none"
                style={{ animationDelay: `${Math.min(i, 8) * 50}ms` }}
              >
                <Card className="card-lift rounded-2xl border-border/70 bg-card/70">
                  <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between sm:p-5">
                    <div className="flex min-w-0 items-center gap-3.5">
                      <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 font-display text-sm font-bold text-primary">
                        #{String(data.totalCount - (data.page - 1) * data.pageSize - i).padStart(2, "0")}
                      </div>
                      <div className="min-w-0">
                        <p className="truncate font-mono text-xs text-muted-foreground">{order.id}</p>
                        <p className="mt-0.5 text-[13px] text-muted-foreground">
                          {formatRelative(order.createdAtUtc)}
                          {order.discountAmount > 0 && (
                            <span className="ml-2 rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary">
                              −{formatMoney(order.discountAmount, order.currency)} promo
                            </span>
                          )}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center justify-between gap-3 border-t border-dashed border-border/70 pt-3 sm:justify-end sm:border-0 sm:pt-0 sm:pl-4">
                      <StatusBadge status={order.status} />
                      <span className="font-display text-base font-bold tabular-nums sm:min-w-20 sm:text-right">
                        {formatMoney(order.totalPrice - order.discountAmount, order.currency)}
                      </span>
                      <ChevronRight className="size-4 shrink-0 text-muted-foreground transition-transform duration-300 group-hover:translate-x-1 group-hover:text-primary" />
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
