import { StrictMode } from "react"
import { createRoot } from "react-dom/client"

import { BrowserRouter } from "react-router"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"

import { App } from "@/App"
import { ThemeProvider } from "@/components/theme"
import { AuthProvider } from "@/lib/auth-context"
import { PermissionsProvider } from "@/lib/permissions"

import "@fontsource-variable/inter"
import "@fontsource-variable/space-grotesk"
import "./index.css"

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000
    }
  }
})

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <PermissionsProvider>
          <ThemeProvider defaultTheme="dark">
            <BrowserRouter>
              <App />
            </BrowserRouter>
          </ThemeProvider>
        </PermissionsProvider>
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>
)
