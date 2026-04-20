Architectural Decision Record (ADR), documenting the critical structural choices made during this session. 
## Architectural Decision Record (ADR) Log

## ADR 001: Hybrid MVC-React Micro-Frontend Architecture
Date: 2024-04-19
Status: Accepted
## Context
The application requires a robust enterprise-grade backend (Clean Architecture) and a rich, interactive frontend. We need to handle initial authentication via server-side rendered pages (for security and SEO) while providing a high-performance Single Page Application (SPA) experience for the dashboard. 
## Decision
We will implement a Hybrid MVC-React architecture using Vite's Multi-Page App (MPA) feature.

* Backend: .NET 10 following Clean Architecture (Domain, Application, Infrastructure, Web).
* Frontend: ReactJS hosted within ASP.NET MVC Views.
* Micro-frontend: Separate Vite entry points for auth (Login/Register) and dashboard.

## Consequences

* Positive: Improved security for login (server-side POST); optimized bundle sizes (auth vs dashboard); unified deployment within a single .NET process.
* Negative: Increased complexity in Vite/MSBuild configuration; requires managing dual authentication schemes.

------------------------------
## ADR 002: Dual Authentication Scheme (Cookies + JWT)
Date: 2024-04-19
Status: Accepted
## Context
Standard browser-based navigation (MVC/Razor) works best with Cookies, while API consumption by a React SPA often utilizes JWTs for stateless communication and cross-origin flexibility.
## Decision
Implement a Dual-Auth Scheme in Program.cs.

