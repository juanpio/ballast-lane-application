# Orders Feature - TDD Scaffolding Guide

## Overview

This document describes the Test-Driven Development (TDD) scaffolding for the Orders feature components. The scaffold is designed with Storybook-first approach, providing component contracts before implementation.

## Project Structure

```
src/features/orders/
├── components/
│   ├── index.ts                    # Component exports
│   ├── OrderGrid.tsx               # Grid/card view component
│   ├── OrderTable.tsx              # Table/list view component
│   ├── OrderTracking.tsx           # Tracking timeline component
│   ├── OrderCreate.tsx             # Create order form component
│   ├── OrderDetail.tsx             # Order detail view component
│   └── OrderEdit.tsx               # Edit order form component
├── hooks/
│   ├── index.ts                    # Hook exports
│   ├── useOrders.ts                # Fetch paginated orders
│   ├── useOrderDetail.ts           # Fetch single order
│   ├── useOrderTracking.ts         # Fetch tracking events
│   ├── useCreateOrder.ts           # Create order mutation
│   ├── useUpdateOrder.ts           # Update order mutation
│   └── useDeleteOrder.ts           # Delete order mutation
├── stories/
│   ├── OrderGrid.stories.tsx       # Grid component stories
│   ├── OrderTable.stories.tsx      # Table component stories
│   ├── OrderTracking.stories.tsx   # Tracking component stories
│   ├── OrderCreate.stories.tsx     # Create form stories
│   ├── OrderDetail.stories.tsx     # Detail view stories
│   └── OrderEdit.stories.tsx       # Edit form stories
├── __tests__/
│   ├── OrderGrid.test.tsx          # Grid component tests
│   ├── OrderTable.test.tsx         # Table component tests
│   ├── OrderTracking.test.tsx      # Tracking component tests
│   ├── OrderCreate.test.tsx        # Create form tests
│   ├── OrderDetail.test.tsx        # Detail view tests
│   └── OrderEdit.test.tsx          # Edit form tests
├── types.ts                        # Shared type definitions
└── index.ts                        # Feature exports
```

## Component Descriptions

### 1. **OrderGrid** - Grid/Card View
Display orders in a responsive grid layout with card components.

**States:**
- `loading`: Shows loading placeholder
- `empty`: No orders available
- `error`: Display error message
- `success`: Show grid of order cards

**Interactions:**
- View order details
- Edit order
- Delete order

**Stories Location:** [OrderGrid.stories.tsx](./stories/OrderGrid.stories.tsx)

---

### 2. **OrderTable** - Table/List View
Display orders in a structured table with pagination.

**States:**
- `loading`: Show loading state
- `empty`: No orders message
- `error`: Error message display
- `success`: Display paginated table

**Features:**
- Pagination controls
- Page size selector
- Row actions (view, edit, delete)

**Stories Location:** [OrderTable.stories.tsx](./stories/OrderTable.stories.tsx)

---

### 3. **OrderTracking** - Tracking Timeline
Visual timeline showing order status progression.

**States:**
- `loading`: Loading tracking data
- `empty`: No order selected
- `error`: Error loading tracking
- `success`: Show timeline with events

**Features:**
- Visual timeline with status progression
- Tracking events with timestamps
- Order details summary

**Stories Location:** [OrderTracking.stories.tsx](./stories/OrderTracking.stories.tsx)

---

### 4. **OrderCreate** - Create Form
Form for creating new orders with items.

**States:**
- `idle`: Empty form ready for input
- `loading`: Submitting order
- `error`: Display validation/submission errors
- `success`: Order created (handled by parent)

**Features:**
- Customer ID input
- Dynamic item management
- Order summary calculation
- Form validation

**Stories Location:** [OrderCreate.stories.tsx](./stories/OrderCreate.stories.tsx)

---

### 5. **OrderDetail** - Detail View
Comprehensive view of a single order with all details.

**States:**
- `loading`: Fetching order details
- `empty`: No order selected
- `error`: Error loading order
- `success`: Display full order information

**Features:**
- Order header with status badge
- Summary cards (customer, amount, dates)
- Items table with totals
- Action buttons (edit, delete, close)

**Stories Location:** [OrderDetail.stories.tsx](./stories/OrderDetail.stories.tsx)

---

### 6. **OrderEdit** - Edit Form
Form for modifying existing orders.

**States:**
- `loading`: Saving changes
- `empty`: No order selected for editing
- `error`: Validation/submission errors
- `success`: Form populated with order data

**Features:**
- Status selector with all available statuses
- Edit individual order items
- Add/remove items
- Order summary with calculations
- Form validation

**Stories Location:** [OrderEdit.stories.tsx](./stories/OrderEdit.stories.tsx)

---

## Hooks Descriptions

### Data Fetching Hooks

#### `useOrders`
Fetches paginated list of orders.

```typescript
const { orders, total, page, pageSize, isLoading, error } = useOrders({
  page: 1,
  pageSize: 10,
  status: 'shipped' // optional filter
});
```

