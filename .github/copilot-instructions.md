# Ballast Lane Application Copilot Instructions

Use this file as the primary implementation guide when generating code, tests, and documentation for this repository. Prefer the documented architecture and conventions here over generic framework defaults.

## Primary Sources Of Truth

- `README.md` defines the target solution structure and major technology choices.
- `Documents/ADR.md` defines architectural decisions. Treat accepted ADRs as constraints.
- `Documents/Requirements/UserStories.md` defines behavior and acceptance criteria.
- `Documents/Requirements/NFRs.md` defines cross-cutting constraints.
- `Documents/TDD-TestPlan.md` defines the intended test strategy and coverage expectations.

If generated code conflicts with an accepted ADR or NFR, follow the ADR/NFR.

## Architecture Summary

This application uses a hybrid .NET 10 + React architecture with Clean Architecture boundaries.

- Backend layers:
	- `BLA.Ordering.Domain`: entities, value objects, domain rules, repository abstractions.
	- `BLA.Ordering.Application`: use cases, commands, queries, validators, DTOs, service interfaces.
	- `BLA.Ordering.Infrastructure`: ADO.NET/Npgsql persistence, auth/JWT, external integrations.
	- `BLA.Ordering.Web`: MVC, Razor, API controllers, DI, auth wiring, middleware, static assets.
- Frontend model:
	- Razor/MVC handles public auth and SEO-sensitive pages.
	- React handles authenticated dashboard experiences.
	- React is embedded as a micro-frontend via Vite entry points.
- Authentication model:
	- Cookies protect MVC navigation.
	- JWT Bearer protects `/api/*` endpoints.
	- JWT must remain in memory only. Do not use `localStorage` or `sessionStorage` for auth tokens.
- Data access model:
	- Use ADO.NET with parameterized `NpgsqlCommand` queries only.
	- Do not introduce an ORM.
- Database model:
	- `auth` schema stores identity/authentication data.
	- `domain` schema stores business data such as orders.

## Scaffolding Rules

Generate code into the target structure below. If the folders do not exist yet, scaffold them exactly with these boundaries.

### Backend Solution Shape

Use this structure for new backend features:

```text
src/
	BLA.Ordering.Domain/
		Entities/
		Interfaces/
	BLA.Ordering.Application/
		<Feature>/
			Commands/
			Queries/
			Dtos(records)/
			Validators/
	BLA.Ordering.Infrastructure/
		Persistence/
			Repositories/
		Auth/
		Identity/
	BLA.Ordering.Web/
		Api/
		Controllers/
		Views/
		ClientApp/
```

### Frontend Shape

Use this structure for React work:

```text
src/BLA.Ordering.Web/ClientApp/src/
	apps/
		auth/
		dashboard/
	core/
		api/
		context/
		services/
	features/
		<feature>/
			components/
			hooks/
			stories/
			__tests__/
	shared/
		components/
		styles/
		utils/
```

Keep business behavior feature-local. Put cross-feature primitives in `shared` or `core` only when they are truly reusable.

## How To Scaffold A New API Path

When asked to add a new API route, generate the full vertical slice, not only the controller.

Example feature: `Orders`

1. Domain
- Add or extend entity/value object only if the capability changes domain behavior.
- Add or extend repository interfaces in `BLA.Ordering.Domain/Interfaces`.

2. Application
- Add request/response DTOs in `BLA.Ordering.Application/<Feature>/Dtos`.
- Add command or query models in `BLA.Ordering.Application/<Feature>/Commands` or `Queries`.
- Add validators in `Validators`.
- Keep orchestration in Application, not in controllers.

3. Infrastructure
- Implement repository interfaces under `BLA.Ordering.Infrastructure/Persistence/Repositories`.
- Use parameterized SQL only.
- Map records explicitly. Keep SQL close to the repository method that owns it.

4. Web/API
- Add API controllers in `BLA.Ordering.Web/Api`.
- Use explicit route attributes and explicit auth requirements.
- API controllers must stay thin: validate input, invoke Application services, shape HTTP response.

5. Tests
- Add unit tests for Application logic.
- Add integration tests for repository and controller behavior.
- Add Cypress coverage if the endpoint changes a critical user workflow.

### API Conventions

- Use resource-oriented routes such as `/api/orders` and `/api/orders/{id}`.
- Keep controllers focused on one aggregate or feature.
- Return standard status codes: `200`, `201`, `204`, `400`, `401`, `403`, `404`, `409`, `500`.
- APIs under `/api/*` must use JWT Bearer auth, not cookie-only auth.
- Validate on both client and server.
- Support cancellation tokens on async request flows.
- Log through centralized middleware and services, not ad hoc controller logging unless extra business context is needed.

### Backend File Scaffold Example

