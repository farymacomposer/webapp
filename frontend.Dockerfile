# Base stage
FROM node:jod-alpine AS base
WORKDIR /src
COPY frontend/package.json frontend/pnpm-lock.yaml frontend/pnpm-workspace.yaml ./
RUN apk add --no-cache libc6-compat && \
    corepack enable pnpm && \
    pnpm install --frozen-lockfile

# Build stage
FROM base AS build
ARG NEXT_PUBLIC_API_BASE_URL
ARG NEXT_PUBLIC_TWITCH_CLIENT_ID
ARG NEXT_PUBLIC_TWITCH_REDIRECT_URI
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
ENV NEXT_PUBLIC_TWITCH_CLIENT_ID=$NEXT_PUBLIC_TWITCH_CLIENT_ID
ENV NEXT_PUBLIC_TWITCH_REDIRECT_URI=$NEXT_PUBLIC_TWITCH_REDIRECT_URI
COPY frontend/ .
RUN pnpm run build

# Runtime stage
FROM node:jod-alpine AS final
WORKDIR /app
EXPOSE 3000

USER node

COPY --from=build /src/public ./public
COPY --from=build --chown=node:node /src/.next/standalone ./
COPY --from=build --chown=node:node /src/.next/static ./.next/static

CMD ["node", "server.js"]