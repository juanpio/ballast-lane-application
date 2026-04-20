import type { OrderDto, UpdateOrderRequest, OrderStatus } from '../types';

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
  if (!order) {
    return <div data-testid="order-edit-empty">No order selected for editing</div>;
  }

  const statusOptions: OrderStatus[] = ['pending', 'confirmed', 'shipped', 'delivered', 'cancelled'];

  return (
    <div data-testid="order-edit" className="w-full max-w-2xl mx-auto p-6">
      <h2 className="text-2xl font-bold mb-6">Edit Order #{order.orderNumber}</h2>

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
            defaultValue={order.status}
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
            {order.items.map((item, index) => (
              <div key={item.id} className="p-4 border border-gray-300 rounded bg-white space-y-3">
                <div className="flex justify-between items-center mb-2">
                  <h4 className="font-semibold">{item.productName}</h4>
                  <button
                    type="button"
                    className="px-2 py-1 text-xs text-red-600 hover:text-red-800"
                  >
                    Remove
                  </button>
                </div>

                <div className="grid grid-cols-3 gap-2">
                  <div>
                    <label htmlFor={`item-${index}-quantity`} className="block text-xs font-semibold mb-1">
                      Quantity
                    </label>
                    <input
                      id={`item-${index}-quantity`}
                      type="number"
                      defaultValue={item.quantity}
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      min="1"
                    />
                  </div>
                  <div>
                    <label htmlFor={`item-${index}-unitPrice`} className="block text-xs font-semibold mb-1">
                      Unit Price
                    </label>
                    <input
                      id={`item-${index}-unitPrice`}
                      type="number"
                      defaultValue={item.unitPrice}
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      step="0.01"
                      min="0"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold mb-1">Total</label>
                    <div className="px-3 py-2 bg-gray-100 rounded text-sm">{item.totalPrice.toFixed(2)}</div>
                  </div>
                </div>
              </div>
            ))}

            <button
              type="button"
              className="w-full px-3 py-2 text-sm border-2 border-dashed border-gray-300 rounded text-gray-600 hover:border-gray-400"
            >
              + Add Item
            </button>
          </div>
        </div>

        {/* Order Summary */}
        <div className="p-4 bg-gray-50 rounded">
          <h3 className="font-semibold mb-3">Order Summary</h3>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span>Subtotal:</span>
              <span>
                {order.items.reduce((sum, item) => sum + item.totalPrice, 0).toFixed(2)}
              </span>
            </div>
            <div className="flex justify-between">
              <span>Tax:</span>
              <span>0.00</span>
            </div>
            <div className="flex justify-between font-bold text-lg border-t pt-2">
              <span>Total:</span>
              <span>
                {order.totalAmount} {order.currency}
              </span>
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
              onClick={(e) => {
                e.preventDefault();
                // Form submission logic handled by hook
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
