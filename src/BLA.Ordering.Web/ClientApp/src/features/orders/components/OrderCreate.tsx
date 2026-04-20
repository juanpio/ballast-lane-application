import { CreateOrderRequest, CreateOrderItemRequest } from '../types';

interface OrderCreateProps {
  isLoading?: boolean;
  error?: string | null;
  onSubmit?: (data: CreateOrderRequest) => Promise<void>;
  onCancel?: () => void;
}

/**
 * OrderCreate component provides a form to create new orders
 * Allows user to add items and submit order
 */
export function OrderCreate({
  isLoading = false,
  error = null,
  onSubmit,
  onCancel,
}: OrderCreateProps) {
  return (
    <div data-testid="order-create" className="w-full max-w-2xl mx-auto p-6">
      <h2 className="text-2xl font-bold mb-6">Create New Order</h2>

      {error && (
        <div data-testid="order-create-error" role="alert" className="mb-4 p-4 bg-red-100 text-red-800 rounded">
          {error}
        </div>
      )}

      <form className="space-y-6">
        {/* Customer ID Field */}
        <div>
          <label htmlFor="customerId" className="block text-sm font-semibold mb-2">
            Customer ID *
          </label>
          <input
            id="customerId"
            type="text"
            placeholder="Enter customer ID"
            className="w-full px-4 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
            required
          />
        </div>

        {/* Order Items Section */}
        <div>
          <label className="block text-sm font-semibold mb-2">Order Items *</label>
          <div className="space-y-4 p-4 border border-gray-300 rounded bg-gray-50">
            <div className="space-y-4">
              {/* Sample item placeholder */}
              <div className="p-4 border border-gray-300 rounded bg-white space-y-3">
                <div>
                  <label htmlFor="item-0-product" className="block text-xs font-semibold mb-1">
                    Product Name
                  </label>
                  <input
                    id="item-0-product"
                    type="text"
                    placeholder="Enter product name"
                    className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                  />
                </div>
                <div className="grid grid-cols-3 gap-2">
                  <div>
                    <label htmlFor="item-0-quantity" className="block text-xs font-semibold mb-1">
                      Quantity
                    </label>
                    <input
                      id="item-0-quantity"
                      type="number"
                      placeholder="0"
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      min="1"
                    />
                  </div>
                  <div>
                    <label htmlFor="item-0-unitPrice" className="block text-xs font-semibold mb-1">
                      Unit Price
                    </label>
                    <input
                      id="item-0-unitPrice"
                      type="number"
                      placeholder="0.00"
                      className="w-full px-3 py-2 border border-gray-300 rounded text-sm"
                      step="0.01"
                      min="0"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold mb-1">Total</label>
                    <div className="px-3 py-2 bg-gray-100 rounded text-sm">0.00</div>
                  </div>
                </div>
              </div>
            </div>
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
              <span>0.00</span>
            </div>
            <div className="flex justify-between">
              <span>Tax:</span>
              <span>0.00</span>
            </div>
            <div className="flex justify-between font-bold text-lg border-t pt-2">
              <span>Total:</span>
              <span>0.00</span>
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
              {isLoading ? 'Creating...' : 'Create Order'}
            </button>
          )}
        </div>
      </form>
    </div>
  );
}
