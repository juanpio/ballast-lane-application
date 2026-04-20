import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { OrderEdit } from '../components/OrderEdit';
import type { OrderDto } from '../types';

describe('OrderEdit', () => {
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
        productName: 'Laptop',
        quantity: 1,
        unitPrice: 1000.0,
        totalPrice: 1000.0,
      },
    ],
  };

  it('renders populated edit form', () => {
    render(<OrderEdit order={mockOrder} />);

    expect(screen.getByText(`Edit Order #${mockOrder.orderNumber}`)).toBeInTheDocument();
    expect(screen.getByLabelText('Order Status *')).toHaveValue('confirmed');
    expect(screen.getByLabelText('Product Name 1')).toHaveValue('Laptop');
  });

  it('renders empty message when no order is selected', () => {
    render(<OrderEdit order={null} />);
    expect(screen.getByTestId('order-edit-empty')).toBeInTheDocument();
  });

  it('allows adding and removing items', async () => {
    const user = userEvent.setup();
    render(<OrderEdit order={mockOrder} />);

    expect(screen.getAllByTestId(/edit-item-row-/)).toHaveLength(1);
    await user.click(screen.getByRole('button', { name: 'Add Item' }));
    expect(screen.getAllByTestId(/edit-item-row-/)).toHaveLength(2);

    await user.click(screen.getByRole('button', { name: 'Remove item 2' }));
    expect(screen.getAllByTestId(/edit-item-row-/)).toHaveLength(1);
  });

  it('updates subtotal when values are changed', async () => {
    const user = userEvent.setup();
    render(<OrderEdit order={mockOrder} />);

    await user.clear(screen.getByLabelText('Quantity 1'));
    await user.type(screen.getByLabelText('Quantity 1'), '2');
    await user.clear(screen.getByLabelText('Unit Price 1'));
    await user.type(screen.getByLabelText('Unit Price 1'), '1200');

    expect(screen.getByTestId('edit-subtotal')).toHaveTextContent('2400.00');
  });

  it('submits edited payload', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<OrderEdit order={mockOrder} onSubmit={onSubmit} />);

    await user.selectOptions(screen.getByLabelText('Order Status *'), 'shipped');
    await user.clear(screen.getByLabelText('Product Name 1'));
    await user.type(screen.getByLabelText('Product Name 1'), 'Docking Station');
    await user.clear(screen.getByLabelText('Quantity 1'));
    await user.type(screen.getByLabelText('Quantity 1'), '2');
    await user.clear(screen.getByLabelText('Unit Price 1'));
    await user.type(screen.getByLabelText('Unit Price 1'), '300');

    await user.click(screen.getByRole('button', { name: 'Save Changes' }));

    expect(onSubmit).toHaveBeenCalledWith({
      id: '1',
      status: 'shipped',
      items: [{ productName: 'Docking Station', quantity: 2, unitPrice: 300 }],
    });
  });

  it('shows loading state, external errors and calls cancel', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(
      <OrderEdit
        order={mockOrder}
        isLoading={true}
        error={'Failed to save changes'}
        onCancel={onCancel}
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Failed to save changes');
    expect(screen.getByRole('button', { name: 'Saving...' })).toBeDisabled();

    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
