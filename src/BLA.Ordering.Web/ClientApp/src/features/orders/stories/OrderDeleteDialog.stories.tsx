import type { Meta, StoryObj } from '@storybook/react';
import { OrderDeleteDialog } from '../components/OrderDeleteDialog';
import type { OrderDto } from '../types';

const mockOrder: OrderDto = {
  id: 'ord-1005',
  customerId: 'CUST003',
  orderNumber: 'BL-2026-1005',
  status: 'pending',
  totalAmount: 320,
  currency: 'USD',
  createdAt: '2026-04-20T09:00:00Z',
  updatedAt: '2026-04-20T09:00:00Z',
  items: [],
};

const meta = {
  title: 'Features/Orders/OrderDeleteDialog',
  component: OrderDeleteDialog,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderDeleteDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    isOpen: true,
    order: mockOrder,
    isLoading: false,
    error: null,
  },
};

export const Loading: Story = {
  args: {
    isOpen: true,
    order: mockOrder,
    isLoading: true,
    error: null,
  },
};

export const WithError: Story = {
  args: {
    isOpen: true,
    order: mockOrder,
    isLoading: false,
    error: 'Failed to delete order. Please retry.',
  },
};

export const Closed: Story = {
  args: {
    isOpen: false,
    order: mockOrder,
  },
};
