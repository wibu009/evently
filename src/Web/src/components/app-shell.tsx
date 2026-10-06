import { NavLink, Outlet, useLocation, useNavigate } from "react-router"
import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import {
  CalendarDays,
  LogOut,
  Menu,
  Moon,
  ScanBarcode,
  ShoppingCart,
  Sun,
  Ticket,
  Timer,
  UserRound,
  Settings2,
  X,
  Zap
} from "lucide-react"

import { useTheme } from "@/components/theme"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu"
import { cartApi } from "@/api/endpoints"
import { useAuthContext } from "@/lib/auth-context"
import { useCurrentUser, useHasPermission } from "@/lib/use-auth"
import { cn } from "@/lib/utils"

function ThemeToggle() {
  const { theme, setTheme } = useTheme()
  const isDark = theme === "dark"

  return (
    <Button
      variant="ghost"
      size="icon"
      aria-label="Toggle theme"
      onClick={() => setTheme(isDark ? "light" : "dark")}
      className="btn-press relative overflow-hidden rounded-lg"
    >
      <span key={theme} className="animate-scale-in flex">
        {isDark ? <Sun className="size-4" /> : <Moon className="size-4" />}
      </span>
    </Button>
  )
}

const navItems = [
  { to: "/", label: "Discover", end: true, authenticated: false },
  { to: "/orders", label: "Orders", authenticated: true },
  { to: "/tickets", label: "Tickets", authenticated: true },
  { to: "/payments", label: "Payments", authenticated: true },
  { to: "/waiting-list", label: "Waiting list", authenticated: true }
]

function NavLinks({ onNavigate, mobile = false }: { onNavigate?: () => void; mobile?: boolean }) {
  const has = useHasPermission()
  const { isAuthenticated } = useAuthContext()

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    cn(
      "group relative rounded-lg px-3 py-2 text-sm font-medium transition-all duration-200",
      mobile && "w-full px-4 py-2.5 text-[15px]",
      isActive
        ? "bg-accent text-accent-foreground shadow-[inset_0_0_0_1px_var(--border)]"
        : "text-muted-foreground hover:bg-accent/60 hover:text-foreground active:scale-[0.98]"
    )

  return (
    <>
      {navItems
        .filter((item) => !item.authenticated || isAuthenticated)
        .map((item) => (
          <NavLink key={item.to} to={item.to} end={item.end} onClick={onNavigate} className={linkClass}>
            {item.label}
          </NavLink>
        ))}
      {isAuthenticated && has("tickets:check-in") && (
        <NavLink to="/check-in" onClick={onNavigate} className={linkClass}>
          <span className="inline-flex items-center gap-1.5">
            <ScanBarcode className="size-3.5" />
            Check-in
          </span>
        </NavLink>
      )}
      {has("events:update") && (
        <NavLink to="/admin/events" onClick={onNavigate} className={linkClass}>
          <span className="inline-flex items-center gap-1.5">
            Admin
            <Zap className="size-3 text-primary" />
          </span>
        </NavLink>
      )}
    </>
  )
}

