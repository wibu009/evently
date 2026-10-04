import { useCallback, useEffect, useRef, useState } from "react"
import { useMutation } from "@tanstack/react-query"
import { toast } from "sonner"
import { Camera, CameraOff, CheckCircle2, CopyX, HelpCircle, Keyboard, ScanLine, ShieldAlert } from "lucide-react"

import { statisticsApi, type CheckInResult } from "@/api/endpoints"
import { EmptyState, PageHeader } from "@/components/shared"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/http"
import { useHasPermission } from "@/lib/use-auth"
import { cn } from "@/lib/utils"

interface ScanRecord {
  id: number
  code: string
  outcome: CheckInResult["outcome"]
  at: Date
}

const outcomeConfig: Record<string, { label: string; tone: string; icon: typeof CheckCircle2 }> = {
  CheckedIn: { label: "Checked in", tone: "bg-success/15 text-success", icon: CheckCircle2 },
  Duplicate: { label: "Already scanned", tone: "bg-warning/15 text-warning", icon: CopyX },
  Invalid: { label: "Invalid ticket", tone: "bg-destructive/10 text-destructive", icon: ShieldAlert },
  NotFound: { label: "Unknown code", tone: "bg-destructive/10 text-destructive", icon: HelpCircle }
}

const SCAN_COOLDOWN_MS = 5_000
const DETECT_INTERVAL_MS = 400

declare global {
  interface Window {
    BarcodeDetector?: new (options?: { formats: string[] }) => {
      detect: (source: HTMLVideoElement) => Promise<Array<{ rawValue: string }>>
    }
  }
}

function isScannerSupported(): boolean {
  return typeof window !== "undefined" && typeof window.BarcodeDetector === "function"
}

