# TDD Test Plan — Ballast Lane Application

> Consolidated from US-001 through US-008. Each section is ordered **Red → Green → Refactor**: write the failing test first, implement the minimum code to pass, then refine.
>
> **Layer legend**: 🔴 Unit (xUnit / Vitest) · 🟠 Integration (Testcontainers) · 🟢 E2E (Cypress)

---

## Phase 1 — Domain & Infrastructure Foundation

These tests have zero UI dependency. Start here to build the core from the inside out.

### 1.1 Password Hashing (US-001)

| # | Test | Layer | Framework |
|---|---|---|---|
| 1 | `HashPassword_ReturnsNonNullBcryptHash` — given a plain-text password, returns a bcrypt/Argon2 hash that is not equal to the input | 🔴 Unit | xUnit |
| 2 | `VerifyPassword_CorrectPassword_ReturnsTrue` — hash produced by `HashPassword` verifies successfully | 🔴 Unit | xUnit |
| 3 | `VerifyPassword_WrongPassword_ReturnsFalse` — mismatched password returns false | 🔴 Unit | xUnit |
| 4 | `HashPassword_NullInput_ThrowsArgumentException` | 🔴 Unit | xUnit |

### 1.2 User Repository — `auth` Schema (US-001, US-002)

| # | Test | Layer | Framework |
|---|---|---|---|
| 5 | `CreateUser_ValidData_InsertsIntoAuthSchema` — row exists in `auth.users` after insert | 🟠 Integration | xUnit + Testcontainers (Postgres) |
| 6 | `CreateUser_DuplicateEmail_ThrowsUniqueConstraintException` | 🟠 Integration | xUnit + Testcontainers |
| 7 | `GetUserByEmail_ExistingUser_ReturnsUserEntity` | 🟠 Integration | xUnit + Testcontainers |
| 8 | `GetUserByEmail_NonExistent_ReturnsNull` | 🟠 Integration | xUnit + Testcontainers |
| 9 | `CreateUser_UseParameterizedQuery_NoSqlInjection` — input with SQL metacharacters stored and retrieved safely | 🟠 Integration | xUnit + Testcontainers |

### 1.3 Order Repository — `domain` Schema (US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 10 | `CreateOrder_ValidOrder_PersistsAndReturnsId` | 🟠 Integration | xUnit + Testcontainers |
| 11 | `GetOrders_Paginated_ReturnsCorrectPageAndCount` — page 2 of 5-per-page returns expected slice | 🟠 Integration | xUnit + Testcontainers |
| 12 | `GetOrders_EmptyTable_ReturnsEmptyList` | 🟠 Integration | xUnit + Testcontainers |
| 13 | `UpdateOrder_ExistingOrder_PersistsChanges` | 🟠 Integration | xUnit + Testcontainers |
| 14 | `UpdateOrder_NonExistent_ThrowsNotFoundException` | 🟠 Integration | xUnit + Testcontainers |
| 15 | `DeleteOrder_SoftDeletes_SetsIsDeletedFlag` — row still exists with `is_deleted = true` | 🟠 Integration | xUnit + Testcontainers |
| 16 | `GetOrders_ExcludesSoftDeleted` — soft-deleted orders not returned in default query | 🟠 Integration | xUnit + Testcontainers |
| 17 | `CreateOrder_SqlInjectionAttempt_ParameterizedSafely` | 🟠 Integration | xUnit + Testcontainers |

---

## Phase 2 — Application Layer (Use Cases)

Pure business logic. Mock repository interfaces; no database, no HTTP.

### 2.1 Registration Use Case (US-001)

| # | Test | Layer | Framework |
|---|---|---|---|
| 18 | `RegisterUser_ValidInput_CallsRepositoryAndHashesPassword` — verify `CreateUser` called with hashed (not plain) password | 🔴 Unit | xUnit + Moq/NSubstitute |
| 19 | `RegisterUser_DuplicateEmail_ReturnsConflictResult` — repo throws unique constraint → use case returns meaningful error | 🔴 Unit | xUnit |
| 20 | `RegisterUser_WeakPassword_ReturnsValidationError` — password policy enforced before repo call | 🔴 Unit | xUnit |
| 21 | `RegisterUser_InvalidEmail_ReturnsValidationError` | 🔴 Unit | xUnit |

### 2.2 Login Use Case (US-002)

