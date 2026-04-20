import { OrderDto } from '../types';

interface OrderDetailProps {
  order: OrderDto | null;
  isLoading?: boolean;
  error?: string | null;
  onEdit?: () => void;
  onDelete?: () => void;
  onClose?: () => void;
}

/**
 * OrderDetail component displays comprehensive information about a single order
 * Shows all items, pricing, and dates
 */
export function OrderDetail({
  order,
  isLoading = false,
  error = null,
  onEdit,
  onDelete,
  onClose,
}: OrderDetailProps) {
  if (isLoading) {
    return <div data-testid="order-detail-loading">Loading order details...</div>;
  }

  if (error) {
    return <div data-testid="order-detail-error" role="alert">{error}</div>;
  }

  if (!order) {
    return <div data-testid="order-detail-empty">No order selected</div>;
  }

  return (
    <div data-testid="order-detail" className="w-full max-w-3xl mx-auto p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Order #{order.orderNumber}</h1>
        {onClose && (
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700 text-2xl">
            ×
          </button>
        )}
      </div>

      {/* Status Badge */}
      <div className="mb-6">
        <span className="px-4 py-2 bg-blue-100 text-blue-800 rounded-full font-semibold capitalize">
          {order.status}
        </span>
      </div>

      {/* Order Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div className="p-4 border rounded bg-gray-50">
          <p className="text-gray-600 text-sm">Customer ID</p>
          <p className="font-semibold">{order.customerId}</p>
        </div>
        <div className="p-4 border rounded bg-gray-50">
          <p className="text-gray-600 text-sm">Total Amount</p>
          <p className="font-semibold text-lg">
            {order.totalAmount} {order.currency}
          </p>
        </div>
        <div className="p-4 border rounded bg-gray-50">
          <p className="text-gray-600 text-sm">Created</p>
          <p className="font-semibold text-sm">{new Date(order.createdAt).toLocaleDateString()}</p>
        </div>
        <div className="p-4 border rounded bg-gray-50">
          <p className="text-gray-600 text-sm">Updated</p>
          <p className="font-semibold text-sm">{new Date(order.updatedAt).toLocaleDateString()}</p>
        </div>
      </div>

      {/* Order Items Table */}
      <div className="mb-8">
        <h2 className="text-xl font-bold mb-4">Order Items</h2>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse border border-gray-300">
            <thead className="bg-gray-100">
              <tr>
                <th className="border border-gray-300 px-4 py-2 text-left">Product</th>
                <th className="border border-gray-300 px-4 py-2 text-right">Quantity</th>
                <th className="border border-gray-300 px-4 py-2 text-right">Unit Price</th>
                <th className="border border-gray-300 px-4 py-2 text-right">Total</th>
              </tr>
            </thead>
            <tbody>
              {order.items.map((item) => (
                <tr key={item.id} data-testid={`order-item-${item.id}`}>
                  <td className="border border-gray-300 px-4 py-2">{item.productName}</td>
                  <td className="border border-gray-300 px-4 py-2 text-right">{item.quantity}</td>
                  <td className="border border-gray-300 px-4 py-2 text-right">{item.unitPrice}</td>
                  <td className="border border-gray-300 px-4 py-2 text-right font-semibold">
                    {item.totalPrice}
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-gray-50">
              <tr>
                <td colSpan={3} className="border border-gray-300 px-4 py-2 text-right font-bold">
                  Total:
                </td>
                <td className="border border-gray-300 px-4 py-2 text-right font-bold text-lg">
                  {order.totalAmount} {order.currency}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

      {/* Actions */}
      <div className="flex gap-3 justify-end pt-6 border-t">
        {onEdit && (
          <button
            onClick={onEdit}
            className="px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600"
          >
            Edit Order
          </button>
        )}
        {onDelete && (
          <button
            onClick={onDelete}
            className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          >
            Delete Order
          </button>
        )}
        {onClose && (
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
          >
            Close
          </button>
        )}
      </div>
    </div>
  );
}
