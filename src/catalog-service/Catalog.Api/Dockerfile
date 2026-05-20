# Catalog.Api Dockerfile — multi-stage.
# Build context = repo root.
#   docker build -f src/catalog-service/Catalog.Api/Dockerfile .

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY src/catalog-service/Catalog.Domain/Catalog.Domain.csproj           src/catalog-service/Catalog.Domain/
COPY src/catalog-service/Catalog.Application/Catalog.Application.csproj src/catalog-service/Catalog.Application/
COPY src/catalog-service/Catalog.Infrastructure/Catalog.Infrastructure.csproj src/catalog-service/Catalog.Infrastructure/
COPY src/catalog-service/Catalog.Api/Catalog.Api.csproj                 src/catalog-service/Catalog.Api/
RUN dotnet restore src/catalog-service/Catalog.Api/Catalog.Api.csproj

COPY src/catalog-service/ src/catalog-service/
RUN dotnet publish src/catalog-service/Catalog.Api/Catalog.Api.csproj \
    -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