| # | Test | Layer | Framework |
|---|---|---|---|
| 22 | `Login_ValidCredentials_ReturnsJwtAndSetsCookie` — returns token payload + cookie descriptor | 🔴 Unit | xUnit |
| 23 | `Login_InvalidPassword_ReturnsUnauthorized` — does **not** reveal whether email exists (no user enumeration) | 🔴 Unit | xUnit |
| 24 | `Login_NonExistentEmail_ReturnsUnauthorized` — same generic error as wrong password | 🔴 Unit | xUnit |
| 25 | `Login_NullInputs_ReturnsValidationError` | 🔴 Unit | xUnit |

### 2.3 JWT Provider (US-002, US-004)

| # | Test | Layer | Framework |
|---|---|---|---|
| 26 | `GenerateToken_ContainsExpectedClaims` — sub, email, roles, exp present | 🔴 Unit | xUnit |
| 27 | `GenerateToken_ExpiresWithinConfiguredTTL` | 🔴 Unit | xUnit |
| 28 | `ValidateToken_ValidJwt_ReturnsClaimsPrincipal` | 🔴 Unit | xUnit |
| 29 | `ValidateToken_ExpiredJwt_ThrowsSecurityTokenExpiredException` | 🔴 Unit | xUnit |
| 30 | `ValidateToken_TamperedJwt_ThrowsSecurityTokenInvalidSignatureException` | 🔴 Unit | xUnit |

### 2.4 Order Use Cases (US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 31 | `CreateOrder_ValidInput_DelegatesToRepository` | 🔴 Unit | xUnit |
| 32 | `CreateOrder_MissingRequiredFields_ReturnsValidationError` | 🔴 Unit | xUnit |
| 33 | `GetOrders_DelegatesToRepositoryWithPaginationParams` | 🔴 Unit | xUnit |
| 34 | `UpdateOrder_NonExistent_ReturnsNotFound` | 🔴 Unit | xUnit |
| 35 | `DeleteOrder_NonExistent_ReturnsNotFound` | 🔴 Unit | xUnit |
| 36 | `DeleteOrder_ExistingOrder_CallsSoftDelete` | 🔴 Unit | xUnit |

### 2.5 Logout Use Case (US-004)

| # | Test | Layer | Framework |
|---|---|---|---|
| 37 | `Logout_InvalidatesRefreshToken` — refresh token marked revoked in store | 🔴 Unit | xUnit |
| 38 | `Logout_ClearsSessionCookie` — cookie descriptor returned with expired date | 🔴 Unit | xUnit |

---

## Phase 3 — Web Layer (Controllers & Middleware)

### 3.1 Auth Controller — MVC (US-001, US-002, US-004)

| # | Test | Layer | Framework |
|---|---|---|---|
| 39 | `POST /auth/register` — valid payload → 201 + redirect to login | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 40 | `POST /auth/register` — duplicate email → 409 Conflict | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 41 | `POST /auth/login` — valid credentials → 200 + Set-Cookie + JWT in body | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 42 | `POST /auth/login` — invalid credentials → 401 + generic message | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 43 | `POST /auth/logout` — clears cookie + returns 200 | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 44 | `POST /auth/logout` — subsequent API call with old JWT → 401 | 🟠 Integration | xUnit + `WebApplicationFactory` |

### 3.2 Orders API Controller (US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 45 | `POST /api/orders` — valid JWT + valid body → 201 Created | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 46 | `POST /api/orders` — no JWT → 401 Unauthorized | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 47 | `GET /api/orders?page=1&size=10` — returns paginated JSON | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 48 | `PUT /api/orders/{id}` — valid update → 200 OK | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 49 | `PUT /api/orders/{id}` — non-existent → 404 | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 50 | `DELETE /api/orders/{id}` — existing → 204 No Content (soft-delete) | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 51 | `DELETE /api/orders/{id}` — non-existent → 404 | 🟠 Integration | xUnit + `WebApplicationFactory` |

### 3.3 Middleware — Observability (US-001–US-004)

| # | Test | Layer | Framework |
|---|---|---|---|
| 52 | `RequestLoggingMiddleware_AttachesCorrelationId` — response includes `X-Correlation-Id` header | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 53 | `RequestLoggingMiddleware_LogsRequestAndResponse` — Serilog test sink captures structured log with path, status code | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 54 | `ExceptionMiddleware_UnhandledException_Returns500WithStandardBody` — no stack trace leaked | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 55 | `ExceptionMiddleware_LogsExceptionWithTraceId` — Serilog test sink contains TraceId and SpanId | 🟠 Integration | xUnit + `WebApplicationFactory` |

