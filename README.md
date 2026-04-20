# Ballast Lane Application

A full-stack order management platform built with **.NET 10** (Clean Architecture) and **React** (Vite), using a hybrid MVC-SPA rendering model, dual authentication, and a per-environment observability stack.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Environment Configuration](#environment-configuration)
- [Testing](#testing)
- [Observability](#observability)
- [Deployment](#deployment)
- [Documentation](#documentation)
- [Contributing](#contributing)

## Architecture Overview

The application follows **Clean Architecture** with four layers and a **Hybrid MVC-React micro-frontend** pattern:

```
┌─────────────────────────────────────────────────────┐
│  Web (Presentation)                                 │
│  ┌──────────────┐  ┌────────────────────────────┐   │
│  │ MVC / Razor  │  │ React SPA (Vite MPA)       │   │
│  │ Auth pages   │  │ Dashboard (orders, CRUD)    │   │
│  └──────┬───────┘  └─────────────┬──────────────┘   │
│         │  Cookie auth           │  JWT Bearer auth  │
├─────────┴────────────────────────┴──────────────────┤
│  Application (Use Cases / Commands / Queries)       │
├─────────────────────────────────────────────────────┤
│  Domain (Entities, Interfaces, Business Rules)      │
├─────────────────────────────────────────────────────┤
│  Infrastructure (ADO.NET / Npgsql, Identity, Auth)  │
└─────────────────────────────────────────────────────┘
```

- **Public pages** (Login, Register) are server-rendered via Razor for SEO and security.
- **Dashboard** is a React SPA served from an MVC host view, consuming REST APIs with JWT.
- **Dual authentication**: Cookies protect MVC navigation; JWT Bearer protects `/api/*` endpoints.
- **Database**: PostgreSQL with schema separation — `auth` schema for identity, `domain` schema for business data.
- **Data access**: ADO.NET with parameterized queries (no ORM).

See [ADR.md](ADR.md) for the full decision log (ADR 001–017).

## Key Features

| Feature | Description |
|---|---|
| **User Registration & Login** | Server-rendered forms with bcrypt password hashing, dual-auth token issuance |
| **Order Management (CRUD)** | Create, read (paginated), update, soft-delete orders via REST API |
| **SEO** | Per-page meta tags, Open Graph, JSON-LD structured data, `robots.txt`, `sitemap.xml` |
| **Accessibility** | WCAG 2.1 AA — semantic HTML, keyboard nav, 4.5:1 contrast, `axe-core` checks |
| **Observability** | Serilog structured logging, OpenTelemetry distributed tracing, per-environment sinks |
| **Performance** | Route-level code splitting, content-hashed assets, Core Web Vitals monitoring |
| **i18n Ready** | Strings in `locales/en.json`, `Intl` API formatting, logical CSS properties |

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET MVC + Web API, Clean Architecture |
| Frontend | React 18+, Vite (MPA), Tailwind CSS, React Router |
| Database | PostgreSQL (Npgsql / ADO.NET) |
| Auth | Cookies + JWT Bearer, bcrypt, HttpOnly refresh cookies |
| Logging | Serilog, Serilog.Enrichers.Span, OpenTelemetry |
| Testing | xUnit, Vitest, Cypress, Storybook, Testcontainers |
| Infra | Docker Compose, Terraform, AWS (ECS, RDS, CloudWatch, X-Ray) |
| CI/CD | GitHub Actions |

## Project Structure
All the folders fallows the assembly naming convention
```
.
├── /apps
│   ├── /web-client              # React (Vite) + Tailwind CSS (Source below)
│   └── /api-server              # .NET 10 Web API Entry Point
├── /src
│   ├── /BLA.Ordering.Web                     # Presentation Layer (MVC + Web API)
│   │   ├── /ClientApp           # React Micro-Frontend Source
│   │   │   ├── /src
│   │   │   │   ├── /apps        # Entry Points
│   │   │   │   │   ├── /auth
│   │   │   │   │   │   └── main.tsx
│   │   │   │   │   └── /dashboard
│   │   │   │   │       └── main.tsx
│   │   │   │   ├── /core        # Global Singleton Logic
│   │   │   │   │   ├── /api
│   │   │   │   │   │   └── apiClient.ts
│   │   │   │   │   ├── /context
│   │   │   │   │   │   └── AuthContext.tsx
│   │   │   │   │   └── /services
│   │   │   │   │       ├── authService.ts
│   │   │   │   │       └── orderService.ts
│   │   │   │   ├── /features    # Domain Logic
│   │   │   │   │   ├── /auth
│   │   │   │   │   │   ├── /components
│   │   │   │   │   │   │   ├── LoginForm.tsx
│   │   │   │   │   │   │   └── LoginForm.stories.tsx
│   │   │   │   │   │   └── /hooks
│   │   │   │   │   │       └── useAuthActions.ts
│   │   │   │   │   └── /orders
│   │   │   │   │       ├── /components
│   │   │   │   │       │   └── OrderList.tsx
│   │   │   │   │       │   └── OrderList.stories.tsx
│   │   │   │   │       ├── /skeletons
│   │   │   │   │       │   └── OrderSkeleton.tsx
│   │   │   │   │       └── /hooks
│   │   │   │   │           └── useOrders.ts
│   │   │   │   └── /shared      # Design System
│   │   │   │       ├── /components
│   │   │   │       │   ├── Button.tsx
│   │   │   │       │   └── Modal.tsx
│   │   │   │       │   └── Loading.tsx
│   │   │   │       │   └── InputControl.tsx
│   │   │   │       │   └── CheckControl.tsx
│   │   │   │       └── /styles
│   │   │   │           └── tailwind.css
│   │   │   ├── vite.config.ts
│   │   │   └── tailwind.config.js
│   │   ├── /Controllers         # MVC (Razor)
│   │   │   ├── AuthController.cs
│   │   │   └── HomeController.cs
│   │   ├── /Api                 # REST Endpoints
│   │   │   ├── AuthApiController.cs
│   │   │   └── OrdersController.cs
│   │   ├── /Views               # Razor Shells
│   │   │   ├── Auth/Login.cshtml
│   │   │   └── Home/Index.cshtml
│   │   ├── /wwwroot             # Build output
│   │   └── Program.cs           # Auth & DI Config
│   │
│   ├── /BLA.Ordering.Domain                  # Enterprise Rules
│   │   ├── /Entities
│   │   │   ├── Order.cs
│   │   │   └── Product.cs
│   │   └── /Interfaces
│   │       └── IOrderRepository.cs
│   │
│   ├── /BLA.Ordering.Application             # Use Cases
│   │   ├── /Orders
│   │   │   ├── CreateOrderCommand.cs
│   │   │   └── GetOrdersQuery.cs
│   │   └── /Interfaces
│   │       └── IIdentityService.cs
│   │
│   └── /BLA.Ordering.Infrastructure          # Technical implementations
│       ├── /Persistence
│       │   ├── ApplicationDbContext.cs
│       │   └── /Repositories
│       │       └── OrderRepository.cs
│       ├── /Identity
│       │   └── IdentityService.cs (Postgres + BCrypt)
│       └── /Auth
│           └── JwtProvider.cs
│
├── /tests
│   ├── /UnitTests               # xUnit / Vitest
│       └── /BLA.Ordering.Application.Tests
│       └── /BLA.Ordering.Domain.Tests
│       └── /BLA.Ordering.Infrastructure.Tests
│   ├── /IntegrationTests        # Testcontainers
│       └── /BLA.Ordering.Web.API.Tests
│   └── /cypress                 # E2E
│       └── /e2e
│           └── login.cy.ts
│
├── .storybook                   # UI Documentation Config
├── docker-compose.yml
└── README.md

```

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v24+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for IDE support)
- [Node.js 20+](https://nodejs.org/) (for frontend tooling outside Docker)
- [PostgreSQL 18+](https://www.postgresql.org/docs/) (for database documentation)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/<org>/ballast-lane-application.git
cd ballast-lane-application

# Copy environment template
cp .env.example .env

# Start all services (backend, frontend, Postgres, Seq, Jaeger)
docker-compose up
```

Once running:

| Service | URL |
|---|---|
| Application | http://localhost:5000 |
| Vite Dev Server (HMR) | http://localhost:5173 |
| Seq (logs) | http://localhost:5341 |
| Jaeger (traces) | http://localhost:16686 |
| PostgreSQL | localhost:5432 |

### Manual Setup (without Docker)

```bash
# Backend
cd src/Web
dotnet restore
dotnet run

# Frontend
cd src/Web/ClientApp
npm install
npm run dev
```

> Requires a local PostgreSQL instance. Update `appsettings.Development.json` with your connection string.

## Environment Configuration

Serilog sink selection and service endpoints are driven by `ASPNETCORE_ENVIRONMENT`:

| Environment | Log Sink | Trace Backend | Config File |
|---|---|---|---|
| `Development` | Seq (`localhost:5341`) | Jaeger (`localhost:16686`) | `appsettings.Development.json` |
| `Staging` | Console (stdout) | Disabled | `appsettings.Staging.json` |
| `Production` | AWS CloudWatch Logs | AWS X-Ray | `appsettings.Production.json` |

No code changes required to switch environments.

## Testing

```bash
# Unit tests (backend)
dotnet test tests/UnitTests

# Unit tests (frontend)
cd src/Web/ClientApp && npm run test

# Integration tests (requires Docker for Testcontainers)
dotnet test tests/IntegrationTests

# E2E tests (on-demand, with pre-seeded DB)
cd tests/cypress && npx cypress run

# Storybook (component documentation)
cd src/Web/ClientApp && npm run storybook
```

### Coverage Targets

| Layer | Target | Runner |
|---|---|---|
| UI Components, Business Logic, Data Access | 90% unit coverage | CI (every PR) |
| API Services | 100% integration coverage | CI (every PR) |
| Critical User Journeys | 100% E2E coverage | On-demand |

### Accessibility Testing

- **Lint-time**: `eslint-plugin-jsx-a11y` runs in the frontend lint pipeline
- **Dev-time**: `@axe-core/react` overlay highlights violations in the browser
- **E2E**: `cypress-axe` asserts WCAG 2.1 AA compliance in Cypress specs

## Observability

The application uses **Serilog** for structured logging and **OpenTelemetry** for distributed tracing.

Every log event is enriched with `TraceId`, `SpanId`, and `CorrelationId` via `Serilog.Enrichers.Span`, enabling end-to-end request correlation across middleware, services, and database calls.

```
Request → Serilog Middleware → Application Service (ActivitySource span)
    → ADO.NET Repository (ActivitySource span) → Response
         ↓                        ↓
    Seq / CloudWatch         Jaeger / X-Ray
```

See [ADR.md](ADR.md) — ADR 010, 011, 012 for full details.

## Deployment

### Production (AWS)

Infrastructure is provisioned via Terraform modules:

```bash
cd infra/terraform
terraform init
terraform plan -var-file=production.tfvars
terraform apply
```

Key AWS services:
- **ECS / Fargate** — containerized application hosting
- **RDS PostgreSQL** — managed database
- **CloudWatch Logs + Dashboards** — log aggregation and monitoring
- **X-Ray** — distributed tracing (via ADOT Collector sidecar)
- **Secrets Manager** — credentials and API keys
- **SNS** — alarm notifications

CI/CD is handled by **GitHub Actions** — build, test, and deploy on merge/release tag.

## Documentation

| Document | Description |
|---|---|
| [ADR.md](ADR.md) | Architectural Decision Records (ADR 001–017) |
| [Documents/Requirements/UserStories.md](Documents/Requirements/UserStories.md) | User Stories (US-001 through US-008) |
| [Documents/Requirements/NFRs.md](Documents/Requirements/NFRs.md) | 28 Non-Functional Requirements with ADR traceability |
| [plan.md](plan.md) | Project assumptions and solution scaffolding |

## Contributing

1. Create a feature branch from `main`
2. Follow the project's Clean Architecture layer boundaries
3. Ensure all tests pass and coverage targets are met
4. Frontend PRs must pass `eslint-plugin-jsx-a11y` and `axe-core` checks
5. All user-facing strings must be added to `locales/en.json`
6. Open a pull request — CI will enforce quality gates automatically