```text
src/BLA.Ordering.Application/Orders/
	Commands/
		CreateOrderCommand.cs
		UpdateOrderCommand.cs
	Queries/
		GetOrdersQuery.cs
	Dtos/
		OrderDto.cs
		CreateOrderRequest.cs
	Validators/
		CreateOrderValidator.cs

src/BLA.Ordering.Domain/Interfaces/
	IOrderRepository.cs

src/BLA.Ordering.Infrastructure/Persistence/Repositories/
	OrderRepository.cs

src/BLA.Ordering.Web/Api/
	OrdersController.cs

tests/UnitTests/BLA.Ordering.Application.Tests/Orders/
tests/IntegrationTests/BLA.Ordering.Web.API.Tests/Orders/
```

## How To Scaffold A New React Feature Or UI Component

When asked to add UI, generate the service, hook, component, story, and tests together if the feature is non-trivial.

### React Feature Workflow

1. Add API calls in `core/services`.
- Components must not call `fetch` or `axios` directly.
- Put HTTP concerns, request mapping, and response mapping in the service layer.

2. Add a feature hook in `features/<feature>/hooks`.
- Hooks own loading, error, retry, and side-effect coordination.
- Keep hooks small and focused on one use case.

3. Add UI in `features/<feature>/components`.
- Components should be presentation-focused.
- Derive data from props or hooks rather than embedding transport logic.

4. Add Storybook stories.
- Co-locate with the component or place under a `stories` folder.
- Include happy path, loading, empty, and error states when relevant.

5. Add tests.
- Use Vitest and React Testing Library for unit/component tests.
- Use Cypress for full flows and accessibility checks on critical journeys.

### React File Scaffold Example

```text
src/BLA.Ordering.Web/ClientApp/src/core/services/
	orderService.ts

src/BLA.Ordering.Web/ClientApp/src/features/orders/hooks/
	useOrders.ts
	useCreateOrder.ts

src/BLA.Ordering.Web/ClientApp/src/features/orders/components/
	OrderList.tsx
	OrderForm.tsx
	DeleteOrderDialog.tsx

src/BLA.Ordering.Web/ClientApp/src/features/orders/stories/
	OrderList.stories.tsx
	OrderForm.stories.tsx

src/BLA.Ordering.Web/ClientApp/src/features/orders/__tests__/
	OrderList.test.tsx
	useOrders.test.tsx
```

## .NET 10 / C# 14 Guidelines

Write modern C# that is explicit, testable, and easy to read.

- Target .NET 10 APIs and idioms when the project uses them.
- Prefer file-scoped namespaces.
- Prefer `required` members, primary constructors, collection expressions, and pattern matching where they improve clarity.
- Prefer immutable request/response DTOs where practical.
- Keep domain entities focused on business invariants.
- Keep infrastructure concerns out of Domain and Application.
- Use `async`/`await` end-to-end for I/O.
- Accept and pass `CancellationToken` through async boundaries.
- Validate constructor arguments and public method inputs.
- Prefer `ILogger<T>` or centralized logging abstractions already established in the project.
- Avoid static mutable state.
- Avoid god classes and controller-heavy orchestration.
- Do not place SQL in controllers.
- Do not add Entity Framework, Dapper, or any ORM abstraction unless an ADR explicitly changes the rule.

### C# Style

- Use `PascalCase` for public types and members.
- Use `camelCase` for locals and parameters.
- Use clear noun names for DTOs and entities, verb names for commands and methods.
- Keep methods short and single-purpose.
- Return specific result models when behavior is not trivially represented by primitives.
- Prefer guard clauses over deep nesting.
- Keep exceptions meaningful and do not leak secrets or PII.

### xUnit Guidelines

Use xUnit for backend unit and integration tests unless an existing test project already establishes a different pattern.

- Structure each test using the AAA pattern: Arrange, Act, Assert.
- Keep Arrange focused on inputs and dependency setup only.
- Keep Act to a single operation under test.
- Keep Assert focused on observable behavior, returned values, thrown exceptions, or dependency interactions.
- Prefer one logical assertion group per test. Split tests when multiple unrelated behaviors are being checked.
- Name tests with scenario and expected outcome, for example `CreateOrder_ValidInput_ReturnsCreatedOrder`.
- Use `[Fact]` for fixed scenarios and `[Theory]` with inline or member data for input permutations.
- Prefer test helpers, builders, and fixtures to remove repetitive setup, but keep the test intent readable at the call site.
- Avoid asserting implementation details that do not affect behavior.

### Testcontainers Guidelines

Use Testcontainers for integration tests that exercise ADO.NET, Npgsql repositories, or other database-dependent infrastructure components.

- Prefer a real PostgreSQL container over mocking ADO.NET behavior when testing repositories, schema interactions, SQL queries, or transaction behavior.
- Use Testcontainers to validate parameterized SQL, result mapping, paging, filtering, soft deletes, and constraint handling against a real database engine.
- Create the container lifecycle in shared test fixtures so startup cost is paid once per test collection when practical.
- Apply schema setup and seed data explicitly in test setup code; keep each test independent and deterministic.
- Open real `NpgsqlConnection` instances against the containerized database and test the production repository implementation directly.
- Cover failure cases that matter for ADO.NET code, including unique constraints, missing rows, null handling, and SQL injection attempts.
- Keep container configuration close to the test project and avoid hidden dependencies on a developer's local database.
- Clean up data between tests using isolated databases, transactional reset strategies, or explicit teardown logic.
- Use integration tests with Testcontainers for Infrastructure and repository behavior, not for pure Application-layer business rules that can remain unit-tested.

