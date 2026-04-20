import type { Meta, StoryObj } from '@storybook/react';
import { OrderTracking } from '../components/OrderTracking';
import type { OrderDto, OrderTrackingEvent } from '../types';

const meta = {
  title: 'Features/Orders/OrderTracking',
  component: OrderTracking,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderTracking>;

export default meta;
type Story = StoryObj<typeof meta>;

const mockOrder: OrderDto = {
  id: '1',
  customerId: 'CUST001',
  orderNumber: 'ORD-2024-001',
  status: 'shipped',
  totalAmount: 1250.5,
  currency: 'USD',
  createdAt: '2024-01-15T10:00:00Z',
  updatedAt: '2024-01-19T09:00:00Z',
  items: [
    {
      id: 'item-1',
      productName: 'Laptop',
      quantity: 1,
      unitPrice: 999.99,
      totalPrice: 999.99,
    },
  ],
};

const mockTrackingEvents: OrderTrackingEvent[] = [
  {
    id: '1',
    timestamp: '2024-01-15T10:00:00Z',
    status: 'pending',
    description: 'Order placed',
  },
  {
    id: '2',
    timestamp: '2024-01-15T14:30:00Z',
    status: 'confirmed',
    description: 'Order confirmed',
  },
  {
    id: '3',
    timestamp: '2024-01-18T08:00:00Z',
    status: 'shipped',
    description: 'Order shipped',
  },
];

/**
 * Default tracking view showing active order with timeline
 */
export const Default: Story = {
  args: {
    order: mockOrder,
    trackingEvents: mockTrackingEvents,
    isLoading: false,
    error: null,
  },
};

/**
 * Loading state
 */
export const Loading: Story = {
  args: {
    order: null,
    trackingEvents: [],
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
    trackingEvents: [],
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
    trackingEvents: [],
    isLoading: false,
    error: 'Failed to load tracking information. Please try again.',
  },
};

/**
 * Delivered order showing complete timeline
 */
export const DeliveredOrder: Story = {
  args: {
    order: {
      ...mockOrder,
      status: 'delivered',
      updatedAt: '2024-01-20T14:30:00Z',
    },
    trackingEvents: [
      ...mockTrackingEvents,
      {
        id: '4',
        timestamp: '2024-01-20T14:30:00Z',
        status: 'delivered',
        description: 'Order delivered',
      },
    ],
    isLoading: false,
    error: null,
  },
};

/**
 * Pending order showing only first step
 */
export const PendingOrder: Story = {
  args: {
    order: {
      ...mockOrder,
      status: 'pending',
      updatedAt: '2024-01-15T10:00:00Z',
    },
    trackingEvents: [mockTrackingEvents[0]],
    isLoading: false,
    error: null,
  },
};

/**
 * Tracking with close handler
 */
export const WithCloseHandler: Story = {
  args: {
    order: mockOrder,
    trackingEvents: mockTrackingEvents,
    isLoading: false,
    error: null,
    onClose: () => alert('Close tracking view'),
  },
};
