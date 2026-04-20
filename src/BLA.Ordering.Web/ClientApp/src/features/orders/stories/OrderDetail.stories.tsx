import type { Meta, StoryObj } from '@storybook/react';
import { OrderDetail } from '../components/OrderDetail';
import type { OrderDto } from '../types';

const meta = {
  title: 'Features/Orders/OrderDetail',
  component: OrderDetail,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderDetail>;

export default meta;
type Story = StoryObj<typeof meta>;

const mockOrder: OrderDto = {
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
      productName: 'Laptop Computer',
      quantity: 1,
      unitPrice: 999.99,
      totalPrice: 999.99,
    },
    {
      id: 'item-2',
      productName: 'Wireless Mouse',
      quantity: 1,
      unitPrice: 29.99,
      totalPrice: 29.99,
    },
    {
      id: 'item-3',
      productName: 'USB-C Cable',
      quantity: 3,
      unitPrice: 10.51,
      totalPrice: 31.53,
    },
  ],
};

/**
 * Default view of an order with full details
 */
export const Default: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: null,
  },
};

/**
 * Loading state while fetching order details
 */
export const Loading: Story = {
  args: {
    order: null,
    isLoading: true,
    error: null,
  },
};

/**
 * No order selected state
 */
export const NoOrderSelected: Story = {
  args: {
    order: null,
    isLoading: false,
    error: null,
  },
};

/**
 * Error state with error message
 */
export const Error: Story = {
  args: {
    order: null,
    isLoading: false,
    error: 'Failed to load order details. Please try again.',
  },
};

/**
 * Order with single item
 */
export const SingleItemOrder: Story = {
  args: {
    order: {
      ...mockOrder,
      items: [mockOrder.items[0]],
      totalAmount: 999.99,
    },
    isLoading: false,
    error: null,
  },
};

/**
 * Pending order showing early status
 */
export const PendingOrder: Story = {
  args: {
    order: {
      ...mockOrder,
      status: 'pending',
    },
    isLoading: false,
    error: null,
  },
};

/**
 * Detail view with action handlers
 */
export const WithActions: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: null,
    onEdit: () => alert('Edit order'),
    onDelete: () => alert('Delete order'),
    onClose: () => alert('Close detail view'),
  },
};

/**
 * Cancelled order
 */
export const CancelledOrder: Story = {
  args: {
    order: {
      ...mockOrder,
      status: 'cancelled',
    },
    isLoading: false,
    error: null,
    onClose: () => alert('Close'),
  },
};
