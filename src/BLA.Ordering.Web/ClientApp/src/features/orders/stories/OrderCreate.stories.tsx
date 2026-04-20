import type { Meta, StoryObj } from '@storybook/react';
import { OrderCreate } from '../components/OrderCreate';

const meta = {
  title: 'Features/Orders/OrderCreate',
  component: OrderCreate,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof OrderCreate>;

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Default empty form ready for input
 */
export const Default: Story = {
  args: {
    isLoading: false,
    error: null,
  },
};

/**
 * Loading state while submitting
 */
export const Loading: Story = {
  args: {
    isLoading: true,
    error: null,
  },
};

/**
 * Error state with validation message
 */
export const WithValidationError: Story = {
  args: {
    isLoading: false,
    error: 'Customer ID is required and must be valid.',
  },
};

/**
 * Error state with server error
 */
export const WithServerError: Story = {
  args: {
    isLoading: false,
    error: 'Failed to create order. Please try again later.',
  },
};

/**
 * Form with submit and cancel handlers
 */
export const WithHandlers: Story = {
  args: {
    isLoading: false,
    error: null,
    onSubmit: async (data) => {
      alert(`Submitting order for customer: ${data.customerId}`);
    },
    onCancel: () => {
      alert('Create order cancelled');
    },
  },
};

/**
 * Form showing error after failed submission
 */
export const ErrorAfterSubmit: Story = {
  args: {
    isLoading: false,
    error: 'Order creation failed: Invalid customer ID format.',
    onCancel: () => alert('Dismissing form'),
  },
};

/**
 * Form in loading state with interaction attempt
 */
export const SubmittingOrder: Story = {
  args: {
    isLoading: true,
    error: null,
    onSubmit: async (data) => {
      // Simulating slow submission
      await new Promise((resolve) => setTimeout(resolve, 2000));
      alert(`Order created successfully!`);
    },
  },
};
