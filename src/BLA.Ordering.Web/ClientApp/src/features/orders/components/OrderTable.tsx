import type { OrderDto } from '../types';

interface OrderTableProps {
  orders: OrderDto[];
  isLoading?: boolean;
  isEmpty?: boolean;
  error?: string | null;
  total?: number;
  page?: number;
  pageSize?: number;
  onSelectOrder?: (order: OrderDto) => void;
  onEditOrder?: (order: OrderDto) => void;
  onDeleteOrder?: (orderId: string) => void;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

/**
 * OrderTable component displays orders in a tabular format
 * Provides detailed information and quick actions
 */
export function OrderTable({
  orders,
  isLoading = false,
  isEmpty = false,
  error = null,
  total = 0,
  page = 1,
  pageSize = 10,
  onSelectOrder,
  onEditOrder,
  onDeleteOrder,
  onPageChange,
  onPageSizeChange,
}: OrderTableProps) {
  if (isLoading) {
    return <div data-testid="order-table-loading">Loading orders...</div>;
  }

  if (error) {
    return <div data-testid="order-table-error" role="alert">{error}</div>;
  }

  if (isEmpty || orders.length === 0) {
    return <div data-testid="order-table-empty">No orders found</div>;
  }

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div data-testid="order-table">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse border border-gray-300">
          <thead className="bg-gray-100">
            <tr>
              <th className="border border-gray-300 px-4 py-2 text-left">Order #</th>
              <th className="border border-gray-300 px-4 py-2 text-left">Customer</th>
              <th className="border border-gray-300 px-4 py-2 text-left">Amount</th>
              <th className="border border-gray-300 px-4 py-2 text-left">Status</th>
              <th className="border border-gray-300 px-4 py-2 text-left">Date</th>
              <th className="border border-gray-300 px-4 py-2 text-center">Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id} data-testid={`order-row-${order.id}`}>
                <td className="border border-gray-300 px-4 py-2 font-semibold">{order.orderNumber}</td>
                <td className="border border-gray-300 px-4 py-2">{order.customerId}</td>
                <td className="border border-gray-300 px-4 py-2">
                  {order.totalAmount} {order.currency}
                </td>
                <td className="border border-gray-300 px-4 py-2">
                  <span className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">{order.status}</span>
                </td>
                <td className="border border-gray-300 px-4 py-2 text-sm">
                  {new Date(order.createdAt).toLocaleDateString()}
                </td>
                <td className="border border-gray-300 px-4 py-2 text-center">
                  <div className="flex justify-center gap-2">
                    {onSelectOrder && (
                      <button
                        onClick={() => onSelectOrder(order)}
                        className="px-2 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600"
                      >
                        View
                      </button>
                    )}
                    {onEditOrder && (
                      <button
                        onClick={() => onEditOrder(order)}
                        className="px-2 py-1 text-xs bg-yellow-500 text-white rounded hover:bg-yellow-600"
                      >
                        Edit
                      </button>
                    )}
                    {onDeleteOrder && (
                      <button
                        onClick={() => onDeleteOrder(order.id)}
                        className="px-2 py-1 text-xs bg-red-500 text-white rounded hover:bg-red-600"
                      >
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <div>
            <label htmlFor="pageSize" className="mr-2">
              Items per page:
            </label>
            <select
              id="pageSize"
              value={pageSize}
              onChange={(e) => onPageSizeChange?.(Number(e.target.value))}
              className="border border-gray-300 rounded px-2 py-1"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange?.(page - 1)}
              disabled={page === 1}
              className="px-3 py-1 border border-gray-300 rounded disabled:opacity-50"
            >
              Previous
            </button>
            <span className="px-3 py-1">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => onPageChange?.(page + 1)}
              disabled={page === totalPages}
              className="px-3 py-1 border border-gray-300 rounded disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
