# User Stories - Ballast Lane Application

## US-001: User Registration
**As a** new user  
**I want to** create an account with email and password  
**So that** I can access the application securely

**Acceptance Criteria:**
- Registration form is a server-rendered Razor view (ADR 001) with a server-side POST for security
- Form validates email format and password strength (client-side + server-side)
- User record persisted in the `auth.users` table within the PostgreSQL `auth` schema (ADR 004) via ADO.NET parameterized queries (ADR 005)
- Passwords hashed with a secure algorithm (bcrypt/Argon2); plain-text passwords never stored or logged
- Registration page includes SEO meta tags (`<title>`, `<meta description>`, Open Graph) via Razor `@RenderSection("Meta")` (ADR 013)
- Form meets WCAG 2.1 AA: labels associated with inputs, error messages announced to screen readers, keyboard-navigable (ADR 015)
- All user-facing strings sourced from `locales/en.json` (ADR 015)
- On success, display confirmation feedback and redirect to login
- On failure, display inline validation errors without page reload
- Registration request logged via Serilog middleware with correlation ID; no PII in logs (ADR 010)

---

## US-002: User Login
**As a** registered user  
**I want to** authenticate with my credentials  
**So that** I can access my order management dashboard

**Acceptance Criteria:**
- Login form is a server-rendered Razor view with server-side POST (ADR 001)
- On successful authentication, server issues both a session cookie (for MVC navigation) and a JWT (returned in response body) per dual-auth scheme (ADR 002)
- JWT stored in React state (memory only — never localStorage) and refreshed via a secure HttpOnly refresh cookie (ADR 002, ADR 017)
- Cookie protects the MVC `HomeController` that serves the React dashboard host view
- JWT used as `Authorization: Bearer` header for all `/api/*` requests from the React SPA
- Login page includes SEO meta tags and `<link rel="canonical">` (ADR 013)
- Form meets WCAG 2.1 AA: visible focus indicators, error messages announced to assistive tech (ADR 015)
- Invalid credentials return a generic error message (no user enumeration)
- Login attempt logged via Serilog middleware; failed attempts include IP for audit without leaking credentials (ADR 010)
- On success, redirect to `/dashboard` (React SPA entry point)

---

## US-003: Order Management (CRUD)
**As a** logged-in user  
**I want to** create, read, update, and delete orders  
**So that** I can manage my business operations

**Acceptance Criteria:**

### Create
- Form submission stores order in PostgreSQL `domain` schema via ADO.NET parameterized queries (ADR 005)
- Input validated client-side (React) and server-side (Application layer)
- Success/error feedback via global toast notification system (ADR 016)

### Read
- Orders displayed in a paginated list; pagination state carried via URL search params for deep-linkable pages (ADR 017)
- Order list route lazy-loaded via `React.lazy()` + `Suspense` (ADR 014)
- Empty state and loading skeleton rendered during data fetch

### Update
- Edit order details and persist changes via API; optimistic UI optional
- Concurrent edit conflicts handled with server-side validation

### Delete
- Soft-delete with confirmation dialog; dialog uses `<dialog>` element with focus trap (ADR 015)
- Toast notification confirms deletion with undo option (time-limited)

### Cross-Cutting (all CRUD operations)
- All API calls routed through the service layer (`/core/services`); no direct `fetch` in components (ADR 003)
- React custom hooks (`useOrders`, `useCreateOrder`, etc.) manage loading/error state between services and UI (ADR 003)
- API errors intercepted centrally: 401 → redirect to login, 403 → permission toast, 5xx → retry message (ADR 016)
- React Error Boundaries wrap the order module; widget crash does not take down the full dashboard (ADR 016)
- Client-side errors forwarded to backend Serilog pipeline with route and stack trace for trace correlation (ADR 011, ADR 016)
- All interactive elements keyboard-accessible; data tables use proper `<th>` scope and `aria-sort` attributes (ADR 015)
- All user-facing strings from `locales/en.json`; dates/numbers formatted via `Intl` API (ADR 015)
- Application state managed via React Context; no client-side storage abstraction (ADR 017)

---

## US-004: User Logout and Session Termination
**As a** logged-in user  
**I want to** log out of the application  
**So that** my session is securely terminated