### FluentAssertions Guidelines

Use FluentAssertions for backend assertions to keep tests expressive and consistent.

- Prefer FluentAssertions over raw `Assert.Equal`, `Assert.True`, and similar low-level assertions unless required by xUnit APIs.
- Use expressive assertions such as `result.Should().Be(...)`, `collection.Should().HaveCount(...)`, and `await action.Should().ThrowAsync<...>()`.
- Assert object graphs with `BeEquivalentTo` when comparing DTOs or response models.
- Use assertion scopes when validating several related values from the same result.
- Keep exception assertions explicit about the exception type and, when relevant, the message or key error details.
- For async code, assert with async FluentAssertions APIs rather than blocking on `.Result` or `.Wait()`.

## TypeScript And React Guidelines

Write strict, predictable TypeScript. Prefer explicit types at boundaries and inferred types inside small local scopes.

- Prefer named exports for reusable modules.
- Define API request/response contracts near the service or feature that owns them.
- Avoid `any`. Use `unknown` when a value must be narrowed.
- Model async states explicitly when needed: idle, loading, success, error.
- Keep components focused and composable.
- Prefer controlled forms when validation and UX require it.
- Keep side effects in hooks, not in presentational components.
- Use React Context only for app-level shared state such as auth or global notifications.
- Keep routing concerns close to app entry points and feature navigation helpers.
- Use accessibility-first markup: semantic elements, labels, keyboard flow, `aria-*` only when needed.
- All user-facing strings should come from locale resources, not be hardcoded throughout components.
- Format dates and numbers via `Intl` APIs.
- Do not store auth tokens in browser storage.

### React Component Style

- Prefer one component per file unless tightly coupled helper components are trivial.
- Keep components pure where possible.
- Lift network and persistence concerns out of components.
- Prefer composition over prop explosion.
- Use `Suspense` and route-level lazy loading for larger dashboard areas.
- Add explicit loading, empty, error, and success states for data-heavy UI.
- Use error boundaries for feature isolation where appropriate.

## Storybook Guidelines

Storybook is required for isolated UI documentation and state coverage.

- Write stories for every shared component and for feature components with meaningful UI states.
- Cover at least the default, loading, empty, error, and disabled states where applicable.
- Keep stories deterministic. Mock services or pass static props; do not call real APIs.
- Prefer CSF 3 style stories.
- Use realistic data shapes that match feature DTOs.
- Add accessibility-friendly story names and controls.
- Stories should document component contracts, not internal implementation details.

## Cypress Guidelines

Cypress covers critical user journeys and cross-layer integration.

- Put specs under `tests/cypress/e2e`.
- Name files by user journey, for example `login.cy.ts` or `orders-crud.cy.ts`.
- Cover end-to-end flows, not implementation details.
- Prefer stable selectors such as `data-testid` when semantic selectors are not enough.
- Seed or reset data through supported fixtures/helpers rather than brittle UI setup where possible.
- Include `cypress-axe` accessibility checks on critical pages and dialogs.
- Assert observable outcomes: redirects, text, state changes, HTTP results exposed through UI.
- Keep tests isolated and idempotent.

## Testing Expectations

Follow the TDD plan and maintain the intended coverage profile.

- Backend unit tests: xUnit.
- Frontend unit/component tests: Vitest + React Testing Library.
- Integration tests: `WebApplicationFactory` and Testcontainers where applicable.
- E2E tests: Cypress.
- Component documentation and state coverage: Storybook.

### Minimum expectations from the requirements:

- Unit tests cover business logic, UI components, and data access layers.
- Integration tests cover API behavior thoroughly.
- E2E tests cover critical user journeys on demand.
- Accessibility checks are included in unit/component and E2E flows.

## Accessibility, SEO, And Observability Constraints

These are not optional implementation details.

- Public auth and marketing pages must remain server-rendered and SEO-friendly.
- Dashboard pages should be excluded from indexing.
- Use semantic HTML first.
- Meet WCAG 2.1 AA expectations.
- Keep logs structured and centralized through Serilog-based infrastructure.
- Preserve correlation and trace context.
- Do not log secrets, tokens, passwords, or other sensitive PII.

## Things To Avoid

- No direct API calls from React components.
- No ORM-based persistence.
- No auth token storage in `localStorage` or `sessionStorage`.
- No business logic in MVC or API controllers.
- No feature code dumped into `shared` prematurely.
- No inaccessible custom controls when semantic HTML solves the problem.
- No test gaps for loading, empty, error, and auth-failure states.

## Preferred Output Behavior For Code Generation

When generating code for this repository:

- Scaffold complete vertical slices for backend features.
- Scaffold service + hook + component + story + tests for frontend features when appropriate.
- Match the folder structure and naming conventions in this file.
- Include only focused comments where the code would otherwise be hard to parse.
- Keep code production-ready, not tutorial-style.
