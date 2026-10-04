# Order Fulfillment Saga — Ticketing Module

## Overview

Selling tickets is a distributed transaction across three concerns that must never drift apart:

1. **Inventory** — `ticket_types.available_quantity` must not oversell and must not leak stock.
2. **Money** — a payment must exist for every confirmed order and every order must have exactly one definitive outcome.
3. **Tickets** — customers must only receive tickets for orders that were actually paid.

The Ticketing module implements this as an **orchestrated saga** built from the module's existing
primitives (transactional outbox, idempotent domain-event handlers, and a Quartz-based timeout
sweeper). It is conceptually identical to the event-cancellation state machine saga in the Events
module, but the state lives *in the aggregates themselves* (`Order.Status`, `Payment.Status`),
which keeps the database the single source of truth.

## The happy path

```
Checkout (POST /orders)
  └─ CreateOrderCommandHandler  [single DB transaction, pessimistic row locks]
       ├─ reserve stock:      ticket_type.UpdateQuantity()      (FOR UPDATE NOWAIT)
       ├─ create order:       Order.Create(customer, paymentDueUtc)   → Status = Pending
       ├─ create payment:     Payment.Create(order, amount, currency) → Status = Pending
       ├─ save + commit       (domain events → ticketing.outbox_messages)
       └─ clear the cart      (Redis, after commit)

Outbox → PaymentCreatedDomainEvent → ProcessPaymentCommand   [outside any DB transaction]
  └─ payment gateway charge (IPaymentService)
       ├─ success → payment.Succeed(txId) + order.MarkAsPaid()
       │             outbox → OrderPaidDomainEvent
       │                       ├─ CreateTicketBatchCommand → tickets issued → TicketIssued events
       │                       └─ order-confirmation notification (INotificationService)
       └─ failure → payment.Fail(reason)
                     outbox → PaymentFailedDomainEvent
                                └─ CancelOrderCommand (compensation, see below)
```

## Timeout and compensations

The saga has three terminal failure outcomes, each with its own compensation:

| Trigger                                  | Order state | Compensation steps                                                        |
|------------------------------------------|-------------|---------------------------------------------------------------------------|
| Gateway rejects the charge               | `Canceled`  | release reserved inventory (`RestockOrderItemsCommand`) + customer notice |
| Customer abandons checkout (deadline)    | `Expired`   | payment marked `Failed` + release inventory + customer notice             |
| Customer cancels a **pending** order     | `Canceled`  | release inventory + customer notice                                       |
| Customer cancels a **paid** order        | `Refunded`  | gateway refund + archive issued tickets + release inventory + notice      |

The **timeout** of the saga is enforced by `ProcessOrderExpirationsJob` (Quartz). Every
`Ticketing:Orders:ExpirationIntervalInSeconds` it finds orders with
`status = Pending AND payment_due_utc < now()` and sends `ExpireOrderCommand`, which is
idempotent and therefore safe to race against a late payment result.

Inventory release (`TicketType.Restock`) is guarded in the domain: it cannot push
`available_quantity` above the original capacity, which makes double-restock bugs impossible.

## Refunds

* `RefundOrderCommand` (used by canceling a paid order) refunds the remaining balance of the
  order's payment; a full refund flips the order to `Refunded`, which fans out to
  ticket archival (`ArchiveOrderTicketsCommand`) and restock.
* `RefundPaymentCommand` (admin endpoint `PUT payments/{id}/refund`) supports partial refunds;
  only a fully refunded payment transitions the order.
* `RefundPaymentsForEventCommand` (event-cancellation saga) also transitions fully refunded
  orders to `Refunded`, so event cancellations no longer leave orders stuck in `Pending`.

The actual gateway refund is executed asynchronously through the outbox
(`PaymentRefundedDomainEventHandler`); the Stripe implementation deduplicates refunds with
an idempotency key derived from the transaction reference and the amount.

## Payment gateways

The gateway sits behind `IPaymentService`:

* **Stripe** (`StripePaymentService`) — used when `Ticketing:Stripe:Enabled` is `true` and a
  secret key is configured. Charges create a server-side confirmed PaymentIntent
  (`confirm=true`), whose id becomes the payment's `TransactionReference`. Every request
  carries an idempotency key (`charge:{paymentId}` / `refund:{reference}:{amountMinor}`), so
  outbox retries can never double-charge or double-refund. The charge uses
  `Ticketing:Stripe:DefaultPaymentMethod` (e.g. the `pm_card_visa` test token); in production,
  payment methods are collected on the client with Stripe.js.
* **Fake** (`FakePaymentService`) — the default when Stripe is not configured, so the local
  docker-compose stack works without external dependencies.

## Promo codes

Promo codes are created by admins (`POST promo-codes`) as percentage or fixed-amount discounts
with optional redemption limits and validity windows. Checkout applies the code
(`POST /orders` with a promo code); the discount is validated against the code's currency and
validity, and the pending payment is created for the **net** amount. Redemptions are only
counted when the order is **paid** (`RedeemPromoCodeCommand` via the outbox), so abandoned
checkouts never burn a limited code.

## Waiting list

Customers join the waiting list of a sold out ticket type (`PUT waiting-lists/{id}/join`).
Whenever the fulfillment saga releases inventory back into a ticket type
(`TicketTypeRestockedDomainEvent` — canceled, expired, or refunded orders), the earliest
waiting customers are notified in FIFO order (`NotifyWaitingListCommand`).

## Invariants enforced by the design

* A customer can never receive tickets for an unpaid order (issuance is triggered by `OrderPaidDomainEvent` only).
* Stock can never go negative (pessimistic lock + domain guard) and never exceed capacity after restock.
* Every pending order has a bounded lifetime (`PaymentDueUtc`), so abandoned checkouts cannot lock inventory forever.
* Every saga step is an idempotent handler (outbox-consumer table) — replaying messages is safe.
* Every order ends in exactly one terminal state: `Paid`, `Canceled`, `Expired`, or `Refunded`.

## Configuration (`Ticketing:Orders`)

| Key                          | Default | Meaning                                        |
|------------------------------|---------|------------------------------------------------|
| `PaymentTimeoutMinutes`      | 15      | How long a pending order holds inventory       |
| `ExpirationIntervalInSeconds`| 60      | Sweep interval of the expiration job           |
| `ExpirationBatchSize`        | 100     | Max stale orders expired per sweep             |

## Stripe configuration (`Ticketing:Stripe`)

| Key                    | Default | Meaning                                                                 |
|------------------------|---------|-------------------------------------------------------------------------|
| `Enabled`              | false   | When `false`, the in-memory fake gateway is used                        |
| `SecretKey`            | ""      | The Stripe secret key (`sk_test_...` / `sk_live_...`)                   |
| `DefaultPaymentMethod` | ""      | The payment method used for server-side confirmed charges (e.g. `pm_card_visa` in test mode) |
