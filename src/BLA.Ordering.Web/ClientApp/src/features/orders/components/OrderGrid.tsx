import type { OrderDto } from '../types';

interface OrderGridProps {
  orders: OrderDto[];
  isLoading?: boolean;
  isEmpty?: boolean;
  error?: string | null;
  onSelectOrder?: (order: OrderDto) => void;
  onEditOrder?: (order: OrderDto) => void;
  onDeleteOrder?: (orderId: string) => void;
}

/**
 * OrderGrid component displays a grid/card view of orders
 * Used for visual browsing of multiple orders with key information
 */
export function OrderGrid({
  orders,
  isLoading = false,
  isEmpty = false,
  error = null,
  onSelectOrder,
  onEditOrder,
  onDeleteOrder,
}: OrderGridProps) {
  if (isLoading) {
    return <div data-testid="order-grid-loading">Loading orders...</div>;
  }

  if (error) {
    return <div data-testid="order-grid-error" role="alert">{error}</div>;
  }

  if (isEmpty || orders.length === 0) {
    return <div data-testid="order-grid-empty">No orders found</div>;
  }

  return (
    <div data-testid="order-grid" className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {orders.map((order) => (
        <div key={order.id} className="border rounded-lg p-4 shadow" data-testid={`order-card-${order.id}`}>
          <div className="flex justify-between items-start mb-2">
            <h3 className="font-semibold text-lg">{order.orderNumber}</h3>
            <span className="text-xs px-2 py-1 bg-gray-200 rounded">{order.status}</span>
          </div>
          <p className="text-gray-600 text-sm mb-2">Customer: {order.customerId}</p>
          <p className="text-lg font-bold mb-4">
            {order.totalAmount} {order.currency}
          </p>
          <div className="flex gap-2">
            {onSelectOrder && (
              <button onClick={() => onSelectOrder(order)} className="flex-1 px-3 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
                View
              </button>
            )}
            {onEditOrder && (
              <button onClick={() => onEditOrder(order)} className="flex-1 px-3 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600">
                Edit
              </button>
            )}
            {onDeleteOrder && (
              <button
                onClick={() => onDeleteOrder(order.id)}
                className="flex-1 px-3 py-2 bg-red-500 text-white rounded hover:bg-red-600"
              >
                Delete
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
