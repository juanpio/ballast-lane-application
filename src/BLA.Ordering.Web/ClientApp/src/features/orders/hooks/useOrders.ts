/**
 * useOrders - Hook for fetching paginated orders list
 * TODO: Implement fetching, pagination, error handling
 */

import { useState } from 'react';
import { OrderDto, OrdersListParams, OrdersListResponse } from '../types';

export function useOrders(initialParams: OrdersListParams = { page: 1, pageSize: 10 }) {
  const [data, setData] = useState<OrdersListResponse>({
    data: [],
    total: 0,
    page: initialParams.page,
    pageSize: initialParams.pageSize,
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // TODO: Implement fetching logic
  // const fetchOrders = async (params: OrdersListParams) => { ... }

  return {
    orders: data.data,
    total: data.total,
    page: data.page,
    pageSize: data.pageSize,
    isLoading,
    error,
    // fetchOrders,
  };
}
