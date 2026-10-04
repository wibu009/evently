import { useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { Check, Copy } from "lucide-react"
import { useState } from "react"
import { z } from "zod"

import { usersApi } from "@/api/endpoints"
import { ErrorState, LoadingState, PageHeader } from "@/components/shared"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { ApiError } from "@/lib/http"
import { useCurrentUser } from "@/lib/use-auth"

const profileSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required")
})

type ProfileForm = z.infer<typeof profileSchema>

export function ProfilePage() {
  const queryClient = useQueryClient()
  const { user } = useCurrentUser()
  const [copied, setCopied] = useState(false)

  const profileQuery = useQuery({
    queryKey: ["my-profile"],
    queryFn: ({ signal }) => usersApi.myProfile(signal)
  })

  const form = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    defaultValues: { firstName: "", lastName: "" }
  })

  useEffect(() => {
    if (profileQuery.data) {
      form.reset({ firstName: profileQuery.data.firstName, lastName: profileQuery.data.lastName })
    }
  }, [profileQuery.data, form])

  const saveMutation = useMutation({
    mutationFn: (values: ProfileForm) => usersApi.updateProfile(profileQuery.data!.id, values),
    onSuccess: () => {
      toast.success("Profile updated")
      void queryClient.invalidateQueries({ queryKey: ["my-profile"] })
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.detail : "Could not update the profile")
  })

  if (profileQuery.isPending) {
    return (
      <>
        <PageHeader title="Profile" eyebrow="Account" />
        <LoadingState />
      </>
    )
  }

  if (profileQuery.isError || !user) {
    return (
      <>
        <PageHeader title="Profile" eyebrow="Account" />
        <ErrorState message={String(profileQuery.error ?? "Profile unavailable")} onRetry={() => profileQuery.refetch()} />
      </>
    )
  }

  const initials = `${profileQuery.data.firstName[0] ?? ""}${profileQuery.data.lastName[0] ?? ""}`.toUpperCase()

  const copyId = async () => {
    try {
      await navigator.clipboard.writeText(profileQuery.data.id)
      setCopied(true)
      toast.success("Customer ID copied")
      setTimeout(() => setCopied(false), 1500)
    } catch {
      toast.error("Could not copy — select it manually")
    }
  }

  return (
    <div className="mx-auto w-full max-w-xl">
      <PageHeader title="Profile" eyebrow="Account" description="Your identity across Evently — used for orders, tickets and transfers." />

      <Card className="animate-fade-up overflow-hidden rounded-2xl border-border/70">
        <div className="relative bg-gradient-to-br from-primary via-fuchsia-600 to-orange-500 px-5 pt-6 pb-14 sm:px-6">
          <div className="bg-noise absolute inset-0" />
          <div className="relative flex items-center gap-4">
            <Avatar className="size-14 ring-4 ring-white/25">
              <AvatarFallback className="bg-white text-lg font-bold text-black">{initials}</AvatarFallback>
            </Avatar>
            <div className="min-w-0">
              <p className="truncate font-display text-lg font-bold text-white">
                {profileQuery.data.firstName} {profileQuery.data.lastName}
              </p>
              <p className="truncate text-sm text-white/80">{profileQuery.data.email}</p>
            </div>
          </div>
        </div>
        <CardContent className="space-y-5 p-5 sm:p-6">
          <div className="space-y-1.5">
            <Label>Email</Label>
            <Input id="email" value={profileQuery.data.email} readOnly className="h-11 rounded-xl bg-muted/40 font-mono text-xs" />
          </div>

          <div className="space-y-1.5">
            <Label>Customer ID — share for ticket transfers</Label>
            <div className="flex gap-2">
              <Input value={profileQuery.data.id} readOnly className="h-11 flex-1 rounded-xl bg-muted/40 font-mono text-xs" />
              <Button variant="outline" size="icon" className="btn-press h-11 w-11 shrink-0 rounded-xl" onClick={copyId} aria-label="Copy customer ID">
                {copied ? <Check className="size-4 text-success" /> : <Copy className="size-4" />}
              </Button>
            </div>
          </div>

          <Separator />

          <form className="space-y-4" onSubmit={form.handleSubmit((values) => saveMutation.mutate(values))}>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="firstName">First name</Label>
                <Input id="firstName" {...form.register("firstName")} className="h-11 rounded-xl" autoComplete="given-name" />
                {form.formState.errors.firstName && (
                  <p className="text-xs text-destructive">{form.formState.errors.firstName.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="lastName">Last name</Label>
                <Input id="lastName" {...form.register("lastName")} className="h-11 rounded-xl" autoComplete="family-name" />
                {form.formState.errors.lastName && (
                  <p className="text-xs text-destructive">{form.formState.errors.lastName.message}</p>
                )}
              </div>
            </div>
            <Button type="submit" className="btn-press h-11 w-full rounded-xl font-semibold sm:w-auto sm:px-8" disabled={saveMutation.isPending || !form.formState.isDirty}>
              {saveMutation.isPending ? "Saving…" : "Save changes"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
