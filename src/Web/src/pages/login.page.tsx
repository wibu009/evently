import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Link, useLocation, useNavigate } from "react-router"
import { toast } from "sonner"
import { KeyRound, PartyPopper } from "lucide-react"
import { z } from "zod"

import { useAuthContext } from "@/lib/auth-context"
import { AuthError } from "@/lib/session"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

const loginSchema = z.object({
  email: z.string().email("Enter a valid email"),
  password: z.string().min(1, "Password is required")
})

type LoginForm = z.infer<typeof loginSchema>

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login, isAuthenticated, isLoading } = useAuthContext()

  const from = (location.state as { from?: string } | null)?.from ?? "/"

  const form = useForm<LoginForm>({ resolver: zodResolver(loginSchema) })
  const { isSubmitting } = form.formState

  if (!isLoading && isAuthenticated) {
    void navigate(from, { replace: true })

    return null
  }

  const onSubmit = async (values: LoginForm) => {
    try {
      await login(values.email, values.password)
      toast.success("Welcome back")
      navigate(from, { replace: true })
    } catch (error) {
      if (error instanceof AuthError && error.code === "invalid-credentials") {
        form.setError("password", { message: "Invalid email or password." })
      } else {
        toast.error(error instanceof AuthError ? error.message : "Sign-in failed")
      }
    }
  }

  return (
    <div className="relative flex min-h-[80svh] items-center justify-center overflow-hidden p-4">
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="bg-grid absolute inset-0 opacity-50 [mask-image:radial-gradient(ellipse_60%_60%_at_50%_40%,black,transparent)]" />
        <div className="absolute top-1/4 left-1/2 h-64 w-[36rem] -translate-x-1/2 rounded-full bg-primary/20 blur-[110px]" />
      </div>

      <Card className="animate-scale-in w-full max-w-md rounded-2xl border-border/70 shadow-2xl shadow-primary/10">
        <CardHeader className="pb-2 text-center">
          <div className="mx-auto mb-3 flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-primary via-fuchsia-500 to-orange-400 shadow-lg shadow-primary/30">
            <KeyRound className="size-6 text-white" />
          </div>
          <CardTitle className="text-display text-2xl font-bold tracking-tight sm:text-[1.7rem]">Welcome back</CardTitle>
          <CardDescription className="mt-1">Sign in to your Evently account.</CardDescription>
        </CardHeader>
        <CardContent className="p-5 sm:p-6">
          <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                {...form.register("email")}
                className="h-11 rounded-xl"
                autoComplete="email"
                placeholder="you@night.out"
              />
              {form.formState.errors.email && (
                <p className="text-xs text-destructive">{form.formState.errors.email.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Password</Label>
              </div>
              <Input
                id="password"
                type="password"
                {...form.register("password")}
                className="h-11 rounded-xl"
                autoComplete="current-password"
                placeholder="Your password"
              />
              {form.formState.errors.password && (
                <p className="text-xs text-destructive">{form.formState.errors.password.message}</p>
              )}
            </div>
            <Button
              type="submit"
              className="btn-press h-11 w-full rounded-xl text-[15px] font-bold shadow-lg shadow-primary/25"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Signing in…" : "Sign in"}
            </Button>
            <p className="flex items-center justify-center gap-1.5 text-center text-sm text-muted-foreground">
              <PartyPopper className="size-3.5" />
              New here?{" "}
              <Link to="/register" className="font-medium text-primary hover:underline">
                Create an account
              </Link>
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
