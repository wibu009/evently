import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Link, useNavigate } from "react-router"
import { useMutation } from "@tanstack/react-query"
import { toast } from "sonner"
import { PartyPopper } from "lucide-react"
import { z } from "zod"

import { usersApi } from "@/api/endpoints"
import { useAuthContext } from "@/lib/auth-context"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/http"

const registerSchema = z
  .object({
    email: z.string().email("Enter a valid email"),
    password: z.string().min(6, "At least 6 characters"),
    firstName: z.string().min(1, "First name is required"),
    lastName: z.string().min(1, "Last name is required")
  })
  .required()

type RegisterForm = z.infer<typeof registerSchema>

export function RegisterPage() {
  const navigate = useNavigate()
  const { isAuthenticated, login } = useAuthContext()

  const form = useForm<RegisterForm>({ resolver: zodResolver(registerSchema) })

  const registerMutation = useMutation({
    mutationFn: async (values: RegisterForm) => {
      await usersApi.register(values)

      return values
    },
    onSuccess: async (values) => {
      // The account was just provisioned in Keycloak — sign straight in.
      try {
        await login(values.email, values.password)
        toast.success("Account created — welcome to Evently")
      } catch {
        toast.success("Account created — sign in to continue")
      }
      navigate("/", { replace: true })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.status === 409) {
        toast.error("That email is already registered — try signing in")
      } else {
        toast.error(error instanceof ApiError ? error.detail : "Registration failed")
      }
    }
  })

  if (isAuthenticated) {
    void navigate("/", { replace: true })
  }

  return (
    <div className="relative flex min-h-svh items-center justify-center overflow-hidden p-4">
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="bg-grid absolute inset-0 opacity-50 [mask-image:radial-gradient(ellipse_60%_60%_at_50%_40%,black,transparent)]" />
        <div className="absolute top-1/4 left-1/2 h-64 w-[36rem] -translate-x-1/2 rounded-full bg-primary/20 blur-[110px]" />
      </div>

      <Card className="animate-scale-in w-full max-w-md rounded-2xl border-border/70 shadow-2xl shadow-primary/10">
        <CardHeader className="pb-2 text-center">
          <div className="mx-auto mb-3 flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-primary via-fuchsia-500 to-orange-400 shadow-lg shadow-primary/30">
            <PartyPopper className="size-6 text-white" />
          </div>
          <CardTitle className="text-display text-2xl font-bold tracking-tight sm:text-[1.7rem]">Create your account</CardTitle>
          <CardDescription className="mt-1">Join Evently to discover and book live events.</CardDescription>
        </CardHeader>
        <CardContent className="p-5 sm:p-6">
          <form className="space-y-4" onSubmit={form.handleSubmit((values) => registerMutation.mutate(values))}>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div className="space-y-1.5">
                <Label htmlFor="firstName">First name</Label>
                <Input id="firstName" {...form.register("firstName")} className="h-11 rounded-xl" autoComplete="given-name" placeholder="Ada" />
                {form.formState.errors.firstName && (
                  <p className="text-xs text-destructive">{form.formState.errors.firstName.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="lastName">Last name</Label>
                <Input id="lastName" {...form.register("lastName")} className="h-11 rounded-xl" autoComplete="family-name" placeholder="Lovelace" />
                {form.formState.errors.lastName && (
                  <p className="text-xs text-destructive">{form.formState.errors.lastName.message}</p>
                )}
              </div>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" {...form.register("email")} className="h-11 rounded-xl" autoComplete="email" placeholder="you@night.out" />
              {form.formState.errors.email && <p className="text-xs text-destructive">{form.formState.errors.email.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="password">Password</Label>
              <Input id="password" type="password" {...form.register("password")} className="h-11 rounded-xl" autoComplete="new-password" placeholder="At least 6 characters" />
              {form.formState.errors.password && (
                <p className="text-xs text-destructive">{form.formState.errors.password.message}</p>
              )}
            </div>
            <Button type="submit" className="btn-press h-11 w-full rounded-xl text-[15px] font-bold shadow-lg shadow-primary/25" disabled={registerMutation.isPending}>
              {registerMutation.isPending ? "Creating…" : "Create account"}
            </Button>
            <p className="text-center text-sm text-muted-foreground">
              Already registered?{" "}
              <Link to="/" className="font-medium text-primary hover:underline">
                Sign in
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
