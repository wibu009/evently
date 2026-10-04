export const config = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000",
  ticketingBaseUrl: import.meta.env.VITE_TICKETING_BASE_URL ?? "http://localhost:5004",
  oidc: {
    authority: import.meta.env.VITE_OIDC_AUTHORITY ?? "http://localhost:18080/realms/evently",
    clientId: import.meta.env.VITE_OIDC_CLIENT_ID ?? "evently-public-client"
  }
} as const
