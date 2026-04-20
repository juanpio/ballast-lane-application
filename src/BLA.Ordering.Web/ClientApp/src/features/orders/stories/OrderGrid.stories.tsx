import type { Meta, StoryObj } from '@storybook/react';
import { OrderGrid } from './OrderGrid';
import { OrderDto } from '../types';

const meta = {
  title: 'Features/Orders/OrderGrid',
  component: OrderGrid,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderGrid>;

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
      {
        id: 'item-2',
        productName: 'Mouse',
        quantity: 1,
        unitPrice: 29.99,
        totalPrice: 29.99,
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
 * Default state showing a grid of orders
 */
export const Default: Story = {
  args: {
    orders: mockOrders,
    isLoading: false,
    isEmpty: false,
    error: null,
  },
};

/**
 * Loading state with skeleton/placeholder display
 */
export const Loading: Story = {
  args: {
    orders: [],
    isLoading: true,
    isEmpty: false,
    error: null,
  },
};

/**
 * Empty state when no orders exist
 */
export const Empty: Story = {
  args: {
    orders: [],
    isLoading: false,
    isEmpty: true,
    error: null,
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
    error: 'Failed to load orders. Please try again later.',
  },
};

/**
 * Grid with click handlers for user interactions
 */
export const WithInteractions: Story = {
  args: {
    orders: mockOrders,
    isLoading: false,
    isEmpty: false,
    error: null,
    onSelectOrder: (order) => alert(`Selected order: ${order.orderNumber}`),
    onEditOrder: (order) => alert(`Editing order: ${order.orderNumber}`),
    onDeleteOrder: (orderId) => alert(`Deleted order: ${orderId}`),
  },
};

/**
 * Single order in grid for focus testing
 */
export const SingleOrder: Story = {
  args: {
    orders: [mockOrders[0]],
    isLoading: false,
    isEmpty: false,
    error: null,
    onSelectOrder: (order) => alert(`Selected order: ${order.orderNumber}`),
  },
};
