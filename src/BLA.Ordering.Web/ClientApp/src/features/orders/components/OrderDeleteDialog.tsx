import type { OrderDto } from '../types';

interface OrderDeleteDialogProps {
  isOpen: boolean;
  order: OrderDto | null;
  isLoading?: boolean;
  error?: string | null;
  onConfirm?: (orderId: string) => Promise<void>;
  onCancel?: () => void;
}

export function OrderDeleteDialog({
  isOpen,
  order,
  isLoading = false,
  error = null,
  onConfirm,
  onCancel,
}: OrderDeleteDialogProps) {
  if (!isOpen || !order) {
    return null;
  }

  return (
    <div role="dialog" aria-modal="true" aria-label="Delete order confirmation" className="order-delete-overlay">
      <div className="order-delete-card">
        <h2>{`Delete order ${order.orderNumber}?`}</h2>
        <p>This action cannot be undone.</p>
        <p>{`${order.currency} ${order.totalAmount.toFixed(2)}`}</p>

        {error ? (
          <div role="alert" className="order-delete-error">
            {error}
          </div>
        ) : null}

        <div className="order-delete-actions">
          <button type="button" onClick={onCancel}>
            Cancel
          </button>
          <button
            type="button"
            onClick={async () => {
              await onConfirm?.(order.id);
            }}
            disabled={isLoading}
          >
            {isLoading ? 'Deleting...' : 'Delete Order'}
          </button>
        </div>
      </div>
    </div>
  );
}
