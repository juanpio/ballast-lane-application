import type { OrderDto, UpdateOrderRequest, OrderStatus } from '../types';
import { useEffect, useMemo, useState } from 'react';

interface OrderEditProps {
  order: OrderDto | null;
  isLoading?: boolean;
  error?: string | null;
  onSubmit?: (data: UpdateOrderRequest) => Promise<void>;
  onCancel?: () => void;
}

/**
 * OrderEdit component provides a form to edit existing orders
 * Allows modification of order items and status
 */
export function OrderEdit({
  order,
  isLoading = false,
  error = null,
  onSubmit,
  onCancel,
}: OrderEditProps) {
  const [status, setStatus] = useState<OrderStatus>(order?.status ?? 'pending');
  const [validationError, setValidationError] = useState<string | null>(null);
  const [items, setItems] = useState(
    (order?.items ?? []).map((item) => ({
      id: item.id,
      productName: item.productName,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
    })),
  );

  if (!order) {
    return <div data-testid="order-edit-empty">No order selected for editing</div>;
  }

  useEffect(() => {
    setStatus(order.status);
    setValidationError(null);
    setItems(
      order.items.map((item) => ({
        id: item.id,
        productName: item.productName,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
      })),
    );
  }, [order]);

  const statusOptions: OrderStatus[] = ['pending', 'confirmed', 'shipped', 'delivered', 'cancelled'];

  const subtotal = useMemo(
    () => items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0),
    [items],
  );

  const updateItem = (id: string, field: 'productName' | 'quantity' | 'unitPrice', value: string) => {
    setItems((current) =>
      current.map((item) => {
        if (item.id !== id) {
          return item;
        }

        if (field === 'productName') {
          return { ...item, productName: value };
        }

        const numericValue = Number(value);
        if (Number.isNaN(numericValue)) {
          return item;
        }

        return { ...item, [field]: numericValue };
      }),
    );
  };

  const addItem = () => {
    setItems((current) => [
      ...current,
      {
        id: crypto.randomUUID(),
        productName: '',
        quantity: 1,
        unitPrice: 0,
      },
    ]);
  };

  const removeItem = (id: string) => {
    setItems((current) => {
      if (current.length === 1) {
        return current;
      }
      return current.filter((item) => item.id !== id);
    });
  };

  const submitForm = async () => {
    if (!onSubmit) {
      return;
    }

    const normalizedItems = items.map((item) => ({
      productName: item.productName.trim(),
      quantity: item.quantity,
      unitPrice: item.unitPrice,
    }));

    if (normalizedItems.some((item) => !item.productName || item.quantity <= 0 || item.unitPrice < 0)) {
      setValidationError('Items must have a product name, quantity greater than zero and a valid price');
      return;
    }

    setValidationError(null);
    await onSubmit({
      id: order.id,
      status,
      items: normalizedItems,
    });
  };

  return (
    <div data-testid="order-edit" className="w-full max-w-2xl mx-auto p-6">
      <h2 className="text-2xl font-bold mb-6">Edit Order #{order.orderNumber}</h2>

      {validationError && (
        <div role="alert" className="mb-4 p-4 bg-red-100 text-red-800 rounded">
          {validationError}
        </div>
      )}

      {error && (
        <div data-testid="order-edit-error" role="alert" className="mb-4 p-4 bg-red-100 text-red-800 rounded">
          {error}
        </div>
      )}

      <form className="space-y-6">
        {/* Status Field */}
        <div>
          <label htmlFor="status" className="block text-sm font-semibold mb-2">
            Order Status *
          </label>
          <select
            id="status"
            value={status}
            onChange={(event) => setStatus(event.target.value as OrderStatus)}
            className="w-full px-4 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
          >
            {statusOptions.map((status) => (
              <option key={status} value={status}>
                {status.charAt(0).toUpperCase() + status.slice(1)}
              </option>
            ))}
          </select>
        </div>

        {/* Order Items Section */}
        <div>
          <label className="block text-sm font-semibold mb-2">Order Items</label>
          <div className="space-y-4 p-4 border border-gray-300 rounded bg-gray-50">
            {items.map((item, index) => (
              <div key={item.id} className="p-4 border border-gray-300 rounded bg-white space-y-3" data-testid={`edit-item-row-${item.id}`}>
                <div className="flex justify-between items-center mb-2">
                  <h4 className="font-semibold">{item.productName || `Item ${index + 1}`}</h4>
                  <button
                    type="button"
                    onClick={() => removeItem(item.id)}
                    className="px-2 py-1 text-xs text-red-600 hover:text-red-800"
                  >
                    {`Remove item ${index + 1}`}
                  </button>
                </div>

                <div>
                  <label htmlFor={`item-${item.id}-product`} className="block text-xs font-semibold mb-1">
                    {`Product Name ${index + 1}`}
                  </label>
                  <input
                    id={`item-${item.id}-product`}
                    type="text"
                    value={item.productName}
                    onChange={(event) => updateItem(item.id, 'productName', event.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                  />
                </div>

                <div className="grid grid-cols-3 gap-2">
                  <div>
                    <label htmlFor={`item-${item.id}-quantity`} className="block text-xs font-semibold mb-1">
                      {`Quantity ${index + 1}`}
                    </label>
                    <input
                      id={`item-${item.id}-quantity`}
                      type="number"
                      value={item.quantity}
                      onChange={(event) => updateItem(item.id, 'quantity', event.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      min="1"
                    />
                  </div>
                  <div>
                    <label htmlFor={`item-${item.id}-unitPrice`} className="block text-xs font-semibold mb-1">
                      {`Unit Price ${index + 1}`}
                    </label>
                    <input
                      id={`item-${item.id}-unitPrice`}
                      type="number"
                      value={item.unitPrice}
                      onChange={(event) => updateItem(item.id, 'unitPrice', event.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      step="0.01"
                      min="0"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold mb-1">Total</label>
                    <div className="px-3 py-2 bg-gray-100 rounded text-sm">{(item.quantity * item.unitPrice).toFixed(2)}</div>
                  </div>
                </div>
              </div>
            ))}

            <button
              type="button"
              onClick={addItem}
              className="w-full px-3 py-2 text-sm border-2 border-dashed border-gray-300 rounded text-gray-600 hover:border-gray-400"
            >
              Add Item
            </button>
          </div>
        </div>

        {/* Order Summary */}
        <div className="p-4 bg-gray-50 rounded">
          <h3 className="font-semibold mb-3">Order Summary</h3>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span>Subtotal:</span>
              <span data-testid="edit-subtotal">{subtotal.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span>Tax:</span>
              <span>0.00</span>
            </div>
            <div className="flex justify-between font-bold text-lg border-t pt-2">
              <span>Total:</span>
              <span>{subtotal.toFixed(2)} {order.currency}</span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3 justify-end">
          {onCancel && (
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
            >
              Cancel
            </button>
          )}
          {onSubmit && (
            <button
              type="submit"
              disabled={isLoading}
              onClick={async (e) => {
                e.preventDefault();
                await submitForm();
              }}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50"
            >
              {isLoading ? 'Saving...' : 'Save Changes'}
            </button>
          )}
        </div>
      </form>
    </div>
  );
}
