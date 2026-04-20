import type { OrderDto, OrderTrackingEvent, OrderStatus } from '../types';

interface OrderTrackingProps {
  order: OrderDto | null;
  trackingEvents?: OrderTrackingEvent[];
  isLoading?: boolean;
  error?: string | null;
  onClose?: () => void;
}

/**
 * OrderTracking component displays order tracking timeline
 * Shows order status progression with timestamps and events
 */
export function OrderTracking({
  order,
  trackingEvents = [],
  isLoading = false,
  error = null,
  onClose,
}: OrderTrackingProps) {
  if (isLoading) {
    return <div data-testid="order-tracking-loading">Loading tracking information...</div>;
  }

  if (error) {
    return <div data-testid="order-tracking-error" role="alert">{error}</div>;
  }

  if (!order) {
    return <div data-testid="order-tracking-empty">No order selected</div>;
  }

  const statusSequence: OrderStatus[] = ['pending', 'confirmed', 'shipped', 'delivered'];
  const currentStatusIndex = statusSequence.indexOf(order.status);

  return (
    <div data-testid="order-tracking" className="w-full max-w-2xl mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold">Order #{order.orderNumber}</h2>
        {onClose && (
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700 text-2xl">
            ×
          </button>
        )}
      </div>

      {/* Status Badge */}
      <div className="mb-6 text-center">
        <span className="px-4 py-2 bg-blue-100 text-blue-800 rounded-full text-lg font-semibold">
          Current Status: {order.status.toUpperCase()}
        </span>
      </div>

      {/* Timeline */}
      <div data-testid="order-tracking-timeline" className="relative">
        {statusSequence.map((status, index) => (
          <div key={status} className="flex items-start mb-6">
            {/* Timeline dot and line */}
            <div className="flex flex-col items-center mr-4">
              <div
                className={`w-4 h-4 rounded-full border-2 ${
                  index <= currentStatusIndex
                    ? 'bg-blue-500 border-blue-500'
                    : 'bg-white border-gray-300'
                }`}
                data-testid={`timeline-dot-${status}`}
              />
              {index < statusSequence.length - 1 && (
                <div
                  className={`w-1 h-8 ${
                    index < currentStatusIndex ? 'bg-blue-500' : 'bg-gray-300'
                  }`}
                  data-testid={`timeline-line-${status}`}
                />
              )}
            </div>

            {/* Status text */}
            <div>
              <h4 className={`font-semibold capitalize ${index <= currentStatusIndex ? 'text-blue-600' : 'text-gray-400'}`}>
                {status}
              </h4>
              {trackingEvents
                .filter((event) => event.status === status)
                .map((event) => (
                  <p key={event.id} className="text-sm text-gray-600">
                    {new Date(event.timestamp).toLocaleString()}
                  </p>
                ))}
            </div>
          </div>
        ))}
      </div>

      {/* Order Details */}
      <div className="mt-8 pt-6 border-t border-gray-300">
        <h3 className="text-lg font-bold mb-4">Order Details</h3>
        <div className="space-y-2">
          <p>
            <span className="font-semibold">Total Amount:</span> {order.totalAmount} {order.currency}
          </p>
          <p>
            <span className="font-semibold">Items:</span> {order.items.length}
          </p>
          <p>
            <span className="font-semibold">Created:</span> {new Date(order.createdAt).toLocaleString()}
          </p>
          <p>
            <span className="font-semibold">Last Updated:</span> {new Date(order.updatedAt).toLocaleString()}
          </p>
        </div>
      </div>
    </div>
  );
}
