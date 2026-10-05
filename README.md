# Evently

Evently is a modular event management system built with .NET 9, following Clean Architecture principles and Domain-Driven Design (DDD). It demonstrates a transition from a Modular Monolith to a Microservices architecture.

## 🚀 Technologies

- **Framework**: ASP.NET Core 9
- **Databases**:
  - PostgreSQL (Write Model)
  - MongoDB (Read Model)
- **Message Broker**: RabbitMQ (MassTransit)
- **Caching**: Redis (Hybrid Caching)
- **Identity Provider**: Keycloak (branded in-app login/signup pages)
- **Background Jobs**: Quartz.NET
- **Orchestration**: .NET Aspire 13 (AppHost + ServiceDefaults) with Docker Compose as an alternative
- **Observability**:
  - OpenTelemetry
  - Aspire dashboard (dev) / Jaeger (Compose)
  - Seq (Logging)
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **API Documentation**: Scalar

## 📦 Modules

The system is divided into the following modules:

- **Users**: User management and authentication.
- **Events**: Event creation, scheduling, and management.
- **Ticketing**: Ticket sales and inventory management (Microservice).
- **Attendance**: Tracking event attendance.

## 🚀 Getting Started

### Option A — .NET Aspire (recommended)

The whole system is defined as an Aspire app model in `src/Aspire/Evently.AppHost`:
Postgres, MongoDB, Redis, RabbitMQ, Keycloak, and Seq run as managed containers, the three
.NET hosts run as projects, and everything is visible in the **Aspire dashboard**
(unified logs, structured logs, traces, metrics, health).

```bash
dotnet run --project src/Aspire/Evently.AppHost
# storefront is orchestrated too: http://localhost:5173
# (run `npm install` in src/Web first if node_modules is missing)
```

The dashboard URL is printed on startup. The AppHost injects all connection strings,
Keycloak URLs, and Seq endpoints into the projects automatically (service discovery +
endpoint references), so no manual configuration is needed. The React storefront is
orchestrated by the AppHost as well (`web` resource, `npm run dev` under the hood) and
inherits the same env vars.

### Option B — Docker Compose

Everything also runs containerized via `docker-compose.yml` (the UI at `http://localhost:3000`
via the `evently.web` service):

```bash
docker compose up -d --build
```

That single command is enough on a machine with only Docker installed: each app service
waits (via health checks) for Postgres, MongoDB, Redis, RabbitMQ, and Keycloak to become
healthy, applies its migrations, and — in the Development environment this compose file
sets — seeds the sample catalog and demo accounts automatically.

To wipe all state and reseed from scratch:

```bash
docker compose down
Remove-Item -Recurse -Force ./.containers   # PowerShell; `rm -rf ./.containers` on Linux/macOS
docker compose up -d --build
```

### Keycloak & realm configuration

- The Keycloak **admin account (`admin`/`admin`), `start-dev`, and plain-HTTP transport are
  dev-only defaults** — never use them outside local development. Production requires a
  managed Keycloak (or equivalent) with HTTPS, a real admin credential, and a hardened realm.
- All realm configuration lives in [`.files/evently-realm-export.json`](.files/evently-realm-export.json)
  and is imported on startup (`--import-realm`). **The import only runs when the realm does
  not exist** — after changing the realm JSON, existing environments must wipe their Keycloak
  data to pick up the changes:
  - Docker Compose: stop the stack and delete `./.containers/identity`
  - .NET Aspire: stop the AppHost and run `docker volume rm evently-keycloak-data`