### 3.4 Auth Scheme Enforcement (US-002, US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 56 | `GET /dashboard` — no cookie → redirect to /auth/login (302)` | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 57 | `GET /dashboard` — valid cookie → 200 (serves React host view)` | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 58 | `GET /api/orders` — cookie only (no Bearer) → 401` | 🟠 Integration | xUnit + `WebApplicationFactory` |
| 59 | `GET /api/orders` — valid Bearer → 200` | 🟠 Integration | xUnit + `WebApplicationFactory` |

---

## Phase 4 — Frontend Unit Tests (React)

All tests use **Vitest** + **React Testing Library**. Services mocked via `vi.mock()`.

### 4.1 Service Layer (US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 60 | `orderService.getOrders` — calls `GET /api/orders` with auth header and returns parsed JSON | 🔴 Unit | Vitest |
| 61 | `orderService.createOrder` — calls `POST /api/orders` with body | 🔴 Unit | Vitest |
| 62 | `orderService.updateOrder` — calls `PUT /api/orders/{id}` | 🔴 Unit | Vitest |
| 63 | `orderService.deleteOrder` — calls `DELETE /api/orders/{id}` | 🔴 Unit | Vitest |
| 64 | `authService.login` — stores JWT in returned object (not localStorage)` | 🔴 Unit | Vitest |
| 65 | `authService.logout` — calls POST /auth/logout and clears token from state` | 🔴 Unit | Vitest |

### 4.2 Custom Hooks (US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 66 | `useOrders` — sets `loading=true` then populates `data` on resolve | 🔴 Unit | Vitest + RTL `renderHook` |
| 67 | `useOrders` — sets `error` on API failure | 🔴 Unit | Vitest + RTL `renderHook` |
| 68 | `useCreateOrder` — calls service and triggers refetch on success | 🔴 Unit | Vitest + RTL `renderHook` |

### 4.3 API Error Interceptor (US-003, US-004, US-016)

| # | Test | Layer | Framework |
|---|---|---|---|
| 69 | `interceptor` — 401 response triggers redirect to `/auth/login` | 🔴 Unit | Vitest |
| 70 | `interceptor` — 403 response triggers notification with permission message | 🔴 Unit | Vitest |
| 71 | `interceptor` — 500 response triggers generic retry notification | 🔴 Unit | Vitest |

### 4.4 Components (US-001, US-002, US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 72 | `LoginForm` — renders email and password fields with associated labels (a11y) | 🔴 Unit | Vitest + RTL |
| 73 | `LoginForm` — submit calls `authService.login` with form values | 🔴 Unit | Vitest + RTL |
| 74 | `LoginForm` — displays error message on failed login | 🔴 Unit | Vitest + RTL |
| 75 | `LoginForm` — all interactive elements reachable via keyboard (Tab) | 🔴 Unit | Vitest + RTL |
| 76 | `OrderList` — renders loading skeleton while fetching | 🔴 Unit | Vitest + RTL |
| 77 | `OrderList` — renders order rows with data from `useOrders` | 🔴 Unit | Vitest + RTL |
| 78 | `OrderList` — renders empty state when no orders exist | 🔴 Unit | Vitest + RTL |
| 79 | `OrderList` — delete button shows confirmation `<dialog>` with focus trap | 🔴 Unit | Vitest + RTL |
| 80 | `OrderList` — pagination controls update URL search params | 🔴 Unit | Vitest + RTL + MemoryRouter |
| 81 | `OrderList` — table headers have correct `scope` and `aria-sort` attributes | 🔴 Unit | Vitest + RTL |
| 82 | `ErrorBoundary` — catches child render error and displays fallback UI with retry button | 🔴 Unit | Vitest + RTL |
| 83 | `Toast/Notification` — success toast auto-dismisses; error toast persists | 🔴 Unit | Vitest + RTL |

### 4.5 Accessibility — Automated (US-001–US-003)

| # | Test | Layer | Framework |
|---|---|---|---|
| 84 | `LoginForm` — passes `axe-core` with zero violations | 🔴 Unit | Vitest + `@axe-core/react` |
| 85 | `RegisterForm` — passes `axe-core` with zero violations | 🔴 Unit | Vitest + `@axe-core/react` |
| 86 | `OrderList` — passes `axe-core` with zero violations | 🔴 Unit | Vitest + `@axe-core/react` |

---

## Phase 5 — End-to-End (Cypress)

Run on-demand with pre-seeded database fixtures. These validate the full user journey across Razor SSR and React SPA.

### 5.1 Registration Journey (US-001)

| # | Test | Framework |
|---|---|---|
| 87 | Visit `/auth/register` → fill form → submit → see success message → redirected to `/auth/login` | Cypress |
| 88 | Register with existing email → see inline error, no redirect | Cypress |
| 89 | Register with weak password → see inline validation, form not submitted | Cypress |
| 90 | Registration page — `cy.checkA11y()` passes (cypress-axe) | Cypress |

### 5.2 Login Journey (US-002)

| # | Test | Framework |
|---|---|---|
| 91 | Visit `/auth/login` → enter valid credentials → redirected to `/dashboard` | Cypress |
| 92 | Login with wrong password → see generic error, stay on login page | Cypress |
| 93 | Login page — `cy.checkA11y()` passes | Cypress |

### 5.3 Order CRUD Journey (US-003)

| # | Test | Framework |
|---|---|---|
| 94 | Login → navigate to orders → see paginated list → verify pagination URL params update | Cypress |
| 95 | Create new order → see toast confirmation → order appears in list | Cypress |
| 96 | Edit existing order → see updated values persisted | Cypress |
| 97 | Delete order → see confirmation dialog → confirm → toast with undo → order removed from list | Cypress |
| 98 | Orders page — `cy.checkA11y()` passes | Cypress |

### 5.4 Logout Journey (US-004)

| # | Test | Framework |
|---|---|---|
| 99 | Click logout → redirected to `/auth/login` → visit `/dashboard` → redirected back (cookie cleared) | Cypress |
| 100 | After logout, API call with stale JWT → 401 | Cypress |

### 5.5 SEO Verification (US-005)

| # | Test | Framework |
|---|---|---|
| 101 | `/auth/login` — `<title>`, `<meta description>`, `og:title`, `og:image` present in page source | Cypress |
| 102 | `/dashboard` — `<meta name="robots" content="noindex, nofollow">` present | Cypress |
| 103 | `/robots.txt` — contains `Disallow: /dashboard` | Cypress |
| 104 | `/sitemap.xml` — contains public routes, excludes `/dashboard` | Cypress |

---

## Phase 6 — Infrastructure & Observability (Non-TDD Verification)

These are validated during CI or Docker Compose startup, not in the traditional Red-Green-Refactor loop, but must be verified before merge.

### 6.1 Docker Compose (US-006)

| # | Verification | Method |
|---|---|---|
| 105 | `docker-compose up` starts backend, frontend, Postgres, Seq, Jaeger without errors | CI smoke test |
| 106 | Seq reachable at `http://localhost:5341` and receiving logs | Manual / health check |
| 107 | Jaeger reachable at `http://localhost:16686` and displaying traces | Manual / health check |
| 108 | PostgreSQL accepts connections on `5432`; `auth` and `domain` schemas exist | CI init script |

