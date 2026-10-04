# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy everything first: target frameworks live in the root Directory.Build.props and
# restore resolves referenced module projects, so a csproj-only copy is not enough.
COPY . .
RUN dotnet restore "src/API/Evently.Ticketing.Api/Evently.Ticketing.Api.csproj"

WORKDIR "/src/src/API/Evently.Ticketing.Api"
RUN dotnet build "./Evently.Ticketing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Evently.Ticketing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Evently.Ticketing.Api.dll"]
