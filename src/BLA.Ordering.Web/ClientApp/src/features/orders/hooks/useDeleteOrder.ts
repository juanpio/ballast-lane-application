/**
 * useDeleteOrder - Hook for deleting orders
 * TODO: Implement mutation, confirmation, error handling
 */

import { useState } from 'react';

export function useDeleteOrder() {
  const [isLoading] = useState(false);
  const [error] = useState<string | null>(null);

  // TODO: Implement mutation logic with optional confirmation
  // const deleteOrder = async (orderId: string) => { ... }

  return {
    isLoading,
    error,
    // deleteOrder,
    // reset,
  };
}
