import type { Meta, StoryObj } from '@storybook/react';
import { OrderEdit } from '../components/OrderEdit';
import { OrderDto } from '../types';

const meta = {
  title: 'Features/Orders/OrderEdit',
  component: OrderEdit,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderEdit>;

export default meta;
type Story = StoryObj<typeof meta>;

const mockOrder: OrderDto = {
  id: '1',
  customerId: 'CUST001',
  orderNumber: 'ORD-2024-001',
  status: 'confirmed',
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
      quantity: 2,
      unitPrice: 29.99,
      totalPrice: 59.98,
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
 * Default edit form with order data populated
 */
export const Default: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: null,
  },
};

/**
 * Loading state while saving changes
 */
export const Saving: Story = {
  args: {
    order: mockOrder,
    isLoading: true,
    error: null,
  },
};

/**
 * No order selected for editing
 */
export const NoOrderSelected: Story = {
  args: {
    order: null,
    isLoading: false,
    error: null,
  },
};

/**
 * Validation error state
 */
export const ValidationError: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: 'Invalid order status or items configuration.',
  },
};

/**
 * Server error state
 */
export const ServerError: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: 'Failed to save order changes. Please try again later.',
  },
};

/**
 * Edit form with handlers
 */
export const WithHandlers: Story = {
  args: {
    order: mockOrder,
    isLoading: false,
    error: null,
    onSubmit: async (data) => {
      alert(`Updating order ${data.id} with status: ${data.status}`);
    },
    onCancel: () => {
      alert('Edit cancelled');
    },
  },
};

/**
 * Pending order ready to be confirmed
 */
export const PendingOrderEdit: Story = {
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
 * Shipped order with limited editing
 */
export const ShippedOrderEdit: Story = {
  args: {
    order: {
      ...mockOrder,
      status: 'shipped',
    },
    isLoading: false,
    error: null,
  },
};

/**
 * Order with single item
 */
export const SingleItemEdit: Story = {
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
