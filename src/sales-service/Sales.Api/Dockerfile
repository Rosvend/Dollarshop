# Sales.Api Dockerfile — multi-stage.
# Build context = repo root, so we can copy the three referenced projects too.
#   docker build -f src/sales-service/Sales.Api/Dockerfile .

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy only the csproj files first so `dotnet restore` is cached across edits.
COPY src/sales-service/Sales.Domain/Sales.Domain.csproj           src/sales-service/Sales.Domain/
COPY src/sales-service/Sales.Application/Sales.Application.csproj src/sales-service/Sales.Application/
COPY src/sales-service/Sales.Infrastructure/Sales.Infrastructure.csproj src/sales-service/Sales.Infrastructure/
COPY src/sales-service/Sales.Api/Sales.Api.csproj                 src/sales-service/Sales.Api/
RUN dotnet restore src/sales-service/Sales.Api/Sales.Api.csproj

# Now copy the sources and publish.
COPY src/sales-service/ src/sales-service/
RUN dotnet publish src/sales-service/Sales.Api/Sales.Api.csproj \
    -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Sales.Api.dll"]
