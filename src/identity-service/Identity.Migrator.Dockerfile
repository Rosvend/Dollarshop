# Identity DB migrator — applies EF migrations to PostgreSQL (idempotent script).
# Build context = repo root.

ARG DOTNET_VERSION=10.0
ARG POSTGRES_VERSION=17

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY dotnet-tools.json ./
RUN dotnet tool restore

COPY src/identity-service/Identity.Domain/Identity.Domain.csproj           src/identity-service/Identity.Domain/
COPY src/identity-service/Identity.Application/Identity.Application.csproj src/identity-service/Identity.Application/
COPY src/identity-service/Identity.Infrastructure/Identity.Infrastructure.csproj src/identity-service/Identity.Infrastructure/
RUN dotnet restore src/identity-service/Identity.Infrastructure/Identity.Infrastructure.csproj

COPY src/identity-service/Identity.Domain/         src/identity-service/Identity.Domain/
COPY src/identity-service/Identity.Application/    src/identity-service/Identity.Application/
COPY src/identity-service/Identity.Infrastructure/ src/identity-service/Identity.Infrastructure/

RUN dotnet tool run dotnet-ef migrations script --idempotent \
    --project src/identity-service/Identity.Infrastructure/Identity.Infrastructure.csproj \
    --output /migrate.sql

FROM postgres:${POSTGRES_VERSION} AS migrator
COPY --from=build /migrate.sql /migrate.sql
ENTRYPOINT ["sh", "-c", "psql \"$IDENTITY_DB_CONNECTION\" -v ON_ERROR_STOP=1 -f /migrate.sql"]