* Cookies: Default scheme for UI navigation. Used by the browser to protect the HomeController that serves the React app.
* JWT (Bearer): Used by the React app for all axios/fetch requests to /api/* endpoints.
* Storage: JWTs will be stored in memory (React state) and refreshed via a secure, HttpOnly refresh cookie.

## Consequences

* Positive: Seamless transition from Login (MVC) to Dashboard (React); API remains usable by mobile apps or third-party integrators via JWT.
* Negative: Must explicitly define AuthenticationSchemes on API controllers to avoid redirect loops.

------------------------------
## ADR 003: Feature-Based Frontend Decoupling
Date: 2024-04-19
Status: Accepted
## Context
To prevent "spaghetti code" in the React app and ensure testability, the UI must be decoupled from the business logic and API communication.
## Decision
Adopt a Clean Frontend Architecture within /ClientApp/src.

* Service Layer: All API logic resides in /core/services. No fetch calls allowed in components.
* Custom Hooks: Bridge between UI and Services, managing React-specific state (loading, error).
* Testing: Use Storybook for isolated component unit testing and Cypress for full E2E journeys.

## Consequences

* Positive: High testability (services can be tested with Vitest without React); components are "pure" UI.
* Negative: Higher initial boilerplate (Services + Hooks + Components for every feature).

------------------------------
## ADR 004: Postgres 'Auth' Schema Separation
Date: 2024-04-19
Status: Accepted
## Context
Security-sensitive user data (hashes, salts) should be isolated from standard business domain tables (Orders, Products) to follow the Principle of Least Privilege and simplify database auditing.
## Decision
Store all identity-related tables in a dedicated Postgres schema named auth.

* Tables: auth.users, auth.roles, auth.permissions.
* Technology: Managed via Npgsql within the Infrastructure layer.

## Consequences

* Positive: Better security boundaries; cleaner Domain layer as it doesn't need to know about password hashes.
* Negative: Slightly more complex migrations when using Entity Framework or Dapper.

------------------------------
## ADR 005: ADO.NET to Handle DB Operations
Date: 2024-04-19
Status: Accepted
## Context
While ORMs like Entity Framework and Dapper offer abstraction benefits, direct ADO.NET provides fine-grained control over query performance, parameterization, and connection management—critical for handling complex domain operations and maintaining security boundaries across the auth schema and business domains.
## Decision
Use ADO.NET with parameterized queries for data access in the Infrastructure layer.

* Query Execution: Direct SqlCommand/NpgsqlCommand for explicit control over SQL operations.
* Security: Mandatory parameter binding to prevent SQL injection.
* Schema Separation: Explicit handling of auth and domain schema queries with clear separation of concerns.

## Consequences

* Positive: Maximum performance control; explicit security through parameterization; simpler dependency chain; clear audit trail of database operations.
* Negative: More verbose code than ORM alternatives; manual mapping of result sets; increased responsibility for query optimization.

------------------------------
## ADR 006: Docker Containerization with Compose for Local Development and Production Deployment
Date: 2024-04-19
Status: Accepted
## Context
Development environments differ across team members' machines, leading to inconsistencies ("works on my machine"). We need a standardized, reproducible environment for both local development and production deployment. Docker provides containerization for consistency, while Docker Compose orchestrates multi-container applications (backend, frontend, database).
## Decision
Adopt Docker and Docker Compose as the standardization layer.

* Backend: .NET 10 application containerized with a multi-stage Dockerfile (development and production stages).
* Frontend: Node.js development server and production-build containers via Vite.
* Database: Postgres service defined in docker-compose.yml with persistent volume for data.
* Observability: Seq (structured log viewer, port 5341/80) and Jaeger (trace viewer, port 16686) included as development services for local observability aligned with ADR 010, 011, and 012.
* Composition: Single docker-compose.yml file orchestrates all services with shared networking and environment variables.

## Consequences

* Positive: Unified development environment across all developers; seamless CI/CD pipeline integration; production parity reduces deployment surprises; easy onboarding for new team members; improved scalability and portability across cloud providers; built-in local observability via Seq and Jaeger eliminates need for external monitoring during development.
* Negative: Added Docker knowledge requirement; increased build times on first setup; potential performance overhead on non-Linux hosts (Docker Desktop); requires maintenance of Docker files and compose configurations; Seq and Jaeger containers add memory overhead (~256 MB each) to the local Docker environment.

------------------------------

## ADR 007: E2E Testing with Database Seeding (Pending Review)
Date: 2024-04-19
Status: Pending
## Context
TBD
## Decision
Run E2E tests on demand with pre-seeded database fixtures.
## Consequences
TBD

------------------------------

## ADR 008: CI/CD Pipeline with GitHub (Pending Review)
Date: 2024-04-19
Status: Pending
## Context
TBD
## Decision
Implement GitHub Actions for continuous integration and deployment workflows.
## Consequences
TBD

------------------------------

## ADR 009: AWS Infrastructure Deployment with Terraform (Pending Review)
Date: 2024-04-19
Status: Pending
## Context
TBD
## Decision
Deploy to AWS infrastructure using Terraform with modularization, variables, and reusable configurations.

* Observability Services: Provision AWS CloudWatch Logs (log aggregation), AWS X-Ray (distributed tracing), and CloudWatch Metrics/Alarms (health monitoring) as managed replacements for the local Seq and Jaeger services used in development (see ADR 006 and ADR 012).
* Terraform Modules: Observability resources (log groups, X-Ray sampling rules, CloudWatch dashboards, SNS alarm topics) encapsulated in a dedicated `modules/observability` Terraform module.

## Consequences
TBD

------------------------------

## ADR 010: Structured Logging with Serilog and Centralized Error Handling
Date: 2024-04-19
Status: Accepted
## Context
Logging and error handling are critical cross-cutting concerns that, if scattered throughout the codebase, create maintenance burden, inconsistent observability, and tight coupling between business logic and infrastructure concerns. Centralized, structured logging enables correlation tracking and debugging without polluting domain logic.
## Decision
Adopt Serilog as the structured logging framework with centralized middleware and decorator patterns for logging decoupling.

* Middleware: Request/response interceptors and exception-handling middleware capture logs globally without requiring individual controller/service instrumentation.
* Decorators: Service layer decorated with logging behavior (via proxy pattern) to add observability without modifying core logic.
* Structured Logging: Serilog enriches logs with correlation IDs, request context, and metadata for correlation across distributed traces.
* Exception Handling: Centralized middleware converts exceptions to structured error logs before returning standardized API responses.

## Consequences

* Positive: Consistent logging across the application; domain logic remains clean and testable; centralized error handling reduces code duplication; correlation IDs enable end-to-end tracing; easy integration with Application Insights or other observability platforms.
* Negative: Requires careful middleware ordering to ensure proper exception capture; decorator overhead adds slight performance cost; team must adopt structured logging conventions.

------------------------------

## ADR 011: Distributed Tracing with Serilog and OpenTelemetry
Date: 2024-04-19
Status: Accepted
## Context
Structured logs alone are insufficient for diagnosing latency bottlenecks and understanding request flow across service boundaries. Distributed tracing provides span-level visibility into how a request propagates through middleware, services, and database calls. The application already uses Serilog (ADR 010) for structured logging, and leveraging it as the trace correlation backbone avoids introducing a separate tracing SDK while maintaining a unified observability pipeline.
## Decision
Extend the Serilog pipeline with OpenTelemetry trace context to produce correlated, trace-enriched logs.

* Serilog Enrichers: Use `Serilog.Enrichers.Span` to automatically attach `TraceId`, `SpanId`, and `ParentId` from `System.Diagnostics.Activity` to every log event.
* ActivitySource Instrumentation: Register custom `ActivitySource` instances in the Application and Infrastructure layers to create spans for key operations (e.g., database queries, external HTTP calls).
* Sink Flexibility: Write trace-enriched logs to Console (development) and a structured sink such as Seq, OpenTelemetry Collector, or AWS CloudWatch (production), enabling future export to Jaeger, Zipkin, or AWS X-Ray without code changes.
* Correlation: The `TraceId` propagated via W3C `traceparent` header ties HTTP request logs, service spans, and database spans into a single trace view.

## Consequences

* Positive: End-to-end trace visibility without a heavyweight APM agent; reuses existing Serilog infrastructure; W3C trace context ensures interoperability with any OpenTelemetry-compatible backend; minimal performance overhead from `Activity`-based instrumentation.
* Negative: Trace export to a dedicated backend (e.g., Jaeger) requires additional infrastructure; `Serilog.Enrichers.Span` must be kept in sync with .NET Activity API changes; teams must instrument new services with `ActivitySource` to maintain trace continuity.

------------------------------

## ADR 012: Observability Stack — Per-Environment Monitoring Tools
Date: 2024-04-19
Status: Accepted
## Context
ADR 010 (Serilog logging), ADR 011 (distributed tracing), ADR 006 (Docker Compose), and ADR 009 (AWS/Terraform) each reference observability tooling, but no single decision captures *which* concrete tools run in each environment and how they map to the Serilog sink configuration. Without this, developers may misconfigure sinks or miss critical production telemetry.
## Decision
Define an explicit observability tool matrix per environment and wire Serilog sinks accordingly.

### Local Development (Docker Compose — ADR 006)
| Concern | Tool | Access | Serilog Sink |
|---|---|---|---|
| Structured Logs | **Seq** | http://localhost:5341 | `Serilog.Sinks.Seq` |
| Distributed Traces | **Jaeger** (all-in-one) | http://localhost:16686 | OpenTelemetry Collector sidecar or `OTLP` exporter from `ActivitySource` |
| Metrics (optional) | **Prometheus + Grafana** | http://localhost:3000 | `prometheus-net` ASP.NET middleware |

### CI / Staging
| Concern | Tool | Serilog Sink |
|---|---|---|
| Logs | Console (stdout captured by CI runner) | `Serilog.Sinks.Console` |
| Traces | Disabled or Jaeger ephemeral container | — |

### Production (AWS — ADR 009)
| Concern | Tool | Serilog Sink / Integration |
|---|---|---|
| Structured Logs | **AWS CloudWatch Logs** | `Serilog.Sinks.AwsCloudWatch` |
| Distributed Traces | **AWS X-Ray** | OpenTelemetry SDK → X-Ray exporter (via `ADOT` Collector sidecar on ECS) |
| Metrics & Alarms | **CloudWatch Metrics + SNS** | `prometheus-net` → CloudWatch EMF or custom CloudWatch metrics via SDK |
| Dashboards | **CloudWatch Dashboards** | Provisioned via Terraform `modules/observability` |

### Sink Selection Strategy
* Serilog sink selection is driven by the `ASPNETCORE_ENVIRONMENT` variable (`Development`, `Staging`, `Production`).
* `appsettings.{Environment}.json` files configure the active WriteTo sinks; no code changes required to switch environments.
* All sinks share the same enriched log schema (TraceId, SpanId, CorrelationId) defined in ADR 011.

## Consequences

* Positive: Single source of truth for observability tooling; developers get full trace + log visibility locally at zero cloud cost; production uses fully managed AWS services with no self-hosted infrastructure; sink switching is configuration-only; Terraform module keeps production observability reproducible.
* Negative: Prometheus + Grafana in Docker Compose is optional and adds further container overhead; AWS observability services incur cost at scale (CloudWatch Logs ingestion, X-Ray trace sampling); team must keep `appsettings.*.json` sink configurations aligned with this ADR.

------------------------------

## ADR 013: SEO Strategy — Server-Side Meta Tags and Dynamic Metadata
Date: 2024-04-19
Status: Accepted
## Context
The Hybrid MVC-React architecture (ADR 001) splits the application into server-rendered auth pages and a React SPA dashboard. Search engine crawlers and social media link previews rely on metadata present in the initial HTML response. The SPA portion renders content client-side, making it invisible to crawlers that do not execute JavaScript. Public-facing pages (Login, Register, landing/marketing pages) must be fully indexable, while the authenticated dashboard can be excluded from indexing.
## Decision
Implement a dual metadata strategy aligned with the hybrid rendering model.

### Server-Rendered Pages (MVC/Razor — public)
* Razor `_Layout.cshtml` includes a `@RenderSection("Meta", required: false)` block so each view can inject page-specific `<title>`, `<meta name="description">`, Open Graph (`og:*`), and Twitter Card tags.
* Canonical URLs set via `<link rel="canonical">` to prevent duplicate-content penalties from query-string variations.
* Structured Data (JSON-LD) embedded in Razor views for key pages (e.g., Organization, WebApplication schema) to enhance rich snippet eligibility.

### SPA Pages (React Dashboard — authenticated)
* `<meta name="robots" content="noindex, nofollow">` injected by the MVC host view to exclude dashboard routes from search indexes.
* `react-helmet-async` used inside the React app to manage `<title>` and `<meta>` dynamically for browser tab clarity and internal analytics, not for crawler consumption.

### Shared Defaults
* Fallback meta tags defined in `_Layout.cshtml`: site-wide `og:site_name`, default `og:image`, `charset`, and `viewport`.
* `robots.txt` served from `wwwroot` with `Disallow: /dashboard` to reinforce crawler exclusion.
* `sitemap.xml` auto-generated or maintained manually for public routes; submitted to Google Search Console.

## Consequences

* Positive: Public pages are fully crawlable with rich previews on social platforms; dashboard is explicitly excluded, reducing crawl budget waste; structured data improves search result presentation; no SSR framework (Next.js) required since public pages are already server-rendered via Razor.
* Negative: Marketing/landing page additions require Razor views (not React); team must keep Open Graph images and descriptions up to date; `react-helmet-async` adds a small dependency to the SPA bundle.

------------------------------

## ADR 014: Frontend Performance Optimization
Date: 2024-04-19
Status: Accepted
## Context
A React SPA served from a .NET backend must deliver fast initial load, smooth interactions, and minimal bandwidth consumption. Poor frontend performance directly impacts user retention, Core Web Vitals scores (LCP, FID, CLS), and overall perceived quality. The Vite-based multi-entry build (ADR 001) already enables per-page code splitting, but additional strategies are needed.
## Decision
Adopt the following performance optimizations across the frontend build and runtime.

* Code Splitting & Lazy Loading: Use `React.lazy()` + `Suspense` for route-level splitting. Heavy libraries (chart libraries, rich-text editors) loaded on demand, never in the main bundle.
* Tree Shaking: Vite's Rollup-based production build eliminates dead code. Enforce ES module imports (`import { x } from 'lib'` — never `import lib`) to maximize shake-ability.
* Asset Optimization: Images served as WebP with `<picture>` fallback; SVG icons via a sprite sheet or inline React components; fonts subset to used character ranges with `font-display: swap`.
* Caching Strategy: Vite's content-hashed filenames (`[name].[hash].js`) enable aggressive `Cache-Control: immutable` headers. The MVC host view references the Vite manifest to resolve current hashes.
* Bundle Analysis: `rollup-plugin-visualizer` integrated into the Vite config (dev only) to detect bundle regressions in CI.
* Core Web Vitals Monitoring: `web-vitals` library reports LCP, FID, and CLS to the backend analytics endpoint or console in development.

## Consequences

* Positive: Smaller initial bundles; faster TTI (Time to Interactive); improved Lighthouse scores; content-hashed assets enable CDN caching without cache-busting hacks; measurable performance via Core Web Vitals reporting.
* Negative: Lazy-loaded routes show a brief loading state on first navigation; team must avoid barrel-file re-exports that defeat tree shaking; WebP conversion adds a build step.

------------------------------

## ADR 015: Accessibility (a11y) and Internationalization Readiness
Date: 2024-04-19
Status: Accepted
## Context
Accessibility is both a legal requirement (WCAG 2.1 AA compliance) and a quality signal. The application must be usable by people relying on screen readers, keyboard navigation, and assistive technologies. Additionally, while the initial release targets English, the architecture should not preclude future internationalization (i18n) without a rewrite.
## Decision
Embed accessibility as a first-class concern and prepare the frontend for future i18n.

### Accessibility
* Semantic HTML: Use native HTML elements (`<button>`, `<nav>`, `<main>`, `<dialog>`) over `<div>` with ARIA roles wherever possible.
* ARIA Attributes: Applied only when semantic HTML is insufficient (e.g., custom dropdowns, modals). Follow the "no ARIA is better than bad ARIA" principle.
* Keyboard Navigation: All interactive elements reachable via Tab; focus traps in modals; visible focus indicators (`:focus-visible`) styled consistently.
* Color Contrast: Minimum 4.5:1 ratio for normal text, 3:1 for large text. Enforced via design tokens and verified with `axe-core`.
* Automated Testing: `eslint-plugin-jsx-a11y` in the linting pipeline; `@axe-core/react` overlay in development; Cypress `cypress-axe` plugin for E2E a11y assertions.

### Internationalization Readiness
* All user-facing strings extracted into a central `locales/en.json` file from day one, even if no second language is planned.
* Component text rendered via a lightweight hook (`useTranslation` from `react-i18next` or a minimal custom wrapper) to enable future locale swapping without touching components.
* Date, number, and currency formatting via `Intl` API — never hardcoded formats.
* RTL layout support deferred but ensured by using logical CSS properties (`margin-inline-start` instead of `margin-left`).

## Consequences

* Positive: WCAG 2.1 AA compliance from launch; reduced legal risk; better UX for all users (keyboard power users, low-vision users); i18n can be activated by adding locale files without refactoring components; `Intl` API is zero-dependency.
* Negative: Accessibility adds review overhead to every UI PR; string extraction discipline required from all developers; `react-i18next` adds ~8 KB to the bundle (gzipped); RTL-ready CSS is slightly more verbose than directional properties.

------------------------------

## ADR 016: Frontend Error Handling and User Feedback Strategy
Date: 2024-04-19
Status: Accepted
## Context
Unhandled errors in a React SPA result in blank screens and silent failures, damaging user trust. API errors, network failures, and unexpected runtime exceptions must be caught, reported, and surfaced to the user in a consistent, non-disruptive manner. The backend already provides standardized error responses (ADR 010), and the frontend must complement this with a resilient error boundary and notification system.
## Decision
Implement a layered frontend error handling strategy.

* React Error Boundaries: A top-level `<ErrorBoundary>` wraps the app to catch rendering crashes and display a fallback UI with a retry action. Feature-level boundaries isolate widget failures without taking down the entire page.
* API Error Interceptor: A centralized Axios/fetch interceptor (aligned with ADR 003 service layer) maps HTTP status codes to user-friendly messages, handles 401 (redirect to login), 403 (permission denied toast), and 5xx (generic retry message).
* Toast / Notification System: A global notification context (`useNotification` hook) provides success, warning, and error toasts. All user-facing errors flow through this single channel for consistent styling and behavior (auto-dismiss for success, persist for errors).
* Error Reporting: Unhandled exceptions and API 5xx errors forwarded to the backend logging endpoint (or Serilog sink) with client context (route, user agent, stack trace) for correlation with server-side traces (ADR 011).

## Consequences

* Positive: No blank screens on runtime errors; consistent error messaging across the app; 401/403 handling is automatic; client-side errors appear in the same observability pipeline as server-side logs; feature isolation prevents cascading failures.
* Negative: Error boundary fallback UI must be designed and maintained; toast notification fatigue possible if not tuned (debounce, deduplication); client-side error reporting adds a small network overhead per error.

------------------------------

## ADR 017: No Client-Side Storage Manager — React Context and Browser History Sufficient
Date: 2024-04-19
Status: Accepted
## Context
Many SPAs introduce a client-side storage abstraction (localStorage, sessionStorage, IndexedDB wrappers) for caching user preferences, offline data, or persisted UI state. For the scope of this project, the authenticated dashboard is fully server-driven: all data is fetched on demand from the API, JWTs are held in memory (ADR 002), and no offline capability is required. Introducing a storage manager would add complexity without a concrete use case.
## Decision
Explicitly forgo a client-side storage management layer. Use React Context and browser history navigation instead.

* React Context: Application-wide transient state (authenticated user, theme, notification queue) managed via React Context providers. State is intentionally non-persistent — a page refresh re-fetches from the API, ensuring data freshness.
* Browser History / React Router: Navigation state (filters, pagination cursors, selected tabs) carried via URL search params and React Router's location state. This enables back/forward navigation and shareable deep links without a storage layer.
* No localStorage / sessionStorage Abstraction: No wrapper library or custom storage service will be created. If a future feature requires persistent client state (e.g., draft form auto-save), this ADR should be revisited.

## Consequences

* Positive: Fewer dependencies; no stale-cache bugs; no data-sync issues between storage and server; simpler mental model for developers; avoids over-engineering for the current scope.
* Negative: Page refresh loses in-flight UI state (acceptable trade-off); if offline support or complex client caching is later required, a storage layer must be introduced retroactively.

------------------------------