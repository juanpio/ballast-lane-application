import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { OrderCreate } from '../components/OrderCreate';

describe('OrderCreate', () => {
  it('renders create order form with key fields', () => {
    render(<OrderCreate />);

    expect(screen.getByText('Create New Order')).toBeInTheDocument();
    expect(screen.getByLabelText('Customer ID *')).toBeInTheDocument();
    expect(screen.getByText('Order Items *')).toBeInTheDocument();
    expect(screen.getByText('Order Summary')).toBeInTheDocument();
  });

  it('adds and removes line items', async () => {
    const user = userEvent.setup();
    render(<OrderCreate />);

    expect(screen.getAllByTestId(/create-item-row-/)).toHaveLength(1);

    await user.click(screen.getByRole('button', { name: 'Add Item' }));
    expect(screen.getAllByTestId(/create-item-row-/)).toHaveLength(2);

    await user.click(screen.getByRole('button', { name: 'Remove item 2' }));
    expect(screen.getAllByTestId(/create-item-row-/)).toHaveLength(1);
  });

  it('updates subtotal when quantity and price change', async () => {
    const user = userEvent.setup();
    render(<OrderCreate />);

    await user.clear(screen.getByLabelText('Product Name 1'));
    await user.type(screen.getByLabelText('Product Name 1'), 'Mouse');
    await user.clear(screen.getByLabelText('Quantity 1'));
    await user.type(screen.getByLabelText('Quantity 1'), '2');
    await user.clear(screen.getByLabelText('Unit Price 1'));
    await user.type(screen.getByLabelText('Unit Price 1'), '49.5');

    expect(screen.getByTestId('create-subtotal')).toHaveTextContent('99.00');
    expect(screen.getByTestId('create-total')).toHaveTextContent('99.00');
  });

  it('shows validation when required values are missing', async () => {
    const user = userEvent.setup();
    render(<OrderCreate onSubmit={vi.fn()} />);

    await user.click(screen.getByRole('button', { name: 'Create Order' }));

    expect(screen.getByRole('alert')).toHaveTextContent('Customer ID is required');
  });

  it('submits normalized payload', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<OrderCreate onSubmit={onSubmit} />);

    await user.type(screen.getByLabelText('Customer ID *'), ' CUST-99 ');
    await user.clear(screen.getByLabelText('Product Name 1'));
    await user.type(screen.getByLabelText('Product Name 1'), ' Keyboard ');
    await user.clear(screen.getByLabelText('Quantity 1'));
    await user.type(screen.getByLabelText('Quantity 1'), '3');
    await user.clear(screen.getByLabelText('Unit Price 1'));
    await user.type(screen.getByLabelText('Unit Price 1'), '25');

    await user.click(screen.getByRole('button', { name: 'Create Order' }));

    expect(onSubmit).toHaveBeenCalledWith({
      customerId: 'CUST-99',
      items: [{ productName: 'Keyboard', quantity: 3, unitPrice: 25 }],
    });
  });

  it('calls onCancel and disables submit while loading', async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(<OrderCreate isLoading={true} onCancel={onCancel} onSubmit={vi.fn()} />);

    expect(screen.getByRole('button', { name: 'Creating...' })).toBeDisabled();
    await user.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
