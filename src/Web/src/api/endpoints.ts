import { api } from "@/lib/http"
import type {
  Cart,
  CategoryResponse,
  EventDetail,
  EventStatistics,
  GetEventsResponse,
  GetOrdersResponse,
  GetPaymentsResponse,
  GetPromoCodesResponse,
  OrderResponse,
  PaymentResponse,
  SearchEventsResponse,
  TicketResponse,
  UserProfile,
  WaitingListEntryResponse
} from "@/api/types"

// ─── Events module ────────────────────────────────────────────────────────────

export interface SearchEventsParams {
  search?: string
  categoryId?: string
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}

export const eventsApi = {
  search: (params: SearchEventsParams, signal?: AbortSignal) => {
    const query = new URLSearchParams()
    if (params.search) query.set("search", params.search)
    if (params.categoryId) query.set("categoryId", params.categoryId)
    if (params.startDate) query.set("startDate", params.startDate)
    if (params.endDate) query.set("endDate", params.endDate)
    query.set("page", String(params.page ?? 1))
    query.set("pageSize", String(params.pageSize ?? 12))

    return api.get<SearchEventsResponse>(`/events/search?${query.toString()}`, signal)
  },
  listAll: (page = 1, pageSize = 12, signal?: AbortSignal) =>
    api.get<GetEventsResponse>(`/events?page=${page}&pageSize=${pageSize}`, signal),
  get: (eventId: string, signal?: AbortSignal) => api.get<EventDetail>(`/events/${eventId}`, signal),
  create: (body: { categoryId: string; title: string; description: string; location: string; startAtUtc: string; endAtUtc?: string | null }) =>
    api.post<string>("/events", body),
  update: (eventId: string, body: { title: string; description: string; location: string }) =>
    api.put<void>(`/events/${eventId}`, body),
  updateAppearance: (eventId: string, body: { heroBannerUrl?: string | null; accentColor?: string | null }) =>
    api.put<void>(`/events/${eventId}/appearance`, body),
  addImage: (eventId: string, body: { imageUrl: string; setAsCover: boolean }) =>
    api.post<{ id: string }>(`/events/${eventId}/images`, body),
  removeImage: (eventId: string, imageId: string) => api.delete<void>(`/events/${eventId}/images/${imageId}`),
  setCoverImage: (eventId: string, imageId: string) => api.put<void>(`/events/${eventId}/images/${imageId}/cover`),
  publish: (eventId: string) => api.put<void>(`/events/${eventId}/publish`),
  complete: (eventId: string) => api.put<void>(`/events/${eventId}/complete`),
  reschedule: (eventId: string, body: { startAtUtc: string; endAtUtc?: string | null }) =>
    api.put<void>(`/events/${eventId}/reschedule`, body),
  cancel: (eventId: string) => api.delete<void>(`/events/${eventId}/cancel`)
}

export const ticketTypesApi = {
  listForEvent: (eventId: string, signal?: AbortSignal) =>
    api.get<TicketTypeResponses>(`/ticket-types?eventId=${eventId}`, signal),
  create: (body: { eventId: string; name: string; price: number; currency: string; quantity: number }) =>
    api.post<string>("/ticket-types", body),
  updatePrice: (ticketTypeId: string, price: number) => api.put<void>(`/ticket-types/${ticketTypeId}/price`, { price }),
  updateDesign: (ticketTypeId: string, body: { color?: string | null; backgroundImageUrl?: string | null }) =>
    api.put<void>(`/ticket-types/${ticketTypeId}/design`, body)
}

export interface TicketTypeResponses {
  ticketTypes: TicketTypeListResponse[]
}

export interface TicketTypeListResponse {
  ticketTypeId: string
  eventId: string
  name: string
  price: number
  currency: string
  quantity: number
  color: string | null
  backgroundImageUrl: string | null
}

export const categoriesApi = {
  list: (signal?: AbortSignal) => api.get<CategoryResponse[]>("/categories", signal),
  get: (categoryId: string) => api.get<CategoryResponse>(`/categories/${categoryId}`),
  create: (name: string) => api.post<string>("/categories", { name }),
  update: (categoryId: string, name: string) => api.put<void>(`/categories/${categoryId}`, { name }),
  archive: (categoryId: string) => api.delete<void>(`/categories/${categoryId}/archive`)
}

