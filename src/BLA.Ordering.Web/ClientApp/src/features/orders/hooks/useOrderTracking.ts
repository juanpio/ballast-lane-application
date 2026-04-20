/**
 * useOrderTracking - Hook for fetching order tracking events
 * TODO: Implement fetching tracking events, polling updates
 */

import { useState } from 'react';
import { OrderTrackingEvent } from '../types';

export function useOrderTracking(orderId: string | null) {
  const [trackingEvents, setTrackingEvents] = useState<OrderTrackingEvent[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // TODO: Implement fetching logic with optional polling
  // const fetchTrackingEvents = async (id: string) => { ... }

  return {
    trackingEvents,
    isLoading,
    error,
    // fetchTrackingEvents,
  };
}
