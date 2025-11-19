# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend/src/Faryma.Composer.Api/Faryma.Composer.Api.csproj", "src/Faryma.Composer.Api/"]
COPY ["backend/src/Faryma.Composer.MigrationsBundle/Faryma.Composer.MigrationsBundle.csproj", "src/Faryma.Composer.MigrationsBundle/"]
RUN dotnet restore "src/Faryma.Composer.Api/Faryma.Composer.Api.csproj"
COPY backend/ .
WORKDIR "/src/src/Faryma.Composer.Api"
RUN dotnet build "Faryma.Composer.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Faryma.Composer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Migration bundle stage
FROM build AS migrations
WORKDIR "/src/src/Faryma.Composer.MigrationsBundle"
RUN dotnet new tool-manifest && \
    dotnet tool install dotnet-ef -v d --version 9.0.* && \
    dotnet ef migrations bundle -v --self-contained -r linux-x64 -o /app/migrations-bundle && \
    chmod +x /app/migrations-bundle

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
COPY --from=migrations /app/migrations-bundle ./migrations-bundle

COPY backend/entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]