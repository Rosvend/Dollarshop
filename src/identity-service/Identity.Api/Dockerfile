# Identity.Api Dockerfile — multi-stage.
# Build context = repo root.
#   docker build -f src/identity-service/Identity.Api/Dockerfile .

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY src/identity-service/Identity.Domain/Identity.Domain.csproj           src/identity-service/Identity.Domain/
COPY src/identity-service/Identity.Application/Identity.Application.csproj src/identity-service/Identity.Application/
COPY src/identity-service/Identity.Infrastructure/Identity.Infrastructure.csproj src/identity-service/Identity.Infrastructure/
COPY src/identity-service/Identity.Api/Identity.Api.csproj                 src/identity-service/Identity.Api/
RUN dotnet restore src/identity-service/Identity.Api/Identity.Api.csproj

COPY src/identity-service/ src/identity-service/
RUN dotnet publish src/identity-service/Identity.Api/Identity.Api.csproj \
    -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