export function AppShell() {
  const { user, isAuthenticated } = useCurrentUser()
  const has = useHasPermission()
  const navigate = useNavigate()
  const location = useLocation()
  const { login, logout } = useAuthContext()
  const [mobileOpen, setMobileOpen] = useState(false)

  const cartQuery = useQuery({
    queryKey: ["cart"],
    queryFn: ({ signal }) => cartApi.get(signal),
    enabled: isAuthenticated && has("carts:read")
  })

  const cartCount = cartQuery.data?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0

  const initials = user
    ? `${user.name.split(" ")[0]?.[0] ?? ""}${user.name.split(" ")[1]?.[0] ?? ""}`.toUpperCase() || "•"
    : "•"

  return (
    <div className="relative min-h-svh">
      {/* ambient background */}
      <div aria-hidden className="pointer-events-none fixed inset-0 -z-10 overflow-hidden">
        <div className="bg-grid absolute inset-0 opacity-[0.35] [mask-image:radial-gradient(ellipse_75%_55%_at_50%_0%,black,transparent)]" />
        <div className="absolute -top-32 left-1/2 h-72 w-[42rem] -translate-x-1/2 rounded-full bg-primary/15 blur-[110px]" />
        <div className="absolute top-1/3 -left-32 h-64 w-64 rounded-full bg-fuchsia-500/10 blur-[100px] dark:bg-fuchsia-500/[0.07]" />
        <div className="bg-noise absolute inset-0" />
      </div>

      <header className="sticky top-0 z-40 border-b border-border/60 glass">
        <div className="mx-auto flex h-16 max-w-7xl items-center gap-1.5 px-4 sm:px-6 lg:px-8">
          <Button
            variant="ghost"
            size="icon"
            className="btn-press -ml-2 rounded-lg md:hidden"
            aria-label={mobileOpen ? "Close menu" : "Open menu"}
            onClick={() => setMobileOpen((v) => !v)}
          >
            {mobileOpen ? <X className="size-5" /> : <Menu className="size-5" />}
          </Button>

          <NavLink to="/" className="group mr-4 flex items-center gap-2.5 sm:mr-6">
            <span className="flex size-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary via-fuchsia-500 to-violet-600 font-display text-base font-bold text-white shadow-lg shadow-primary/25 transition-transform duration-300 group-hover:rotate-6 group-hover:scale-105">
              E
            </span>
            <span className="text-display text-lg font-bold tracking-tight">
              EVENTLY
              <span className="ml-1.5 hidden rounded-full bg-primary/10 px-1.5 py-0.5 align-middle text-[10px] font-semibold tracking-widest text-primary sm:inline-block">
                BETA
              </span>
            </span>
          </NavLink>

          <nav className="hidden items-center gap-0.5 md:flex">
            <NavLinks />
          </nav>

          <div className="ml-auto flex items-center gap-1">
            {isAuthenticated && (
              <NavLink
                to="/cart"
                className={({ isActive }) =>
                  cn(
                    "btn-press relative rounded-lg p-2.5 transition-colors",
                    isActive ? "bg-accent text-accent-foreground" : "text-muted-foreground hover:bg-accent/60 hover:text-foreground"
                  )
                }
                aria-label={`Cart${cartCount > 0 ? `, ${cartCount} items` : ""}`}
              >
                <ShoppingCart className="size-[18px]" />
                {cartCount > 0 && (
                  <Badge
                    key={cartCount}
                    className="animate-pop absolute -top-0.5 -right-0.5 flex h-5 min-w-5 items-center justify-center rounded-full border-2 border-background px-1 text-[10px] font-bold tabular-nums"
                  >
                    {cartCount > 99 ? "99+" : cartCount}
                  </Badge>
                )}
              </NavLink>
            )}

            <ThemeToggle />

            {isAuthenticated ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" className="btn-press ml-0.5 rounded-full" aria-label="Account">
                    <Avatar className="size-8 ring-2 ring-transparent transition-all hover:ring-primary/30">
                      <AvatarFallback className="rounded-full bg-gradient-to-br from-primary/25 to-fuchsia-500/20 text-xs font-bold text-primary">
                        {initials}
                      </AvatarFallback>
                    </Avatar>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-60 rounded-xl">
                  <DropdownMenuLabel>
                    <p className="truncate text-sm font-semibold">{user?.email}</p>
                    <p className="text-xs font-normal text-muted-foreground">{user?.name}</p>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => navigate("/profile")} className="cursor-pointer">
                    <UserRound className="size-4" /> Profile
                  </DropdownMenuItem>
                  {has("tickets:check-in") && (
                    <DropdownMenuItem onClick={() => navigate("/check-in")} className="cursor-pointer">
                      <ScanBarcode className="size-4" /> Gate check-in
                    </DropdownMenuItem>
                  )}
                  {has("events:update") && (
                    <DropdownMenuItem onClick={() => navigate("/admin/events")} className="cursor-pointer">
                      <Settings2 className="size-4" /> Admin area
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onClick={() => {
                      void logout().then(() => navigate("/", { replace: true }))
                    }}
                    className="cursor-pointer text-destructive focus:text-destructive"
                  >
                    <LogOut className="size-4" /> Sign out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <div className="ml-1 flex items-center gap-1.5">
                <Button variant="ghost" size="sm" className="btn-press hidden rounded-lg sm:inline-flex" onClick={() => void login()}>
                  Sign in
                </Button>
                <Button size="sm" className="btn-press rounded-lg font-semibold" onClick={() => void login()}>
                  Get started
                </Button>
              </div>
            )}
          </div>
        </div>

        {/* mobile nav panel */}
        <div
          className={cn(
            "grid overflow-hidden border-border/60 transition-all duration-300 ease-out md:hidden",
            mobileOpen ? "grid-rows-[1fr] border-t opacity-100" : "grid-rows-[0fr] opacity-0"
          )}
        >
          <nav className="min-h-0 overflow-hidden">
            <div className="flex flex-col gap-0.5 px-4 py-3">
              <NavLinks mobile onNavigate={() => setMobileOpen(false)} />
            </div>
          </nav>
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl px-4 py-6 sm:px-6 sm:py-8 lg:px-8 lg:py-10">
        <div key={location.pathname} className="animate-fade-up">
          <Outlet />
        </div>
      </main>

      <footer className="mt-8 border-t border-border/60 bg-card/30">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-center sm:justify-between sm:px-6 lg:px-8">
          <div>
            <p className="font-display text-sm font-bold tracking-tight">EVENTLY — live the moment</p>
            <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
              Discover · Book · Scan · Repeat. Built for the dancefloor.
            </p>
          </div>
          <div className="flex items-center gap-4 text-muted-foreground">
            <span className="flex items-center gap-1.5 text-xs"><CalendarDays className="size-3.5" /> Events</span>
            <span className="flex items-center gap-1.5 text-xs"><ScanBarcode className="size-3.5" /> Check-in</span>
            <span className="hidden items-center gap-1.5 text-xs sm:flex"><Ticket className="size-3.5" /> Tickets</span>
            <span className="hidden items-center gap-1.5 text-xs sm:flex"><Timer className="size-3.5" /> 15-min holds</span>
          </div>
        </div>
      </footer>
    </div>
  )
}
