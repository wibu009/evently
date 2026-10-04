// ─── Events module ────────────────────────────────────────────────────────────

export type EventStatus = "Draft" | "Published" | "Completed" | "Canceled"

export interface EventSummary {
  id: string
  categoryId: string
  title: string
  description: string
  location: string
  startAtUtc: string
  endAtUtc: string | null
  status: EventStatus | string
  coverImageUrl: string | null
}

export interface EventImageResponse {
  imageId: string
  imageUrl: string
  isCover: boolean
  displayOrder: number
}

export interface TicketTypeResponse {
  ticketTypeId: string
  eventId: string
  name: string
  price: number
  currency: string
  quantity: number
  color: string | null
  backgroundImageUrl: string | null
}

export interface EventDetail extends EventSummary {
  heroBannerUrl: string | null
  accentColor: string | null
  images: EventImageResponse[]
  ticketTypes: TicketTypeResponse[]
}

export interface CategoryResponse {
  id: string
  name: string
  isArchived: boolean
}

export interface SearchEventsResponse {
  page: number
  pageSize: number
  totalCount: number
  events: EventSummary[]
}

export interface GetEventsResponse {
  page: number
  pageSize: number
  totalCount: number
  events: EventSummary[]
}

export interface EventStatistics {
  eventId: string
  title: string
  description: string
  location: string
  startAtUtc: string
  endAtUtc: string | null
  ticketsSold: number
  attendeesCheckedIn: number
  duplicateCheckInTickets: TicketStatModel[]
  invalidCheckInTickets: TicketStatModel[]
}

export interface TicketStatModel {
  ticketId: string
  customerId: string
  code: string
}

// ─── Users module ─────────────────────────────────────────────────────────────

export interface UserProfile {
  id: string
  firstName: string
  lastName: string
  email: string
}

// ─── Ticketing module ─────────────────────────────────────────────────────────

export interface CartItem {
  ticketTypeId: string
  quantity: number
  price: number
  currency: string
}

export interface Cart {
  customerId: string
  items: CartItem[]
}

export type OrderStatus = "Pending" | "Paid" | "Refunded" | "Canceled" | "Expired"

export interface OrderItemResponse {
  orderItemId: string
  orderId: string
  ticketTypeId: string
  quantity: number
  unitPrice: number
  price: number
  currency: string
}

export interface OrderResponse {
  id: string
  customerId: string
  status: OrderStatus | string
  totalPrice: number
  discountAmount: number
  currency: string
  createdAtUtc: string
  orderItems?: OrderItemResponse[]
}

export interface GetOrdersResponse {
  page: number
  pageSize: number
  totalCount: number
  orders: OrderResponse[]
}

export interface PaymentResponse {
  id: string
  orderId: string
  transactionReference: string | null
  amount: number
  currency: string
  status: string
  amountRefunded: number
  failureReason: string | null
  createdAtUtc: string
  paidAtUtc: string | null
  refundedAtUtc: string | null
}

export interface GetPaymentsResponse {
  page: number
  pageSize: number
  totalCount: number
  payments: PaymentResponse[]
}

export interface TicketResponse {
  id: string
  customerId: string
  orderId: string
  eventId: string
  ticketTypeId: string
  code: string
  createdAtUtc: string
}

export interface WaitingListEntryResponse {
  id: string
  ticketTypeId: string
  status: string
  createdAtUtc: string
  notifiedAtUtc: string | null
}

export type DiscountType = "Percentage" | "FixedAmount"

export interface PromoCodeResponse {
  id: string
  code: string
  discountType: DiscountType | string
  discountValue: number
  currency: string
  maxRedemptions: number | null
  timesRedeemed: number
  validFromUtc: string
  validUntilUtc: string | null
}

export interface GetPromoCodesResponse {
  page: number
  pageSize: number
  totalCount: number
  promoCodes: PromoCodeResponse[]
}