### 6.2 CI Pipeline (US-007)

| # | Verification | Method |
|---|---|---|
| 109 | PR triggers: build, lint, unit tests, integration tests, bundle size check | GitHub Actions workflow |
| 110 | Unit test coverage gate ≥ 90% or pipeline fails | `dotnet test` + `vitest --coverage` |
| 111 | Integration test coverage gate = 100% API endpoints | `dotnet test` with endpoint assertions |
| 112 | `eslint-plugin-jsx-a11y` violations fail the lint step | `npm run lint` in CI |

### 6.3 Production Observability (US-008)

| # | Verification | Method |
|---|---|---|
| 113 | Terraform `plan` succeeds for CloudWatch Logs, X-Ray, SNS, Dashboards | `terraform plan` in CI |
| 114 | `appsettings.Production.json` configures `Serilog.Sinks.AwsCloudWatch` | Code review / unit test on config binding |
| 115 | Application starts with `ASPNETCORE_ENVIRONMENT=Production` without sink errors | Integration smoke test |

---

## Execution Order Summary

```
Phase 1  Domain & Infrastructure     Tests  1–17    (🔴 + 🟠)  ← Start here
Phase 2  Application Use Cases       Tests 18–38    (🔴)
Phase 3  Web Controllers & Middleware Tests 39–59    (🟠)
Phase 4  Frontend React              Tests 60–86    (🔴)
Phase 5  End-to-End Cypress          Tests 87–104   (🟢)
Phase 6  Infra & Observability       Checks 105–115 (CI/Manual)
```

Total: **115 tests / verification checks** covering all 8 user stories, 28 NFRs, and 17 ADRs.
