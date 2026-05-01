# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/ ./
WORKDIR "/src/src/Faryma.Composer.Api"

# Publish stage
FROM build AS publish
RUN --mount=type=cache,id=nuget-publish,target=/root/.nuget/packages \
    dotnet publish "Faryma.Composer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Migration bundle stage
FROM build AS migrations
WORKDIR "/src/src/Faryma.Composer.MigrationsBundle"
RUN --mount=type=cache,id=nuget-migrations,target=/root/.nuget/packages \
    dotnet restore "Faryma.Composer.MigrationsBundle.csproj" && \
    dotnet new tool-manifest && \
    dotnet tool install dotnet-ef -v d --version 10.0.* && \
    dotnet ef migrations bundle -v -r linux-x64 -o /app/migrations-bundle && \
    chmod +x /app/migrations-bundle

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ARG APP_UID=10001
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
COPY --from=migrations /app/migrations-bundle ./migrations-bundle

COPY backend/entrypoint.sh .
RUN groupadd --gid "${APP_UID}" appuser && \
    useradd \
        --uid "${APP_UID}" \
        --gid "${APP_UID}" \
        --home-dir "/nonexistent" \
        --shell "/usr/sbin/nologin" \
        --no-create-home \
        appuser && \
    chmod +x entrypoint.sh && \
    chown -R appuser:appuser /app

USER appuser
ENTRYPOINT ["./entrypoint.sh"]