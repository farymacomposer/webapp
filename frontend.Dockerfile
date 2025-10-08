# Build stage
FROM node:20-alpine AS builder
WORKDIR /src
COPY frontend/package*.json ./
COPY frontend/pnpm-lock.yaml* ./
RUN npm install -g pnpm && pnpm install --frozen-lockfile
COPY frontend/ .
RUN pnpm run build

# Production stage
FROM node:20-alpine AS runner
WORKDIR /src

ENV NODE_ENV production
ENV PORT 3000

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /src/public ./public

# Automatically leverage output traces to reduce image size
COPY --from=builder --chown=nextjs:nodejs /src/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /src/.next/static ./.next/static

USER nextjs

EXPOSE 3000

CMD ["node", "server.js"]