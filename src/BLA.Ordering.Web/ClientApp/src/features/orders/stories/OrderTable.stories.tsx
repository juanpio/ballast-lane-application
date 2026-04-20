import type { Meta, StoryObj } from '@storybook/react';
import { OrderTable } from '../components/OrderTable';
import { OrderDto } from '../types';

const meta = {
  title: 'Features/Orders/OrderTable',
  component: OrderTable,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderTable>;

export default meta;
type Story = StoryObj<typeof meta>;

const mockOrders: OrderDto[] = [
  {
    id: '1',
    customerId: 'CUST001',
    orderNumber: 'ORD-2024-001',
    status: 'delivered',
    totalAmount: 1250.5,
    currency: 'USD',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-20T14:30:00Z',
    items: [
      {
        id: 'item-1',
        productName: 'Laptop',
        quantity: 1,
        unitPrice: 999.99,
        totalPrice: 999.99,
      },
    ],
  },
  {
    id: '2',
    customerId: 'CUST002',
    orderNumber: 'ORD-2024-002',
    status: 'shipped',
    totalAmount: 75.0,
    currency: 'USD',
    createdAt: '2024-01-18T11:00:00Z',
    updatedAt: '2024-01-19T09:00:00Z',
    items: [
      {
        id: 'item-3',
        productName: 'Keyboard',
        quantity: 1,
        unitPrice: 75.0,
        totalPrice: 75.0,
      },
    ],
  },
  {
    id: '3',
    customerId: 'CUST003',
    orderNumber: 'ORD-2024-003',
    status: 'confirmed',
    totalAmount: 450.0,
    currency: 'USD',
    createdAt: '2024-01-20T08:30:00Z',
    updatedAt: '2024-01-20T08:30:00Z',
    items: [
      {
        id: 'item-4',
        productName: 'Monitor',
        quantity: 1,
        unitPrice: 450.0,
        totalPrice: 450.0,
      },
    ],
  },
];

/**
 * Default table view with paginated orders
 */
export const Default: Story = {
  args: {
    orders: mockOrders,
    isLoading: false,
    isEmpty: false,
    error: null,
    total: 150,
    page: 1,
    pageSize: 10,
  },
};

/**
 * Loading state showing placeholder
 */
export const Loading: Story = {
  args: {
    orders: [],
    isLoading: true,
    isEmpty: false,
    error: null,
    total: 0,
  },
};

/**
 * Empty state with no orders
 */
export const Empty: Story = {
  args: {
    orders: [],
    isLoading: false,
    isEmpty: true,
    error: null,
    total: 0,
  },
};

/**
 * Error state with error message
 */
export const Error: Story = {
  args: {
    orders: [],
    isLoading: false,
    isEmpty: false,
    error: 'Failed to load orders. Please try again.',
    total: 0,
  },
};

/**
 * Single order in table
 */
export const SingleOrder: Story = {
  args: {
    orders: [mockOrders[0]],
    isLoading: false,
    isEmpty: false,
    error: null,
    total: 1,
    page: 1,
    pageSize: 10,
  },
};

/**
 * Table with pagination controls and interaction handlers
 */
export const WithPagination: Story = {
  args: {
    orders: mockOrders,
    isLoading: false,
    isEmpty: false,
    error: null,
    total: 150,
    page: 1,
    pageSize: 10,
    onPageChange: (page) => alert(`Navigate to page: ${page}`),
    onPageSizeChange: (size) => alert(`Change page size to: ${size}`),
  },
};

/**
 * Table with row actions enabled
 */
export const WithActions: Story = {
  args: {
    orders: mockOrders,
    isLoading: false,
    isEmpty: false,
    error: null,
    total: 150,
    page: 1,
    pageSize: 10,
    onSelectOrder: (order) => alert(`View order: ${order.orderNumber}`),
    onEditOrder: (order) => alert(`Edit order: ${order.orderNumber}`),
    onDeleteOrder: (orderId) => alert(`Delete order: ${orderId}`),
  },
};

/**
 * Last page with fewer items
 */
export const LastPage: Story = {
  args: {
    orders: [mockOrders[0]],
    isLoading: false,
    isEmpty: false,
    error: null,
    total: 25,
    page: 3,
    pageSize: 10,
  },
};
