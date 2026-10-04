import { lazy, Suspense } from "react"
import { Navigate, Outlet, Route, Routes } from "react-router"

import { RequireAuth, RequirePermission } from "@/components/require-auth"
import { AppShell } from "@/components/app-shell"
import { LoadingState } from "@/components/shared"

const LoginPage = lazy(() => import("@/pages/login.page").then((m) => ({ default: m.LoginPage })))
const RegisterPage = lazy(() => import("@/pages/register.page").then((m) => ({ default: m.RegisterPage })))
const CatalogPage = lazy(() => import("@/pages/catalog.page").then((m) => ({ default: m.CatalogPage })))
const EventDetailPage = lazy(() => import("@/pages/event-detail.page").then((m) => ({ default: m.EventDetailPage })))
const CartPage = lazy(() => import("@/pages/cart.page").then((m) => ({ default: m.CartPage })))
const OrdersPage = lazy(() => import("@/pages/orders.page").then((m) => ({ default: m.OrdersPage })))
const OrderDetailPage = lazy(() => import("@/pages/order-detail.page").then((m) => ({ default: m.OrderDetailPage })))
const PaymentsPage = lazy(() => import("@/pages/payments.page").then((m) => ({ default: m.PaymentsPage })))
const TicketsPage = lazy(() => import("@/pages/tickets.page").then((m) => ({ default: m.TicketsPage })))
const WaitingListPage = lazy(() => import("@/pages/waiting-list.page").then((m) => ({ default: m.WaitingListPage })))
const ProfilePage = lazy(() => import("@/pages/profile.page").then((m) => ({ default: m.ProfilePage })))
const CheckInPage = lazy(() => import("@/pages/check-in.page").then((m) => ({ default: m.CheckInPage })))
const NotFoundPage = lazy(() => import("@/pages/not-found.page").then((m) => ({ default: m.NotFoundPage })))

const EventsAdminPage = lazy(() => import("@/pages/admin/events-admin.page").then((m) => ({ default: m.EventsAdminPage })))
const EventFormPage = lazy(() => import("@/pages/admin/event-form.page").then((m) => ({ default: m.EventFormPage })))
const CategoriesAdminPage = lazy(() => import("@/pages/admin/categories-admin.page").then((m) => ({ default: m.CategoriesAdminPage })))
const PromoCodesAdminPage = lazy(() => import("@/pages/admin/promo-codes-admin.page").then((m) => ({ default: m.PromoCodesAdminPage })))

export function App() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-svh items-center justify-center">
          <LoadingState />
        </div>
      }
    >
      <Routes>
        {/* Public storefront: browsing never requires a login. */}
        <Route element={<AppShell />}>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/events/:eventId" element={<EventDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/cart" element={<CartPage />} />
            <Route path="/orders" element={<OrdersPage />} />
            <Route path="/orders/:orderId" element={<OrderDetailPage />} />
            <Route path="/payments" element={<PaymentsPage />} />
            <Route path="/tickets" element={<TicketsPage />} />
            <Route path="/waiting-list" element={<WaitingListPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/check-in" element={<CheckInPage />} />

            <Route
              element={
                <RequirePermission permission="events:update">
                  <Outlet />
                </RequirePermission>
              }
            >
              <Route path="/admin" element={<Navigate to="/admin/events" replace />} />
              <Route path="/admin/events" element={<EventsAdminPage />} />
              <Route path="/admin/events/new" element={<EventFormPage />} />
              <Route path="/admin/events/:eventId/edit" element={<EventFormPage />} />
              <Route path="/admin/categories" element={<CategoriesAdminPage />} />
              <Route path="/admin/promo-codes" element={<PromoCodesAdminPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  )
}