- The login/signup UI is a [Keycloakify](https://www.keycloakify.com) theme
  (`src/KeycloakTheme`) built to a JAR and bind-mounted to `/opt/keycloak/providers/`
  (see "Building the login theme" below).

### Signing in

The app uses **branded in-app login/signup pages** (`/login`, `/register`) with Keycloak
as the identity provider under the hood (direct-access password + refresh grants against
the `evently-public-client`). Users never see Keycloak's pages. Registration provisions
the Keycloak account and signs the user straight in.

Browsing the event catalog and event detail pages is **public** — no login required.
Cart, orders, tickets, and the admin/check-in areas require authentication.

### Run with Docker (Recommended)

The easiest way to run the entire system is using Docker Compose.

1. Clone the repository.
2. Navigate to the project root.
3. Run the following command:

   ```bash
   docker-compose up -d
   ```

### Sample Data (Development)

In development, the API seeds a rich sample data set on startup (configurable via `SampleData:Enabled`):
six categories with three published events each (every event carrying two ticket types —
36 sellable ticket types in total), and two demo accounts (registered in Keycloak):

| Account | Email | Password |
|---------|-------|----------|
| Demo admin* | `admin@evently.local` | `Evently123!` |
| Demo member | `member@evently.local` | `Evently123!` |

\* Registered with the default Member role — the role assignment endpoint does not exist yet, so
administrative endpoints require promoting the user's role in the Users database or via Keycloak.

### Services & Ports

| Service | URL | Description |
|---------|-----|-------------|
| **Evently Web UI** | `http://localhost:5173` (Aspire) / `:3000` (Compose) | React storefront (shadcn/ui) |
| **Aspire dashboard** | printed on `dotnet run` (AppHost) | Logs, traces, metrics, health |
| **Evently API** | `http://localhost:5000` | Main API Gateway / Monolith |
| **Ticketing API** | `http://localhost:5004` | Ticketing Microservice |
| **Gateway (YARP)** | `http://localhost:5002` | API Gateway |
| **Keycloak (admin/admin)** | `http://localhost:18080` | Identity Provider |
| **Seq** | `http://localhost:8082` (Compose) | Structured log dashboard |
| **Jaeger** | `http://localhost:16686` (Compose) | Tracing dashboard |
| **RabbitMQ management (guest/guest)** | `http://localhost:15672` (Compose) | Message broker UI |
| **Scalar Docs (Core)** | `http://localhost:5000/scalar` | API Docs (Users, Events, Attendance) |
| **Scalar Docs (Ticketing)** | `http://localhost:5004/scalar` | API Docs (Ticketing) |

## 🏗️ Architecture

Evently follows a **Modular Monolith** architecture where modules are loosely coupled. Some modules, like **Ticketing**, are extracted into separate services to demonstrate microservices capabilities.

- **Domain-Driven Design (DDD)**: Rich domain models with aggregates, entities, and value objects.
- **CQRS**: Command Query Responsibility Segregation using MediatR.
- **Event-Driven Architecture**: Asynchronous communication between modules using integration events.
- **Outbox Pattern**: Reliable event publishing.
- **Inbox Pattern**: Idempotent event processing.
- **Saga Pattern**: Managing long-running distributed transactions (e.g., using MassTransit).
  - **Order Fulfillment Saga** (Ticketing): checkout reserves inventory transactionally, payment is processed asynchronously through the outbox, and a Quartz timeout sweeper expires unpaid orders and releases the reserved inventory as compensation. See [docs/order-fulfillment-saga.md](docs/order-fulfillment-saga.md).
  - **Cancellation & Refund flow** (Ticketing): pending orders cancel with inventory release; paid orders are refunded, their tickets are archived and the inventory is restocked.
  - **Event Cancellation Saga** (Events): canceling an event orchestrates refunds and ticket archival in the Ticketing module via a MassTransit state machine.

## 🎟️ Ticketing Business Features

- **Time-limited reservations**: checkout holds inventory with a configurable payment deadline (`Ticketing:Orders`).
- **Full order lifecycle**: `Pending → Paid → Refunded` plus `Canceled` / `Expired` outcomes with automatic inventory release.
- **Stripe payments**: server-side confirmed PaymentIntents with per-payment idempotency keys and partial/full refunds (`Ticketing:Stripe`); a built-in fake gateway keeps local development self-contained.
- **Promo codes**: percentage/fixed discounts with redemption limits and validity windows; redemptions are only counted for paid orders.
- **Waiting list**: join sold out ticket types and get notified in FIFO order when refunds/cancellations release inventory.
- **Ticket transfer**: hand a ticket to another customer, synced to the attendance module so only the new owner can check in.
- **Full & partial refunds**: admin refunds (`payments:refund` permission) and customer cancellations with compensation steps.
- **Oversell protection**: pessimistic row locks at checkout; restocks are capacity-guarded.
- **Paged order & payment APIs**, order cancellation, and payment query endpoints.
- **Event visuals**: gallery images with cover selection, custom hero banners and accent colors per event, and customizable ticket face designs (color + backdrop) — all display-only, no cross-module fan-out.
- **QR gate check-in**: tickets render scannable QR codes; staff scan them at `/check-in` (camera via the native BarcodeDetector API, or manual code entry). Each scan is a single indexed lookup returning an explicit outcome (`CheckedIn`/`Duplicate`/`Invalid`/`NotFound`) — no retries, no bottlenecks at crowd scale.

## 📚 Documentation

API documentation is split between the services:
- **Core Modules**: `http://localhost:5000/scalar` (Users, Events, Attendance)
- **Ticketing Module**: `http://localhost:5004/scalar`

The order fulfillment saga (checkout → payment → tickets, with cancellation, expiration,
refunds, and inventory compensation) is documented in
[docs/order-fulfillment-saga.md](docs/order-fulfillment-saga.md).

## 🧪 Testing

To run the tests, use the following command in the solution directory:

```bash
dotnet test
```

Integration tests spin up their own Postgres/MongoDB containers via Testcontainers, so a
running Docker daemon is required; no other environment setup is needed.

## 🔧 Troubleshooting

- Check the logs of the individual services:

  ```bash
  docker compose logs -f
  ```

- Each service exposes health endpoints (`/health`, `/alive`); the compose file already
  orders startup by health checks, so a service that restarts is usually waiting on an
  infrastructure dependency — check that service's logs first.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
