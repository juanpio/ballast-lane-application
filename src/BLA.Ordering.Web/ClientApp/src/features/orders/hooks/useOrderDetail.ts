/**
 * useOrderDetail - Hook for fetching a single order
 * TODO: Implement fetching, error handling, refetching
 */

import { useState } from 'react';
import type { OrderDto } from '../types';

export function useOrderDetail(_orderId: string | null) {
  const [order] = useState<OrderDto | null>(null);
  const [isLoading] = useState(false);
  const [error] = useState<string | null>(null);

  // TODO: Implement fetching logic
  // const fetchOrder = async (id: string) => { ... }

  return {
    order,
    isLoading,
    error,
    // fetchOrder,
    // refetch,
  };
}
