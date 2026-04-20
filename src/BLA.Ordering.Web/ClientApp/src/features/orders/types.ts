/**
 * Order domain types shared across components
 */

export interface OrderDto {
  id: string;
  customerId: string;
  orderNumber: string;
  status: OrderStatus;
  totalAmount: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  id: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export type OrderStatus = 'pending' | 'confirmed' | 'shipped' | 'delivered' | 'cancelled';

export interface CreateOrderRequest {
  customerId: string;
  items: CreateOrderItemRequest[];
}

export interface CreateOrderItemRequest {
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface UpdateOrderRequest {
  id: string;
  status?: OrderStatus;
  items?: CreateOrderItemRequest[];
}

export interface OrdersListParams {
  page: number;
  pageSize: number;
  status?: OrderStatus;
}

export interface OrdersListResponse {
  data: OrderDto[];
  total: number;
  page: number;
  pageSize: number;
}

export interface OrderTrackingEvent {
  id: string;
  timestamp: string;
  status: OrderStatus;
  description: string;
}
