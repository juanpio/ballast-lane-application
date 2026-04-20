/**
 * useOrderDetail - Hook for fetching a single order
 * TODO: Implement fetching, error handling, refetching
 */

import { useState } from 'react';
import { OrderDto } from '../types';

export function useOrderDetail(orderId: string | null) {
  const [order, setOrder] = useState<OrderDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

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