export function CheckInPage() {
  const has = useHasPermission()
  const [cameraOn, setCameraOn] = useState(false)
  const [cameraError, setCameraError] = useState<string | null>(null)
  const [manualCode, setManualCode] = useState("")
  const [scans, setScans] = useState<ScanRecord[]>([])
  const videoRef = useRef<HTMLVideoElement | null>(null)
  const streamRef = useRef<MediaStream | null>(null)
  const lastSubmitRef = useRef<{ code: string; at: number } | null>(null)
  const scanIdRef = useRef(0)

  const checkInMutation = useMutation({
    mutationFn: (code: string) => statisticsApi.checkInByCode(code),
    onSuccess: (result, code) => {
      const record: ScanRecord = { id: ++scanIdRef.current, code, outcome: result.outcome, at: new Date() }
      setScans((prev) => [record, ...prev].slice(0, 20))
    },
    onError: (error) => {
      toast.error(error instanceof ApiError ? error.detail : "Check-in failed — the scanner keeps running")
    }
  })

  const submitCode = useCallback(
    (rawCode: string) => {
      const code = rawCode.trim()
      if (!code || checkInMutation.isPending) {
        return
      }

      // Suppress re-scans of the same code while the camera still sees it.
      const last = lastSubmitRef.current
      if (last && last.code === code && Date.now() - last.at < SCAN_COOLDOWN_MS) {
        return
      }

      lastSubmitRef.current = { code, at: Date.now() }
      checkInMutation.mutate(code)
    },
    [checkInMutation]
  )

  const stopCamera = useCallback(() => {
    streamRef.current?.getTracks().forEach((track) => track.stop())
    streamRef.current = null
    setCameraOn(false)
  }, [])

  const startCamera = useCallback(async () => {
    setCameraError(null)

    if (!isScannerSupported()) {
      setCameraError("This browser has no built-in QR reader — use manual entry below.")
      return
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "environment", width: { ideal: 1280 }, height: { ideal: 720 } },
        audio: false
      })
      streamRef.current = stream
      setCameraOn(true)
    } catch {
      setCameraError("Camera access was denied. Allow camera access or use manual entry below.")
    }
  }, [])

  // Attach the stream once the video element exists.
  useEffect(() => {
    if (cameraOn && videoRef.current && streamRef.current && !videoRef.current.srcObject) {
      videoRef.current.srcObject = streamRef.current
      void videoRef.current.play().catch(() => {})
    }
  }, [cameraOn])

  // Detection loop: interval-based (not per-frame) to keep the main thread free for the queue.
  useEffect(() => {
    if (!cameraOn || !isScannerSupported()) {
      return
    }

    const detector = new window.BarcodeDetector!({ formats: ["qr_code"] })
    let stopped = false
    let timer: number | null = null

    const tick = async () => {
      if (stopped || !videoRef.current || videoRef.current.readyState < 2) {
        return
      }

      try {
        const codes = await detector.detect(videoRef.current)
        const value = codes[0]?.rawValue
        if (value) {
          submitCode(value)
        }
      } catch {
        // Transient decode errors are expected while aiming — ignore and keep scanning.
      }
    }

    timer = window.setInterval(() => void tick(), DETECT_INTERVAL_MS)

    return () => {
      stopped = true
      if (timer !== null) {
        window.clearInterval(timer)
      }
    }
  }, [cameraOn, submitCode])

  useEffect(() => stopCamera, [stopCamera])

  if (!has("tickets:check-in")) {
    return (
      <div className="container flex min-h-[60svh] flex-col items-center justify-center gap-2 text-center">
        <p className="text-display text-4xl font-bold">403</p>
        <p className="text-muted-foreground">
          You need the <code className="text-primary">tickets:check-in</code> permission to run the gate scanner.
        </p>
      </div>
    )
  }

  const checkedIn = scans.filter((s) => s.outcome === "CheckedIn").length
  const duplicates = scans.filter((s) => s.outcome === "Duplicate").length
  const problems = scans.filter((s) => s.outcome === "Invalid" || s.outcome === "NotFound").length

  return (
    <div>
      <PageHeader
        title="Gate check-in"
        eyebrow="Staff · QR scanner"
        description="Point the camera at a ticket QR code. Each scan is one indexed lookup — duplicates and unknown codes resolve instantly without retries."
      />

      <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
        <Card className="animate-fade-up overflow-hidden rounded-2xl border-border/70">
          <CardContent className="p-0">
            <div className="relative aspect-[4/3] bg-black">
              {cameraOn ? (
                <>
                  {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
                  <video ref={videoRef} playsInline muted className="absolute inset-0 h-full w-full object-cover" />
                  <div aria-hidden className="pointer-events-none absolute inset-0">
                    <div className="absolute inset-x-8 top-8 bottom-8 rounded-2xl border-2 border-primary/80 shadow-[0_0_0_9999px_rgba(0,0,0,0.55)]" />
                    <ScanLine className="absolute top-1/2 left-1/2 size-8 -translate-x-1/2 -translate-y-1/2 animate-pulse text-primary" />
                  </div>
                  <Badge className="absolute top-3 left-3 animate-fade-in rounded-full bg-black/60 text-white backdrop-blur">
                    <span className="mr-1.5 inline-block size-1.5 animate-[pulse-dot_1.6s_ease-in-out_infinite] rounded-full bg-success" />
                    Scanning
                  </Badge>
                </>
              ) : (
                <div className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center">
                  <div className="flex size-14 items-center justify-center rounded-2xl bg-white/10">
                    <Camera className="size-7 text-white/70" />
                  </div>
                  <p className="max-w-xs text-sm leading-relaxed text-white/70">
                    {cameraError ?? "Start the camera to scan ticket QR codes at the gate."}
                  </p>
                  <Button onClick={() => void startCamera()} className="btn-press rounded-xl">
                    <Camera className="size-4" /> Start scanner
                  </Button>
                </div>
              )}
            </div>
          </CardContent>
          {cameraOn && (
            <div className="flex items-center justify-between border-t border-border/60 p-3">
              <p className="text-xs text-muted-foreground">
                {checkInMutation.isPending ? "Verifying ticket…" : "Aim a ticket QR at the frame"}
              </p>
              <Button variant="outline" size="sm" className="btn-press rounded-xl" onClick={stopCamera}>
                <CameraOff className="size-3.5" /> Stop
              </Button>
            </div>
          )}
        </Card>

        <div className="min-w-0 space-y-4">
          <Card className="animate-fade-up rounded-2xl border-border/70 stagger-1">
            <CardHeader className="border-b border-border/60 bg-muted/30 pb-3">
              <CardTitle className="flex items-center gap-2 font-display text-[15px]">
                <Keyboard className="size-4 text-primary" /> Manual entry
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 p-4">
              <div className="space-y-1.5">
                <Label htmlFor="ticket-code">Ticket code</Label>
                <Input
                  id="ticket-code"
                  value={manualCode}
                  onChange={(e) => setManualCode(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && manualCode.trim()) {
                      submitCode(manualCode)
                      setManualCode("")
                    }
                  }}
                  placeholder="tc_01J…"
                  className="h-11 rounded-xl font-mono text-sm"
                  autoComplete="off"
                />
              </div>
              <Button
                className="btn-press h-10 w-full rounded-xl font-semibold"
                disabled={checkInMutation.isPending || !manualCode.trim()}
                onClick={() => {
                  submitCode(manualCode)
                  setManualCode("")
                }}
              >
                Check in
              </Button>
            </CardContent>
          </Card>

          <Card className="animate-fade-up rounded-2xl border-border/70 stagger-2">
            <CardHeader className="border-b border-border/60 bg-muted/30 pb-3">
              <CardTitle className="font-display text-[15px]">This session</CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-3 gap-2 p-4 text-center">
              <div className="rounded-xl bg-success/10 p-3">
                <p className="font-display text-xl font-bold text-success tabular-nums">{checkedIn}</p>
                <p className="text-[11px] text-muted-foreground">In</p>
              </div>
              <div className="rounded-xl bg-warning/10 p-3">
                <p className="font-display text-xl font-bold text-warning tabular-nums">{duplicates}</p>
                <p className="text-[11px] text-muted-foreground">Dupes</p>
              </div>
              <div className="rounded-xl bg-destructive/10 p-3">
                <p className="font-display text-xl font-bold text-destructive tabular-nums">{problems}</p>
                <p className="text-[11px] text-muted-foreground">Issues</p>
              </div>
            </CardContent>
          </Card>

          <div>
            <h2 className="label-micro mb-2">Recent scans</h2>
            {scans.length === 0 ? (
              <EmptyState title="No scans yet" description="Scanned tickets appear here with their gate outcome." />
            ) : (
              <div className="space-y-2">
                {scans.map((scan) => {
                  const conf = outcomeConfig[scan.outcome] ?? outcomeConfig.NotFound
                  const Icon = conf.icon

                  return (
                    <div
                      key={scan.id}
                      className="animate-fade-up flex items-center justify-between gap-3 rounded-xl border border-border/70 bg-card/70 px-3.5 py-2.5"
                    >
                      <div className="flex min-w-0 items-center gap-2.5">
                        <Icon className={cn("size-4 shrink-0", conf.tone.split(" ")[1])} />
                        <div className="min-w-0">
                          <p className="truncate font-mono text-xs font-semibold">{scan.code}</p>
                          <p className="text-[11px] text-muted-foreground tabular-nums">
                            {scan.at.toLocaleTimeString()}
                          </p>
                        </div>
                      </div>
                      <span className={cn("shrink-0 rounded-full px-2 py-0.5 text-[11px] font-bold", conf.tone)}>
                        {conf.label}
                      </span>
                    </div>
                  )
                })}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
