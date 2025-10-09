# Base stage
FROM node:jod-alpine AS base
WORKDIR /app
COPY frontend/package.json frontend/pnpm-lock.yaml ./
RUN apk add --no-cache libc6-compat && \
    corepack enable && \
    pnpm install --frozen-lockfile

# Build stage
FROM base AS build
COPY frontend/ .
RUN pnpm run build

# Runtime stage
FROM node:jod-alpine AS final
WORKDIR /app
EXPOSE 3000

USER node

COPY --from=build /app/public ./public
COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static

CMD ["node", "server.js"]