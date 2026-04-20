## Non-Functional Requirements / Architectural Constraints

| ID     | Category        | Requirement                                      | ADR Ref | Applies To |
|--------|-----------------|--------------------------------------------------|---------|------------|
| NFR-01 | Architecture    | Data access via ADO.NET with parameterized queries only | ADR 005 | All stories |
| NFR-02 | CI/CD           | All pipelines via GitHub Actions                 | ADR 008 | All stories |
| NFR-03 | Infrastructure  | AWS-only deployment with Terraform IaC           | ADR 009 | All stories |
| NFR-04 | Infrastructure  | All cloud resources tracked in version control   | ADR 009 | All stories |
| NFR-05 | Quality         | Unit tests cover 90% of UI components, Business Logic, and Data Access layers; run on CI | — | All stories |
| NFR-06 | Quality         | Integration tests cover 100% of API services; run on CI | — | All stories |
| NFR-07 | Quality         | E2E tests cover 100% of critical functionality; run on demand with pre-seeded DB fixtures | ADR 007 | All stories |
| NFR-08 | Security        | All secrets managed via AWS Secrets Manager      | ADR 009 | All stories |
| NFR-09 | Observability   | Logging implemented as cross-cutting concern via middleware and decorators | ADR 010 | All stories |
| NFR-10 | Observability   | Structured logging with Serilog: correlation IDs, request context, and contextual metadata | ADR 010 | All stories |
| NFR-11 | Observability   | Centralized exception handling and error logging via middleware | ADR 010 | All stories |
| NFR-12 | Observability   | Distributed tracing via Serilog enrichers (`TraceId`, `SpanId`) and `ActivitySource` instrumentation | ADR 011 | All stories |
| NFR-13 | Observability   | Per-environment observability stack: Seq + Jaeger (local), CloudWatch + X-Ray (production) | ADR 012 | US-006, US-008 |
| NFR-14 | Observability   | Serilog sink selection driven by `ASPNETCORE_ENVIRONMENT` via `appsettings.{Env}.json` — no code changes | ADR 012 | All stories |
| NFR-15 | Security        | Dual-auth scheme: Cookies for MVC navigation, JWT Bearer for API; JWTs in memory only | ADR 002 | US-001, US-002, US-004 |
| NFR-16 | Security        | Auth data isolated in PostgreSQL `auth` schema; separated from business domain tables | ADR 004 | US-001, US-002 |
| NFR-17 | SEO             | Public pages include `<title>`, meta description, Open Graph, Twitter Cards, and JSON-LD structured data | ADR 013 | US-001, US-002, US-005 |
| NFR-18 | SEO             | Dashboard excluded from indexing via `noindex` meta tag, `robots.txt`, and `sitemap.xml` for public routes | ADR 013 | US-005 |
| NFR-19 | Performance     | Route-level code splitting via `React.lazy()`; content-hashed assets with immutable cache headers | ADR 014 | US-003 |
| NFR-20 | Performance     | Core Web Vitals (LCP, FID, CLS) monitored via `web-vitals` library; bundle regressions detected in CI | ADR 014 | US-003, US-007 |
| NFR-21 | Accessibility   | WCAG 2.1 AA compliance: semantic HTML, keyboard navigation, 4.5:1 contrast, `axe-core` automated checks | ADR 015 | All stories |
| NFR-22 | Accessibility   | `eslint-plugin-jsx-a11y` in lint pipeline; `cypress-axe` in E2E suite | ADR 015 | US-003, US-007 |
| NFR-23 | i18n Readiness  | All user-facing strings in `locales/en.json`; dates/numbers via `Intl` API; logical CSS properties for RTL | ADR 015 | All stories |
| NFR-24 | Resilience      | React Error Boundaries at app and feature level; centralized API error interceptor (401/403/5xx) | ADR 016 | US-003 |
| NFR-25 | Resilience      | Client-side errors forwarded to backend Serilog pipeline with route, user agent, and stack trace | ADR 016 | US-003 |
| NFR-26 | Architecture    | No client-side storage abstraction; state via React Context and URL params only | ADR 017 | US-003 |
| NFR-27 | Infrastructure  | Docker Compose provisions full stack (backend, frontend, DB, Seq, Jaeger) with single command | ADR 006 | US-006 |
| NFR-28 | Resilience      | Application-level cancellation tokens for graceful shutdown | — | All stories |