# Catalog DB migrator — applies EF migrations to PostgreSQL (idempotent script).
# Build context = repo root.
#   docker build -f src/catalog-service/Catalog.Migrator.Dockerfile .

ARG DOTNET_VERSION=10.0
ARG POSTGRES_VERSION=17

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY dotnet-tools.json ./
RUN dotnet tool restore

COPY src/catalog-service/Catalog.Domain/Catalog.Domain.csproj           src/catalog-service/Catalog.Domain/
COPY src/catalog-service/Catalog.Application/Catalog.Application.csproj src/catalog-service/Catalog.Application/
COPY src/catalog-service/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/catalog-service/Catalog.Infrastructure/
RUN dotnet restore src/catalog-service/Catalog.Infrastructure/Catalog.Infrastructure.csproj

COPY src/catalog-service/Catalog.Domain/         src/catalog-service/Catalog.Domain/
COPY src/catalog-service/Catalog.Application/    src/catalog-service/Catalog.Application/
COPY src/catalog-service/Catalog.Infrastructure/ src/catalog-service/Catalog.Infrastructure/

RUN dotnet tool run dotnet-ef migrations script --idempotent \
    --project src/catalog-service/Catalog.Infrastructure/Catalog.Infrastructure.csproj \
    --output /migrate.sql

FROM postgres:${POSTGRES_VERSION} AS migrator
COPY --from=build /migrate.sql /migrate.sql
ENTRYPOINT ["sh", "-c", "psql \"$CATALOG_DB_CONNECTION\" -v ON_ERROR_STOP=1 -f /migrate.sql"]
