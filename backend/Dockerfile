# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Faryma.Composer.Api/Faryma.Composer.Api.csproj", "src/Faryma.Composer.Api/"]
COPY ["src/Faryma.Composer.Infrastructure/Faryma.Composer.Infrastructure.csproj", "src/Faryma.Composer.Infrastructure/"]
RUN dotnet restore "src/Faryma.Composer.Api/Faryma.Composer.Api.csproj"
COPY . .
WORKDIR "/src/src/Faryma.Composer.Api"
RUN dotnet build "Faryma.Composer.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Faryma.Composer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Migration bundle stage
FROM build AS migrations
WORKDIR "/src/src/Faryma.Composer.Infrastructure"
RUN dotnet new tool-manifest && \
    dotnet tool install dotnet-ef -v d && \
    dotnet ef migrations bundle -v --self-contained -r linux-x64 -o /app/migrations-bundle

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
COPY --from=migrations /app/migrations-bundle ./migrations-bundle

COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT [\"./entrypoint.sh\"]