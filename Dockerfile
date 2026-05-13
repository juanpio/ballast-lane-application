FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /workspace
EXPOSE 5178
CMD ["dotnet", "watch", "run", "--project", "src/BLA.Ordering.Web/BLA.Ordering.Web.csproj", "--urls", "http://0.0.0.0:5178"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BLA.Ordering.slnx ./
COPY src/BLA.Ordering.Domain/BLA.Ordering.Domain.csproj src/BLA.Ordering.Domain/
COPY src/BLA.Ordering.Application/BLA.Ordering.Application.csproj src/BLA.Ordering.Application/
COPY src/BLA.Ordering.Infrastructure/BLA.Ordering.Infrastructure.csproj src/BLA.Ordering.Infrastructure/
COPY src/BLA.Ordering.Web/BLA.Ordering.Web.csproj src/BLA.Ordering.Web/
COPY tests/UnitTests/BLA.Ordering.Domain.Tests/BLA.Ordering.Domain.Tests.csproj tests/UnitTests/BLA.Ordering.Domain.Tests/
COPY tests/UnitTests/BLA.Ordering.Application.Tests/BLA.Ordering.Application.Tests.csproj tests/UnitTests/BLA.Ordering.Application.Tests/
COPY tests/UnitTests/BLA.Ordering.Infrastructure.Tests/BLA.Ordering.Infrastructure.Tests.csproj tests/UnitTests/BLA.Ordering.Infrastructure.Tests/
COPY tests/IntegrationTests/BLA.Ordering.Web.API.Tests/BLA.Ordering.Web.API.Tests.csproj tests/IntegrationTests/BLA.Ordering.Web.API.Tests/

RUN dotnet restore BLA.Ordering.slnx

COPY src ./src
COPY tests ./tests

FROM build AS backend-unit-tests
WORKDIR /src
CMD ["/bin/sh"]

FROM build AS backend-integration-tests
WORKDIR /src
ENV TESTCONTAINERS_RYUK_DISABLED=true
ENV TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal
CMD ["/bin/sh"]

FROM node:22-alpine AS frontend-base
WORKDIR /app
COPY src/BLA.Ordering.Web/ClientApp/package*.json ./
RUN npm ci
COPY src/BLA.Ordering.Web/ClientApp .

FROM frontend-base AS frontend-development
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]

FROM frontend-base AS frontend-storybook
EXPOSE 6006
CMD ["npm", "run", "storybook", "--", "--host", "0.0.0.0", "--port", "6006"]

FROM frontend-base AS frontend-tests
CMD ["npm", "run", "test:coverage"]

FROM frontend-base AS frontend-build
RUN npm run build

FROM cypress/included:14.5.4 AS cypress-tests
WORKDIR /e2e
COPY tests/cypress/package*.json ./
RUN npm ci
COPY tests/cypress .
CMD ["npm", "run", "run:auth:ci"]

FROM build AS publish
WORKDIR /src
COPY --from=frontend-build /wwwroot/app ./src/BLA.Ordering.Web/wwwroot/app
RUN dotnet publish src/BLA.Ordering.Web/BLA.Ordering.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "BLA.Ordering.Web.dll"]
