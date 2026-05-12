# Build stage
FROM node:24-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

COPY frontend/ .
RUN npm run build

# Runtime stage
FROM nginxinc/nginx-unprivileged:alpine3.23 AS final

COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --chown=nginx:nginx --from=build /app/dist /usr/share/nginx/html

EXPOSE 3000

USER nginx

CMD ["nginx", "-g", "daemon off;"]