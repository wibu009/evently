import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { Plus, Tag } from "lucide-react"

import { promoCodesApi } from "@/api/endpoints"
import { EmptyState, ErrorState, LoadingState, PageHeader, PaginationControls } from "@/components/shared"
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select"
import { ApiError } from "@/lib/http"
import { formatDateTime, formatMoney } from "@/lib/format"

interface CreateDialogProps {
  onDone: () => void
}

function CreatePromoCodeDialog({ onDone }: CreateDialogProps) {
  const [open, setOpen] = useState(false)
  const [code, setCode] = useState("")
  const [discountType, setDiscountType] = useState("0")
  const [discountValue, setDiscountValue] = useState("10")
  const [currency, setCurrency] = useState("USD")
  const [maxRedemptions, setMaxRedemptions] = useState("100")
  const [validUntilUtc, setValidUntilUtc] = useState("")

  const createMutation = useMutation({
    mutationFn: () =>
      promoCodesApi.create({
        code: code.trim().toUpperCase(),
        discountType: Number(discountType),
        discountValue: Number(discountValue),
        currency: currency.toUpperCase(),
        maxRedemptions: maxRedemptions.trim() ? Number(maxRedemptions) : null,
        validFromUtc: null,
        validUntilUtc: validUntilUtc ? new Date(validUntilUtc).toISOString() : null
      }),
    onSuccess: () => {
      toast.success("Promo code created")
      setOpen(false)
      setCode("")
      onDone()
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not create the promo code")
  })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <Button size="sm" onClick={() => setOpen(true)}>
        <Plus className="size-4" /> New promo code
      </Button>
      <DialogContent className="rounded-2xl">
        <DialogHeader>
          <DialogTitle className="font-display">New promo code</DialogTitle>
          <DialogDescription>
            Percentage discounts are capped at 100; fixed amounts are capped at the order total at checkout.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-3">
          <div className="space-y-1.5">
            <Label>Code</Label>
            <Input
              value={code}
              onChange={(e) => setCode(e.target.value.toUpperCase())}
              placeholder="SUMMER10"
              className="font-mono uppercase"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Discount type</Label>
              <Select value={discountType} onValueChange={setDiscountType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">Percentage (%)</SelectItem>
                  <SelectItem value="1">Fixed amount</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Value</Label>
              <Input type="number" min="0" step="0.01" value={discountValue} onChange={(e) => setDiscountValue(e.target.value)} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Currency</Label>
              <Input value={currency} onChange={(e) => setCurrency(e.target.value.toUpperCase())} maxLength={3} className="font-mono" />
            </div>
            <div className="space-y-1.5">
              <Label>Max redemptions (optional)</Label>
              <Input type="number" min="1" value={maxRedemptions} onChange={(e) => setMaxRedemptions(e.target.value)} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Valid until (UTC, optional)</Label>
            <Input type="datetime-local" value={validUntilUtc} onChange={(e) => setValidUntilUtc(e.target.value)} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button
            disabled={createMutation.isPending || code.trim() === "" || discountValue === ""}
            onClick={() => createMutation.mutate()}
          >
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export function PromoCodesAdminPage() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)

  const promoCodesQuery = useQuery({
    queryKey: ["promo-codes", page],
    queryFn: ({ signal }) => promoCodesApi.list(page, 10, signal)
  })

  const deactivateMutation = useMutation({
    mutationFn: promoCodesApi.deactivate,
    onSuccess: () => {
      toast.success("Promo code deactivated")
      void queryClient.invalidateQueries({ queryKey: ["promo-codes"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not deactivate the promo code")
  })

  return (
    <div>
      <PageHeader
        title="Promo codes"
        description="Redemptions are only counted for paid orders — abandoned checkouts never burn a code."
        actions={<CreatePromoCodeDialog onDone={() => void queryClient.invalidateQueries({ queryKey: ["promo-codes"] })} />}
      />

      {promoCodesQuery.isPending ? (
        <LoadingState />
      ) : promoCodesQuery.isError ? (
        <ErrorState message={String(promoCodesQuery.error)} onRetry={() => promoCodesQuery.refetch()} />
      ) : promoCodesQuery.data.promoCodes.length === 0 ? (
        <EmptyState title="No promo codes yet" description="Create your first discount code." />
      ) : (
        <>
          <div className="space-y-3">
            {promoCodesQuery.data.promoCodes.map((promoCode) => {
              const active = !promoCode.validUntilUtc || new Date(promoCode.validUntilUtc) > new Date()

              return (
                <Card key={promoCode.id} className="rounded-2xl">
                  <CardContent className="flex flex-wrap items-center justify-between gap-3 p-4">
                    <div className="flex items-center gap-3">
                      <div className="flex size-9 items-center justify-center rounded-sm bg-primary/15">
                        <Tag className="size-4 text-primary" />
                      </div>
                      <div>
                        <p className="font-mono font-semibold">{promoCode.code}</p>
                        <p className="text-xs text-muted-foreground">
                          {promoCode.discountType === "Percentage"
                            ? `${promoCode.discountValue}% off`
                            : `${formatMoney(promoCode.discountValue, promoCode.currency)} off`}
                          {" · "}
                          {promoCode.timesRedeemed} redeemed
                          {promoCode.maxRedemptions ? ` / ${promoCode.maxRedemptions}` : ""}
                          {promoCode.validUntilUtc ? ` · until ${formatDateTime(promoCode.validUntilUtc)}` : " · no expiry"}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className={active ? "text-xs font-medium text-success" : "text-xs text-muted-foreground"}>
                        {active ? "Active" : "Inactive"}
                      </span>
                      {active && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={deactivateMutation.isPending}
                          onClick={() => deactivateMutation.mutate(promoCode.id)}
                        >
                          Deactivate
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              )
            })}
          </div>
          <PaginationControls
            page={promoCodesQuery.data.page}
            pageSize={promoCodesQuery.data.pageSize}
            totalCount={promoCodesQuery.data.totalCount}
            onPageChange={setPage}
          />
        </>
      )}
    </div>
  )
}

