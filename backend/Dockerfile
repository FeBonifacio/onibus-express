# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (layer cache): copy only the csproj graph the API needs.
COPY src/OnibusExpress.Domain/*.csproj         src/OnibusExpress.Domain/
COPY src/OnibusExpress.Application/*.csproj     src/OnibusExpress.Application/
COPY src/OnibusExpress.Infrastructure/*.csproj  src/OnibusExpress.Infrastructure/
COPY src/OnibusExpress.Api/*.csproj             src/OnibusExpress.Api/
RUN dotnet restore src/OnibusExpress.Api/OnibusExpress.Api.csproj

COPY src/ src/
RUN dotnet publish src/OnibusExpress.Api/OnibusExpress.Api.csproj \
    -c Release -o /app /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG APP_VERSION=0.0.0-dev
ENV APP_VERSION=${APP_VERSION} \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "OnibusExpress.Api.dll"]