**Acceptance Criteria:**
- Logout action clears the session cookie (server-side) and the JWT from React state (client-side) (ADR 002)
- HttpOnly refresh cookie invalidated on the server
- User redirected to the login page (server-rendered Razor view)
- Logout request logged via Serilog middleware (ADR 010)
- Subsequent API calls with the expired JWT return 401 and trigger the interceptor redirect (ADR 016)

---

## US-005: SEO and Public Page Optimization
**As a** product owner  
**I want** public-facing pages to be fully indexable by search engines and render rich previews on social platforms  
**So that** the application has discoverability and professional link sharing

**Acceptance Criteria:**
- Login, Register, and any landing/marketing pages rendered server-side via Razor with full HTML (ADR 001, ADR 013)
- Each public page defines `<title>`, `<meta name="description">`, Open Graph (`og:title`, `og:description`, `og:image`), and Twitter Card tags via Razor `@RenderSection("Meta")` (ADR 013)
- Structured Data (JSON-LD) embedded for Organization and WebApplication schemas on key pages (ADR 013)
- `<link rel="canonical">` set on all public pages to prevent duplicate-content penalties (ADR 013)
- Authenticated dashboard pages include `<meta name="robots" content="noindex, nofollow">` (ADR 013)
- `robots.txt` served from `wwwroot` with `Disallow: /dashboard` (ADR 013)
- `sitemap.xml` maintained for all public routes and submitted to Google Search Console (ADR 013)
- Fallback meta tags (site name, default image, charset, viewport) defined in `_Layout.cshtml` (ADR 013)

---

## US-006: Local Development Environment Setup
**As a** developer  
**I want to** spin up the entire application stack locally with a single command  
**So that** I can develop and debug without external dependencies or cloud costs

**Acceptance Criteria:**
- `docker-compose up` starts all services: .NET backend, Vite dev server, PostgreSQL, Seq, and Jaeger (ADR 006, ADR 012)
- Backend hot-reloads on code changes (watch mode); frontend hot-reloads via Vite HMR
- PostgreSQL data persisted via Docker volume across restarts
- Seq accessible at `http://localhost:5341` for structured log viewing (ADR 012)
- Jaeger accessible at `http://localhost:16686` for distributed trace viewing (ADR 012)
- Environment variables and connection strings configured via `docker-compose.yml` and `.env` file; no hardcoded secrets
- Serilog sinks auto-configured for `Development` environment via `appsettings.Development.json` (ADR 012)
- README documents one-command setup instructions for new team member onboarding

---

## US-007: CI/CD Pipeline
**As a** development team  
**I want** automated build, test, and deployment workflows  
**So that** code quality is enforced and releases are repeatable

**Acceptance Criteria:**
- All pipelines implemented via GitHub Actions (NFR-02, ADR 008)
- On pull request: build, lint, unit tests (90% coverage gate — NFR-05), integration tests (100% API coverage — NFR-06), bundle size check
- On merge to main: build Docker images, run integration tests, deploy to staging
- On release tag: deploy to production via Terraform (ADR 009)
- Frontend linting includes `eslint-plugin-jsx-a11y` for accessibility checks (ADR 015)
- `rollup-plugin-visualizer` report generated for bundle regression detection (ADR 014)
- E2E tests (Cypress) run on-demand with pre-seeded database fixtures (NFR-07, ADR 007)
- Pipeline logs captured as structured output; deployment events logged for audit

---

## US-008: Production Deployment and Observability
**As an** operations team  
**I want** the application deployed to AWS with full observability  
**So that** we can monitor health, diagnose issues, and respond to incidents

**Acceptance Criteria:**
- Infrastructure provisioned via Terraform with modular configurations (ADR 009); all resources tracked in version control (NFR-04)
- Application deployed as containerized service on AWS (ECS/Fargate or equivalent)
- Secrets managed via AWS Secrets Manager (NFR-08)
- Structured logs shipped to AWS CloudWatch Logs via `Serilog.Sinks.AwsCloudWatch` (ADR 012)
- Distributed traces exported to AWS X-Ray via ADOT Collector sidecar (ADR 011, ADR 012)
- CloudWatch Metrics and Alarms configured for health monitoring; SNS notifications for critical alerts (ADR 012)
- CloudWatch Dashboards provisioned via Terraform `modules/observability` (ADR 012)
- Serilog sink selection driven by `ASPNETCORE_ENVIRONMENT=Production` in `appsettings.Production.json` (ADR 012)
- Graceful shutdown supported via application-level cancellation tokens