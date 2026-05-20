# Sales DB migrator — applies the InitialCreate (and any future) migration to
# PostgreSQL without touching Sales.Api. It is a one-shot container.
#
# Stage 1 (SDK)  : install dotnet-ef from the repo's tool manifest and emit an
#                  idempotent SQL script from the Sales.Infrastructure migrations.
# Stage 2 (psql) : run that script against the database. The script is idempotent,
#                  so re-runs (every `docker compose up`) are safe.
#
# Build context = repo root.
#   docker build -f src/sales-service/Sales.Migrator.Dockerfile .

ARG DOTNET_VERSION=10.0
ARG POSTGRES_VERSION=17

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY dotnet-tools.json ./
RUN dotnet tool restore

COPY src/sales-service/Sales.Domain/Sales.Domain.csproj           src/sales-service/Sales.Domain/
COPY src/sales-service/Sales.Application/Sales.Application.csproj src/sales-service/Sales.Application/
COPY src/sales-service/Sales.Infrastructure/Sales.Infrastructure.csproj src/sales-service/Sales.Infrastructure/
RUN dotnet restore src/sales-service/Sales.Infrastructure/Sales.Infrastructure.csproj

COPY src/sales-service/Sales.Domain/         src/sales-service/Sales.Domain/
COPY src/sales-service/Sales.Application/    src/sales-service/Sales.Application/
COPY src/sales-service/Sales.Infrastructure/ src/sales-service/Sales.Infrastructure/

# Emit the idempotent SQL script. No DB connection is needed at this stage.
RUN dotnet tool run dotnet-ef migrations script --idempotent \
    --project src/sales-service/Sales.Infrastructure/Sales.Infrastructure.csproj \
    --output /migrate.sql

FROM postgres:${POSTGRES_VERSION} AS migrator
COPY --from=build /migrate.sql /migrate.sql
ENTRYPOINT ["sh", "-c", "psql \"$SALES_DB_CONNECTION\" -v ON_ERROR_STOP=1 -f /migrate.sql"]
