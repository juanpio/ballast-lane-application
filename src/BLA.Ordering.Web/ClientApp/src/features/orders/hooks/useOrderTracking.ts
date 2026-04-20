/**
 * useOrderTracking - Hook for fetching order tracking events
 * TODO: Implement fetching tracking events, polling updates
 */

import { useState } from 'react';
import type { OrderTrackingEvent } from '../types';

export function useOrderTracking(_orderId: string | null) {
  const [trackingEvents] = useState<OrderTrackingEvent[]>([]);
  const [isLoading] = useState(false);
  const [error] = useState<string | null>(null);

  // TODO: Implement fetching logic with optional polling
  // const fetchTrackingEvents = async (id: string) => { ... }

  return {
    trackingEvents,
    isLoading,
    error,
    // fetchTrackingEvents,
  };
}
