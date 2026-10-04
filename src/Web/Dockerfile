# Build stage: installs dependencies and produces the static bundle.
# Vite bakes env vars in at build time; the values must be resolvable from the browser.
FROM node:22-alpine AS build

ARG VITE_API_BASE_URL=http://localhost:5000
ARG VITE_TICKETING_BASE_URL=http://localhost:5004
ARG VITE_OIDC_AUTHORITY=http://localhost:18080/realms/evently
ARG VITE_OIDC_CLIENT_ID=evently-public-client

WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci

COPY . .

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_TICKETING_BASE_URL=$VITE_TICKETING_BASE_URL
ENV VITE_OIDC_AUTHORITY=$VITE_OIDC_AUTHORITY
ENV VITE_OIDC_CLIENT_ID=$VITE_OIDC_CLIENT_ID

RUN npm run build

# Serve stage: static SPA behind nginx with an index.html fallback.
FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