export const statisticsApi = {
  forEvent: (eventId: string, signal?: AbortSignal) =>
    api.get<EventStatistics>(`/event-statistics/${eventId}`, signal),
  checkInByCode: (ticketCode: string) =>
    api.put<CheckInResult>(`/attendees/check-in-by-code`, { ticketCode })
}

export interface CheckInResult {
  outcome: "CheckedIn" | "Duplicate" | "Invalid" | "NotFound" | string
  attendeeId: string | null
  eventId: string | null
}

// ─── Users module ─────────────────────────────────────────────────────────────

export const usersApi = {
  myProfile: (signal?: AbortSignal) => api.get<UserProfile>("/users/my-profile", signal),
  myPermissions: (signal?: AbortSignal) => api.get<PermissionsResponse>("/users/my-permissions", signal),
  updateProfile: (userId: string, body: { firstName: string; lastName: string }) =>
    api.put<void>(`/users/${userId}/profile`, body)
}

export interface PermissionsResponse {
  userId: string
  permissions: string[]
}

// ─── Ticketing module ─────────────────────────────────────────────────────────

export const cartApi = {
  get: (signal?: AbortSignal) => api.ticketing.get<Cart>("/carts", signal),
  addItem: (body: { ticketTypeId: string; quantity: number }) => api.ticketing.put<void>("/carts/add", body),
  removeItem: (ticketTypeId: string) => api.ticketing.put<void>("/carts/remove", { ticketTypeId }),
  clear: () => api.ticketing.delete<void>("/carts")
}

export const ordersApi = {
  checkout: (promoCode?: string) =>
    api.ticketing
      .post<{ id: string }>("/orders", promoCode ? { promoCode } : { promoCode: null })
      .then((body) => body.id),
  list: (page = 1, pageSize = 10, signal?: AbortSignal) =>
    api.ticketing.get<GetOrdersResponse>(`/orders?page=${page}&pageSize=${pageSize}`, signal),
  get: (orderId: string, signal?: AbortSignal) => api.ticketing.get<OrderResponse>(`/orders/${orderId}`, signal),
  cancel: (orderId: string, reason?: string) => api.ticketing.put<void>(`/orders/${orderId}/cancel`, { reason: reason ?? null })
}

export const paymentsApi = {
  list: (page = 1, pageSize = 10, signal?: AbortSignal) =>
    api.ticketing.get<GetPaymentsResponse>(`/payments?page=${page}&pageSize=${pageSize}`, signal),
  get: (paymentId: string, signal?: AbortSignal) => api.ticketing.get<PaymentResponse>(`/payments/${paymentId}`, signal),
  refund: (paymentId: string, amount?: number) =>
    api.ticketing.put<void>(`/payments/${paymentId}/refund`, { amount: amount ?? null })
}

export const ticketsApi = {
  forOrder: (orderId: string, signal?: AbortSignal) =>
    api.ticketing.get<TicketResponse[]>(`/tickets/order/${orderId}`, signal),
  get: (ticketId: string, signal?: AbortSignal) => api.ticketing.get<TicketResponse>(`/tickets/${ticketId}`, signal),
  getByCode: (code: string, signal?: AbortSignal) => api.ticketing.get<TicketResponse>(`/tickets/code/${code}`, signal),
  transfer: (ticketId: string, toCustomerId: string) => api.ticketing.put<void>(`/tickets/${ticketId}/transfer`, { toCustomerId })
}

export const waitingListApi = {
  myEntries: (signal?: AbortSignal) => api.ticketing.get<WaitingListEntryResponse[]>("/waiting-lists", signal),
  join: (ticketTypeId: string) => api.ticketing.put<void>(`/waiting-lists/${ticketTypeId}/join`),
  leave: (ticketTypeId: string) => api.ticketing.put<void>(`/waiting-lists/${ticketTypeId}/leave`)
}

export const promoCodesApi = {
  list: (page = 1, pageSize = 10, signal?: AbortSignal) =>
    api.ticketing.get<GetPromoCodesResponse>(`/promo-codes?page=${page}&pageSize=${pageSize}`, signal),
  create: (body: {
    code: string
    discountType: number
    discountValue: number
    currency: string
    maxRedemptions?: number | null
    validFromUtc?: string | null
    validUntilUtc?: string | null
  }) => api.ticketing.post<string>("/promo-codes", body),
  deactivate: (promoCodeId: string) => api.ticketing.put<void>(`/promo-codes/${promoCodeId}/deactivate`)
}
