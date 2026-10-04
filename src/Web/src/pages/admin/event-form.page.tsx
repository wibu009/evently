import { useEffect, useState } from "react"
import { Link, useNavigate, useParams } from "react-router"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ArrowLeft, Ban, CheckCircle2, Pencil, Plus, Rocket, CalendarClock } from "lucide-react"

import { categoriesApi, eventsApi, statisticsApi, ticketTypesApi } from "@/api/endpoints"
import { ErrorState, LoadingState, PageHeader, StatusBadge } from "@/components/shared"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
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
import { Textarea } from "@/components/ui/textarea"
import { ApiError } from "@/lib/http"
import { formatDateTime, formatMoney, normalizeDateTimeInput } from "@/lib/format"

function LifecycleActions({ eventId, status, onDone }: { eventId: string; status: string; onDone: () => void }) {
  const [rescheduleOpen, setRescheduleOpen] = useState(false)
  const [updateOpen, setUpdateOpen] = useState(false)
  const [startAtUtc, setStartAtUtc] = useState("")
  const [endAtUtc, setEndAtUtc] = useState("")
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [location, setLocation] = useState("")

  const onError = (error: unknown) => toast.error(error instanceof ApiError ? error.detail : "Action failed")
  const invalidate = () => {
    onDone()
    void eventsApi.get(eventId)
  }

  const publishMutation = useMutation({
    mutationFn: () => eventsApi.publish(eventId),
    onSuccess: () => {
      toast.success("Event published — it's now on sale and synced to ticketing")
      invalidate()
    },
    onError
  })

  const completeMutation = useMutation({
    mutationFn: () => eventsApi.complete(eventId),
    onSuccess: () => {
      toast.success("Event completed — removed from the public catalog")
      invalidate()
    },
    onError
  })

  const cancelMutation = useMutation({
    mutationFn: () => eventsApi.cancel(eventId),
    onSuccess: () => {
      toast.success("Event canceled — the cancellation saga refunds all paid orders")
      invalidate()
    },
    onError
  })

  const updateMutation = useMutation({
    mutationFn: () => eventsApi.update(eventId, { title, description, location }),
    onSuccess: () => {
      toast.success("Event updated — changes propagate to ticketing and attendance")
      setUpdateOpen(false)
      invalidate()
    },
    onError
  })

  const rescheduleMutation = useMutation({
    mutationFn: () =>
      eventsApi.reschedule(eventId, {
        startAtUtc: normalizeDateTimeInput(startAtUtc),
        endAtUtc: endAtUtc ? normalizeDateTimeInput(endAtUtc) : null
      }),
    onSuccess: () => {
      toast.success("Event rescheduled")
      setRescheduleOpen(false)
      invalidate()
    },
    onError
  })

  return (
    <div className="flex flex-wrap gap-2">
      {status === "Draft" && (
        <Button size="sm" disabled={publishMutation.isPending} onClick={() => publishMutation.mutate()}>
          <Rocket className="size-4" /> Publish
        </Button>
      )}
      {status === "Published" && (
        <Button size="sm" variant="outline" disabled={completeMutation.isPending} onClick={() => completeMutation.mutate()}>
          <CheckCircle2 className="size-4" /> Complete
        </Button>
      )}
      {status !== "Canceled" && status !== "Completed" && (
        <>
          <Button size="sm" variant="outline" onClick={() => setUpdateOpen(true)}>
            <Pencil className="size-4" /> Edit details
          </Button>
          <Button size="sm" variant="outline" onClick={() => setRescheduleOpen(true)}>
            <CalendarClock className="size-4" /> Reschedule
          </Button>
          <Button size="sm" variant="destructive" disabled={cancelMutation.isPending} onClick={() => cancelMutation.mutate()}>
            <Ban className="size-4" /> Cancel event
          </Button>
        </>
      )}

      <Dialog open={updateOpen} onOpenChange={setUpdateOpen}>
        <DialogContent className="rounded-2xl">
          <DialogHeader>
            <DialogTitle className="font-display">Edit event details</DialogTitle>
            <DialogDescription>Title, description and location propagate to all modules.</DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div className="space-y-1.5">
              <Label>Title</Label>
              <Input value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Description</Label>
              <Textarea value={description} onChange={(e) => setDescription(e.target.value)} className="min-h-20" />
            </div>
            <div className="space-y-1.5">
              <Label>Location</Label>
              <Input value={location} onChange={(e) => setLocation(e.target.value)} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setUpdateOpen(false)}>
              Cancel
            </Button>
            <Button disabled={updateMutation.isPending} onClick={() => updateMutation.mutate()}>
              Save
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={rescheduleOpen} onOpenChange={setRescheduleOpen}>
        <DialogContent className="rounded-2xl">
          <DialogHeader>
            <DialogTitle className="font-display">Reschedule event</DialogTitle>
            <DialogDescription>Only future dates are allowed; canceled and completed events cannot be rescheduled.</DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div className="space-y-1.5">
              <Label>Starts (UTC)</Label>
              <Input type="datetime-local" value={startAtUtc} onChange={(e) => setStartAtUtc(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Ends (UTC, optional)</Label>
              <Input type="datetime-local" value={endAtUtc} onChange={(e) => setEndAtUtc(e.target.value)} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setRescheduleOpen(false)}>
              Cancel
            </Button>
            <Button disabled={rescheduleMutation.isPending || startAtUtc === ""} onClick={() => rescheduleMutation.mutate()}>
              Reschedule
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

function TicketTypesPanel({ eventId, eventStatus }: { eventId: string; eventStatus: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [name, setName] = useState("")
  const [price, setPrice] = useState("89")
  const [quantity, setQuantity] = useState("100")

  const ticketTypesQuery = useQuery({
    queryKey: ["ticket-types", eventId],
    queryFn: ({ signal }) => ticketTypesApi.listForEvent(eventId, signal)
  })

  const createMutation = useMutation({
    mutationFn: () =>
      ticketTypesApi.create({ eventId, name, price: Number(price), currency: "USD", quantity: Number(quantity) }),
    onSuccess: () => {
      toast.success("Ticket type created")
      setCreateOpen(false)
      setName("")
      void queryClient.invalidateQueries({ queryKey: ["ticket-types", eventId] })
      void queryClient.invalidateQueries({ queryKey: ["event", eventId] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not create the ticket type")
  })

  const priceMutation = useMutation({
    mutationFn: ({ ticketTypeId, newPrice }: { ticketTypeId: string; newPrice: number }) =>
      ticketTypesApi.updatePrice(ticketTypeId, newPrice),
    onSuccess: () => {
      toast.success("Price updated — synced to ticketing")
      void queryClient.invalidateQueries({ queryKey: ["ticket-types", eventId] })
      void queryClient.invalidateQueries({ queryKey: ["event", eventId] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not update the price")
  })

  const designMutation = useMutation({
    mutationFn: ({
      ticketTypeId,
      design
    }: {
      ticketTypeId: string
      design: { color?: string | null; backgroundImageUrl?: string | null }
    }) => ticketTypesApi.updateDesign(ticketTypeId, design),
    onSuccess: () => {
      toast.success("Ticket design updated")
      void queryClient.invalidateQueries({ queryKey: ["ticket-types", eventId] })
      void queryClient.invalidateQueries({ queryKey: ["event", eventId] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not update the design")
  })

  return (
    <Card className="rounded-2xl">
      <CardHeader className="flex-row items-center justify-between pb-2">
        <CardTitle className="font-display text-base">Ticket types</CardTitle>
        <Button size="sm" variant="outline" onClick={() => setCreateOpen(true)}>
          <Plus className="size-3.5" /> Add
        </Button>
      </CardHeader>
      <CardContent className="space-y-2">
        {ticketTypesQuery.data?.ticketTypes.map((ticketType) => (
          <TicketTypeAdminRow
            key={ticketType.ticketTypeId}
            ticketType={ticketType}
            disabled={eventStatus === "Canceled"}
            onPriceChange={(newPrice) => priceMutation.mutate({ ticketTypeId: ticketType.ticketTypeId, newPrice })}
            onDesignChange={(design) => designMutation.mutate({ ticketTypeId: ticketType.ticketTypeId, design })}
          />
        ))}
        {ticketTypesQuery.data?.ticketTypes.length === 0 && (
          <p className="text-sm text-muted-foreground">No ticket types yet — an event needs at least one before it can be published.</p>
        )}
      </CardContent>

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent className="rounded-2xl">
          <DialogHeader>
            <DialogTitle className="font-display">New ticket type</DialogTitle>
          </DialogHeader>
          <div className="space-y-3">
            <div className="space-y-1.5">
              <Label>Name</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="General Admission" />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <Label>Price (USD)</Label>
                <Input type="number" min="0" step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} />
              </div>
              <div className="space-y-1.5">
                <Label>Quantity</Label>
                <Input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setCreateOpen(false)}>
              Cancel
            </Button>
            <Button disabled={createMutation.isPending || name.trim() === ""} onClick={() => createMutation.mutate()}>
              Create
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  )
}

function TicketTypeAdminRow({
  ticketType,
  disabled,
  onPriceChange,
  onDesignChange
}: {
  ticketType: { ticketTypeId: string; name: string; price: number; currency: string; quantity: number; color: string | null; backgroundImageUrl: string | null }
  disabled: boolean
  onPriceChange: (newPrice: number) => void
  onDesignChange: (design: { color?: string | null; backgroundImageUrl?: string | null }) => void
}) {
  const [editing, setEditing] = useState(false)
  const [price, setPrice] = useState(String(ticketType.price))
  const [designOpen, setDesignOpen] = useState(false)
  const [color, setColor] = useState(ticketType.color ?? "")
  const [backgroundImageUrl, setBackgroundImageUrl] = useState(ticketType.backgroundImageUrl ?? "")

  return (
    <div className="rounded-xl border border-border/70 p-3 text-sm">
      <div className="flex items-center justify-between gap-2">
        <div className="flex min-w-0 items-center gap-2.5">
          <span
            aria-hidden
            className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary/10 font-display text-xs font-bold text-primary"
            style={
              ticketType.backgroundImageUrl
                ? { background: `center/cover url(${ticketType.backgroundImageUrl})` }
                : ticketType.color
                  ? { background: ticketType.color }
                  : undefined
            }
          >
            {!ticketType.backgroundImageUrl && !ticketType.color && ticketType.name.slice(0, 1).toUpperCase()}
          </span>
          <div className="min-w-0">
            <p className="truncate font-medium">{ticketType.name}</p>
            <p className="text-xs text-muted-foreground tabular-nums">{ticketType.quantity} available</p>
          </div>
        </div>
        {editing ? (
          <div className="flex shrink-0 items-center gap-2">
            <Input type="number" step="0.01" min="0" className="h-8 w-24 rounded-lg" value={price} onChange={(e) => setPrice(e.target.value)} />
            <Button
              size="sm"
              className="btn-press h-8 rounded-lg"
              onClick={() => {
                onPriceChange(Number(price))
                setEditing(false)
              }}
            >
              Save
            </Button>
          </div>
        ) : (
          <div className="flex shrink-0 items-center gap-1">
            <span className="mr-1 font-mono text-[13px] tabular-nums">{formatMoney(ticketType.price, ticketType.currency)}</span>
            <Button size="sm" variant="ghost" className="btn-press h-8 rounded-lg px-2 text-xs" disabled={disabled} onClick={() => setEditing(true)}>
              Price
            </Button>
            <Button
              size="sm"
              variant="ghost"
              className="btn-press h-8 rounded-lg px-2 text-xs"
              disabled={disabled}
              onClick={() => {
                setColor(ticketType.color ?? "")
                setBackgroundImageUrl(ticketType.backgroundImageUrl ?? "")
                setDesignOpen((v) => !v)
              }}
            >
              Design
            </Button>
          </div>
        )}
      </div>
      {designOpen && !editing && (
        <div className="animate-fade-in mt-3 space-y-2.5 rounded-xl bg-muted/40 p-3">
          <div className="grid grid-cols-[auto_1fr] items-center gap-2">
            <Label className="text-xs">Face color</Label>
            <div className="flex items-center gap-2">
              <input
                type="color"
                value={/^#[0-9a-fA-F]{6}$/.test(color) ? color : "#7C3AED"}
                onChange={(e) => setColor(e.target.value.toUpperCase())}
                className="h-8 w-10 cursor-pointer rounded-md border border-input bg-transparent p-0.5"
                aria-label="Ticket face color"
              />
              <Input
                value={color}
                onChange={(e) => setColor(e.target.value)}
                placeholder="#7C3AED (optional)"
                maxLength={7}
                className="h-8 rounded-lg font-mono text-xs uppercase"
              />
              {(color || ticketType.color) && (
                <Button size="sm" variant="ghost" className="h-8 rounded-lg px-2 text-xs" onClick={() => setColor("")}>
                  Clear
                </Button>
              )}
            </div>
          </div>
          <div className="grid grid-cols-[auto_1fr] items-center gap-2">
            <Label className="text-xs">Backdrop</Label>
            <Input
              value={backgroundImageUrl}
              onChange={(e) => setBackgroundImageUrl(e.target.value)}
              placeholder="https://…/ticket-bg.jpg (optional)"
              className="h-8 rounded-lg text-xs"
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button size="sm" variant="ghost" className="h-8 rounded-lg text-xs" onClick={() => setDesignOpen(false)}>
              Cancel
            </Button>
            <Button
              size="sm"
              className="btn-press h-8 rounded-lg text-xs"
              onClick={() => {
                onDesignChange({
                  color: color.trim() ? color.trim() : null,
                  backgroundImageUrl: backgroundImageUrl.trim() ? backgroundImageUrl.trim() : null
                })
                setDesignOpen(false)
              }}
            >
              Apply design
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}

function EventVisualsPanel({
  eventId,
  images,
  heroBannerUrl,
  accentColor,
  disabled
}: {
  eventId: string
  images: { imageId: string; imageUrl: string; isCover: boolean; displayOrder: number }[]
  heroBannerUrl: string | null
  accentColor: string | null
  disabled: boolean
}) {
  const queryClient = useQueryClient()
  const [imageUrl, setImageUrl] = useState("")
  const [bannerUrl, setBannerUrl] = useState(heroBannerUrl ?? "")
  const [accent, setAccent] = useState(accentColor ?? "")

  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: ["event", eventId] })
    void queryClient.invalidateQueries({ queryKey: ["admin-events"] })
    void queryClient.invalidateQueries({ queryKey: ["catalog"] })
  }

  const onError = (error: unknown) => toast.error(error instanceof ApiError ? error.detail : "Action failed")

  const addImageMutation = useMutation({
    mutationFn: () => eventsApi.addImage(eventId, { imageUrl: imageUrl.trim(), setAsCover: images.length === 0 }),
    onSuccess: () => {
      toast.success("Image added to the gallery")
      setImageUrl("")
      refresh()
    },
    onError
  })

  const removeImageMutation = useMutation({
    mutationFn: (imageId: string) => eventsApi.removeImage(eventId, imageId),
    onSuccess: () => {
      toast.success("Image removed")
      refresh()
    },
    onError
  })

  const coverMutation = useMutation({
    mutationFn: (imageId: string) => eventsApi.setCoverImage(eventId, imageId),
    onSuccess: () => {
      toast.success("Cover image updated")
      refresh()
    },
    onError
  })

  const appearanceMutation = useMutation({
    mutationFn: () =>
      eventsApi.updateAppearance(eventId, {
        heroBannerUrl: bannerUrl.trim() ? bannerUrl.trim() : null,
        accentColor: accent.trim() ? accent.trim() : null
      }),
    onSuccess: () => {
      toast.success("Appearance updated")
      refresh()
    },
    onError
  })

  const previewBanner = bannerUrl.trim() || images.find((i) => i.isCover)?.imageUrl || images[0]?.imageUrl || null
  const previewAccent = /^#[0-9a-fA-F]{6}$/.test(accent.trim()) ? accent.trim().toUpperCase() : "#7C3AED"

  return (
    <Card className="overflow-hidden rounded-2xl">
      <CardHeader className="border-b border-border/60 bg-muted/30 pb-3">
        <CardTitle className="font-display text-base">Visuals & branding</CardTitle>
      </CardHeader>
      <CardContent className="space-y-5 p-4 sm:p-5">
        {/* live preview */}
        <div>
          <p className="label-micro mb-2">Live preview</p>
          <div className="relative h-32 overflow-hidden rounded-xl border border-border/70">
            {previewBanner ? (
              <img src={previewBanner} alt="" className="absolute inset-0 h-full w-full object-cover" />
            ) : (
              <div className="absolute inset-0 bg-gradient-to-br from-violet-600 via-fuchsia-600 to-orange-500" style={{ background: `linear-gradient(135deg, ${previewAccent}, #7c3aed 55%, #ea580c)` }} />
            )}
            <div className="bg-noise absolute inset-0" />
            <div className="absolute inset-x-0 bottom-0 h-12 bg-gradient-to-t from-black/60 to-transparent" />
            <span
              className="absolute bottom-2 left-2 rounded-full px-2.5 py-1 text-[10px] font-bold tracking-widest text-white uppercase"
              style={{ background: `${previewAccent}CC` }}
            >
              Hero preview
            </span>
          </div>
        </div>

        {/* gallery */}
        <div>
          <p className="label-micro mb-2">Gallery · {images.length}/8</p>
          {images.length > 0 && (
            <div className="mb-2.5 grid grid-cols-4 gap-2">
              {images.map((image) => (
                <div key={image.imageId} className="group relative aspect-square overflow-hidden rounded-xl border border-border/70">
                  <img src={image.imageUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
                  {image.isCover && (
                    <span className="absolute top-1 left-1 rounded-full bg-black/65 px-1.5 py-0.5 text-[9px] font-bold tracking-widest text-white uppercase">
                      Cover
                    </span>
                  )}
                  <div className="absolute inset-x-0 bottom-0 flex translate-y-1 justify-center gap-1 bg-black/60 p-1 opacity-0 transition-all group-hover:translate-y-0 group-hover:opacity-100">
                    {!image.isCover && (
                      <button
                        onClick={() => coverMutation.mutate(image.imageId)}
                        className="rounded-md px-1.5 py-0.5 text-[10px] font-semibold text-white hover:bg-white/20"
                      >
                        Cover
                      </button>
                    )}
                    <button
                      onClick={() => removeImageMutation.mutate(image.imageId)}
                      className="rounded-md px-1.5 py-0.5 text-[10px] font-semibold text-red-300 hover:bg-white/20"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
          <div className="flex gap-2">
            <Input
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
              placeholder="https://…/photo.jpg"
              disabled={disabled || images.length >= 8}
              className="h-10 flex-1 rounded-xl text-xs"
            />
            <Button
              size="sm"
              className="btn-press h-10 shrink-0 rounded-xl"
              disabled={disabled || !imageUrl.trim() || addImageMutation.isPending || images.length >= 8}
              onClick={() => addImageMutation.mutate()}
            >
              <Plus className="size-3.5" /> Add
            </Button>
          </div>
        </div>

        {/* appearance */}
        <div className="space-y-2.5 rounded-xl bg-muted/40 p-3.5">
          <div className="space-y-1.5">
            <Label className="text-xs">Hero banner URL (optional)</Label>
            <Input
              value={bannerUrl}
              onChange={(e) => setBannerUrl(e.target.value)}
              placeholder="https://…/banner.jpg — falls back to the cover image"
              disabled={disabled}
              className="h-9 rounded-lg text-xs"
            />
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Accent color (optional)</Label>
            <div className="flex items-center gap-2">
              <input
                type="color"
                value={/^#[0-9a-fA-F]{6}$/.test(accent.trim()) ? accent.trim() : "#7C3AED"}
                onChange={(e) => setAccent(e.target.value.toUpperCase())}
                disabled={disabled}
                className="h-9 w-12 cursor-pointer rounded-lg border border-input bg-transparent p-1"
                aria-label="Accent color"
              />
              <Input
                value={accent}
                onChange={(e) => setAccent(e.target.value)}
                placeholder="#7C3AED"
                maxLength={7}
                disabled={disabled}
                className="h-9 rounded-lg font-mono text-xs uppercase"
              />
              {accent.trim() && (
                <Button size="sm" variant="ghost" className="h-9 rounded-lg px-2 text-xs" onClick={() => setAccent("")}>
                  Clear
                </Button>
              )}
            </div>
          </div>
          <div className="flex justify-end">
            <Button
              size="sm"
              className="btn-press rounded-xl"
              disabled={disabled || appearanceMutation.isPending}
              onClick={() => appearanceMutation.mutate()}
            >
              Save appearance
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

export function EventFormPage() {
  const { eventId } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const isEdit = Boolean(eventId)

  const categoriesQuery = useQuery({
    queryKey: ["categories"],
    queryFn: ({ signal }) => categoriesApi.list(signal)
  })

  const eventQuery = useQuery({
    queryKey: ["event", eventId],
    queryFn: ({ signal }) => eventsApi.get(eventId!, signal),
    enabled: isEdit
  })

  const statsQuery = useQuery({
    queryKey: ["event-statistics", eventId],
    queryFn: ({ signal }) => statisticsApi.forEvent(eventId!, signal),
    enabled: isEdit
  })

  const [categoryId, setCategoryId] = useState("")
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [location, setLocation] = useState("")
  const [startAtUtc, setStartAtUtc] = useState("")
  const [endAtUtc, setEndAtUtc] = useState("")

  useEffect(() => {
    if (isEdit && eventQuery.data) {
      setTitle(eventQuery.data.title)
      setDescription(eventQuery.data.description)
      setLocation(eventQuery.data.location)
      setCategoryId(eventQuery.data.categoryId)
    }
  }, [isEdit, eventQuery.data])

  const createMutation = useMutation({
    mutationFn: () =>
      eventsApi.create({
        categoryId,
        title,
        description,
        location,
        startAtUtc: normalizeDateTimeInput(startAtUtc),
        endAtUtc: endAtUtc ? normalizeDateTimeInput(endAtUtc) : null
      }),
    onSuccess: (newEventId) => {
      toast.success("Event created as a draft — add ticket types, then publish")
      void navigate(`/admin/events/${newEventId}/edit`)
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not create the event")
  })

  if (isEdit && eventQuery.isPending) {
    return <LoadingState />
  }

  if (isEdit && eventQuery.isError) {
    return <ErrorState message={String(eventQuery.error)} onRetry={() => eventQuery.refetch()} />
  }

  const event = eventQuery.data

  return (
    <div>
      <Button asChild variant="ghost" size="sm" className="mb-6 -ml-2 text-muted-foreground">
        <Link to="/admin/events">
          <ArrowLeft className="size-4" /> All events
        </Link>
      </Button>

      {isEdit && event ? (
        <>
          <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
            <div>
              <h1 className="text-display text-3xl font-bold tracking-tight">{event.title}</h1>
              <p className="mt-1 font-mono text-xs text-muted-foreground">{formatDateTime(event.startAtUtc)}</p>
            </div>
            <div className="flex items-center gap-3">
              <StatusBadge status={event.status} />
              <LifecycleActions
                eventId={event.id}
                status={event.status}
                onDone={() => {
                  void queryClient.invalidateQueries({ queryKey: ["event", eventId] })
                  void queryClient.invalidateQueries({ queryKey: ["admin-events"] })
                  void queryClient.invalidateQueries({ queryKey: ["catalog"] })
                }}
              />
            </div>
          </div>

          <div className="grid gap-6 lg:grid-cols-2">
            <div className="space-y-6">
              <TicketTypesPanel eventId={event.id} eventStatus={event.status} />
              <EventVisualsPanel
                eventId={event.id}
                images={event.images}
                heroBannerUrl={event.heroBannerUrl}
                accentColor={event.accentColor}
                disabled={event.status === "Canceled"}
              />
            </div>

            <Card className="rounded-2xl">
              <CardHeader className="pb-2">
                <CardTitle className="font-display text-base">Attendance statistics</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                {statsQuery.isError || !statsQuery.data ? (
                  <p className="text-muted-foreground">Statistics appear once tickets are issued.</p>
                ) : (
                  <>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Tickets sold</span>
                      <span className="font-mono font-semibold">{statsQuery.data.ticketsSold}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Checked in</span>
                      <span className="font-mono font-semibold">{statsQuery.data.attendeesCheckedIn}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Duplicate check-in attempts</span>
                      <span className="font-mono">{statsQuery.data.duplicateCheckInTickets.length}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Invalid check-in attempts</span>
                      <span className="font-mono">{statsQuery.data.invalidCheckInTickets.length}</span>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>
          </div>
        </>
      ) : (
        <div className="mx-auto max-w-2xl">
          <PageHeader title="New event" description="Create a draft, add ticket types, then publish." />
          <Card className="rounded-2xl">
            <CardContent className="space-y-4 p-6">
              <div className="space-y-1.5">
                <Label>Category</Label>
                <select
                  className="h-9 w-full rounded-sm border border-input bg-transparent px-3 text-sm"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                >
                  <option value="">Select a category…</option>
                  {categoriesQuery.data
                    ?.filter((c) => !c.isArchived)
                    .map((category) => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))}
                </select>
              </div>
              <div className="space-y-1.5">
                <Label>Title</Label>
                <Input value={title} onChange={(e) => setTitle(e.target.value)} />
              </div>
              <div className="space-y-1.5">
                <Label>Description</Label>
                <Textarea value={description} onChange={(e) => setDescription(e.target.value)} className="min-h-20" />
              </div>
              <div className="space-y-1.5">
                <Label>Location</Label>
                <Input value={location} onChange={(e) => setLocation(e.target.value)} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1.5">
                  <Label>Starts (UTC)</Label>
                  <Input type="datetime-local" value={startAtUtc} onChange={(e) => setStartAtUtc(e.target.value)} />
                </div>
                <div className="space-y-1.5">
                  <Label>Ends (UTC, optional)</Label>
                  <Input type="datetime-local" value={endAtUtc} onChange={(e) => setEndAtUtc(e.target.value)} />
                </div>
              </div>
              <Button
                className="w-full"
                disabled={createMutation.isPending || categoryId === "" || title.trim() === "" || startAtUtc === ""}
                onClick={() => createMutation.mutate()}
              >
                Create draft event
              </Button>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}

