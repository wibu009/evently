# Evently

Evently is a modular event management API built with ASP.NET Core. This project is in its early stages.

## Getting Started
You can run the project using Docker (recommended) or with .NET 9 SDK and PostgreSQL installed locally. Once running, access the API docs at `http://localhost:5000/scalar` (HTTP) or `https://localhost:5001/scalar` (HTTPS) to check it out!

### With Docker
1. Make sure you have Docker installed.
2. From the project root, run:
   ```sh
   docker-compose up
   ```

### With .NET 9 SDK
1. Configure your database connection in `src/API/Evently.Api/appsettings.Development.json`.
2. From `src/API/Evently.Api`, run:
   ```sh
   dotnet run
   ```

More details and modules will be added as the project evolves.
