import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { OrderDeleteDialog } from '../components/OrderDeleteDialog';
import type { OrderDto } from '../types';

const mockOrder: OrderDto = {
  id: 'ord-1',
  customerId: 'CUST001',
  orderNumber: 'ORD-2026-001',
  status: 'pending',
  totalAmount: 120,
  currency: 'USD',
  createdAt: '2026-04-20T10:00:00Z',
  updatedAt: '2026-04-20T10:00:00Z',
  items: [],
};

describe('OrderDeleteDialog', () => {
  it('does not render when closed', () => {
    render(<OrderDeleteDialog isOpen={false} order={mockOrder} />);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('renders confirmation details when open', () => {
    render(<OrderDeleteDialog isOpen={true} order={mockOrder} />);

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Delete order ORD-2026-001?')).toBeInTheDocument();
    expect(screen.getByText('USD 120.00')).toBeInTheDocument();
  });

  it('calls onConfirm with order id', async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn().mockResolvedValue(undefined);

    render(<OrderDeleteDialog isOpen={true} order={mockOrder} onConfirm={onConfirm} />);

    await user.click(screen.getByRole('button', { name: 'Delete Order' }));
    expect(onConfirm).toHaveBeenCalledWith('ord-1');
  });

  it('calls onCancel when cancel is clicked', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();

    render(<OrderDeleteDialog isOpen={true} order={mockOrder} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it('shows loading and error states', () => {
    render(
      <OrderDeleteDialog
        isOpen={true}
        order={mockOrder}
        isLoading={true}
        error={'Delete failed'}
        onConfirm={vi.fn()}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Delete failed');
    expect(screen.getByRole('button', { name: 'Deleting...' })).toBeDisabled();
  });
});
