import { useEffect, useState } from "react"
import QRCode from "qrcode"

import { Skeleton } from "@/components/ui/skeleton"
import { cn } from "@/lib/utils"

/**
 * Renders the ticket code as a scannable QR code (the exact payload the gate
 * scanner reads). Generated client-side — no round trip, no storage.
 */
export function TicketQr({ code, className }: { code: string; className?: string }) {
  const [dataUrl, setDataUrl] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    void QRCode.toDataURL(code, { width: 200, margin: 1 })
      .then((url) => {
        if (!cancelled) {
          setDataUrl(url)
        }
      })
      .catch(() => {
        if (!cancelled) {
          setDataUrl(null)
        }
      })

    return () => {
      cancelled = true
    }
  }, [code])

  if (!dataUrl) {
    return <Skeleton className={cn("size-20 rounded-xl", className)} />
  }

  return (
    <img
      src={dataUrl}
      alt={`QR code for ticket ${code}`}
      width={200}
      height={200}
      loading="lazy"
      className={cn("size-20 rounded-xl border border-border/60 bg-white p-1", className)}
    />
  )
}
