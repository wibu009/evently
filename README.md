# Evently

Evently is a modular event management system built with .NET 9, following Clean Architecture principles and Domain-Driven Design (DDD). It demonstrates a transition from a Modular Monolith to a Microservices architecture.

## 🚀 Technologies

- **Framework**: ASP.NET Core 9
- **Databases**:
  - PostgreSQL (Write Model)
  - MongoDB (Read Model)
- **Message Broker**: RabbitMQ (MassTransit)
- **Caching**: Redis (Hybrid Caching)
- **Identity Provider**: Keycloak
- **Background Jobs**: Quartz.NET
- **Observability**:
  - OpenTelemetry
  - Jaeger (Tracing)
  - Seq (Logging)
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **API Documentation**: Scalar

## 📦 Modules

The system is divided into the following modules:

- **Users**: User management and authentication.
- **Events**: Event creation, scheduling, and management.
- **Ticketing**: Ticket sales and inventory management (Microservice).
- **Attendance**: Tracking event attendance.

## 🛠️ Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (optional, for local development)

### Run with Docker (Recommended)

The easiest way to run the entire system is using Docker Compose.

1. Clone the repository.
2. Navigate to the project root.
3. Run the following command:

   ```bash
   docker-compose up -d
   ```

### Services & Ports

| Service | URL | Description |
|---------|-----|-------------|
| **Evently API** | `http://localhost:5000` | Main API Gateway / Monolith |
| **Ticketing API** | `http://localhost:5004` | Ticketing Microservice |
| **Gateway** | `http://localhost:5002` | API Gateway (YARP) |
| **Keycloak** | `http://localhost:18080` | Identity Provider (admin/admin) |
| **Seq** | `http://localhost:8082` | Logging Dashboard |
| **Jaeger** | `http://localhost:16686` | Tracing Dashboard |
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

## 📚 Documentation

API documentation is split between the services:
- **Core Modules**: `http://localhost:5000/scalar` (Users, Events, Attendance)
- **Ticketing Module**: `http://localhost:5004/scalar`

## 🧪 Testing

To run the tests, use the following command in the solution directory:

```bash
dotnet test
```

## 🔧 Troubleshooting

- If you encounter issues, check the logs of the individual services using:

  ```bash
  docker-compose logs -f
  ```

- For database migrations, use the following command:

  ```bash
  dotnet ef database update
  ```

  Ensure the correct connection string is set in the environment variables.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