#### `useOrderDetail`
Fetches a single order by ID.

```typescript
const { order, isLoading, error } = useOrderDetail(orderId);
```

#### `useOrderTracking`
Fetches tracking events for an order.

```typescript
const { trackingEvents, isLoading, error } = useOrderTracking(orderId);
```

### Mutation Hooks

#### `useCreateOrder`
Creates a new order.

```typescript
const { isLoading, error, createOrder } = useCreateOrder();
await createOrder(createOrderData);
```

#### `useUpdateOrder`
Updates an existing order.

```typescript
const { isLoading, error, updateOrder } = useUpdateOrder();
await updateOrder(updateOrderData);
```

#### `useDeleteOrder`
Deletes an order.

```typescript
const { isLoading, error, deleteOrder } = useDeleteOrder();
await deleteOrder(orderId);
```

---

## Storybook Stories

Each component has comprehensive stories covering:

### Standard Stories
- **Default:** Happy path with typical data
- **Loading:** Loading state visualization
- **Empty:** Empty/no-data state
- **Error:** Error message display
- **WithInteractions:** Interactive variants with mock handlers

### Component-Specific Stories
- **WithPagination** (Table): Shows pagination controls
- **WithActions** (Table): Shows all action buttons enabled
- **DeliveredOrder** (Tracking): Complete timeline
- **PendingOrder** (Tracking): Initial status
- **SingleOrder** (various): Single item focus
- **ValidationError / ServerError** (Forms): Different error types

## Running Storybook

```bash
cd src/BLA.Ordering.Web/ClientApp
npm run storybook
# or
npx storybook dev
```

Visit `http://localhost:6006` to view component stories.

---

## Test Structure

Each component has a corresponding test file with the following structure:

```typescript
describe('ComponentName', () => {
  describe('state_name', () => {
    it('should render expected content', () => {
      // AAA: Arrange, Act, Assert
    });
  });
});
```

### Test Categories
- **State Tests:** Loading, empty, error, success states
- **Interaction Tests:** Button clicks, form submissions, callbacks
- **Accessibility Tests:** ARIA labels, keyboard navigation, semantics

### Running Tests

```bash
cd src/BLA.Ordering.Web/ClientApp
npm test                  # Run tests
npm run test:coverage     # Run with coverage report
```

---

## Type System

All components are fully typed using shared type definitions in `types.ts`:

```typescript
// Core domain types
OrderDto                  // Order object
OrderItemDto              // Order item
OrderStatus               // Status enum
OrderTrackingEvent        // Tracking event

// Request/Response types
CreateOrderRequest
CreateOrderItemRequest
UpdateOrderRequest
OrdersListParams
OrdersListResponse
```

---

## TDD Implementation Workflow

### Phase 1: Storybook Definition ✅
Stories define the component contract and visual states.

### Phase 2: Implement Component Logic (TODO)
1. Update component prop handlers
2. Implement state management with hooks
3. Add form logic and validation
4. Connect to services for API calls

### Phase 3: Implement Tests (TODO)
1. Write unit tests matching story scenarios
2. Test component rendering and interactions
3. Add accessibility tests
4. Test error boundaries

### Phase 4: Implement Hooks (TODO)
1. Create API service calls in hooks
2. Add error handling and retries
3. Implement loading states
4. Add cancellation token support

### Phase 5: Integration (TODO)
1. Connect components to hooks
2. Test end-to-end workflows
3. Add E2E tests with Cypress
4. Performance optimization

---

## API Integration Points

Components expect these API endpoints (to be implemented):

```
GET    /api/orders                    # List orders
GET    /api/orders/:id                # Get order details
POST   /api/orders                    # Create order
PUT    /api/orders/:id                # Update order
DELETE /api/orders/:id                # Delete order
GET    /api/orders/:id/tracking       # Get tracking events
```

---

## Accessibility Guidelines

All components follow WCAG 2.1 AA standards:

- ✅ Semantic HTML with proper heading hierarchy
- ✅ Form labels associated with inputs
- ✅ ARIA roles and attributes where needed
- ✅ Keyboard navigation support
- ✅ Focus management
- ✅ Error announcements with `role="alert"`
- ✅ Loading states announced

---

## Next Steps

1. **Review Storybook:** Open Storybook and explore all component states
2. **Implement Services:** Create API service calls in `core/services`
3. **Implement Hooks:** Connect hooks to services with error/loading handling
4. **Implement Tests:** Fill in test implementations based on story states
5. **Implement Components:** Add form logic, validation, and component logic
6. **Integration Testing:** Add E2E tests with Cypress

---

## Resources

- [Storybook Documentation](https://storybook.js.org/)
- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [Cypress Testing](https://www.cypress.io/)
- [Project Copilot Instructions](./../../../.github/copilot-instructions.md)

---

## Questions?

Refer to the project's copilot instructions for architecture guidelines and the ADR documentation for architectural decisions.
