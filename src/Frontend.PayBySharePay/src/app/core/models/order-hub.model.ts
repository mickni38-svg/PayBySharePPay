export type OrderHubStatus = 'New' | 'Accepted' | 'Preparing' | 'Ready' | 'Completed';

export interface OrderHubSettings {
  enabled: boolean;
}

export interface OrderHubDeliveryAddress {
  address?: string;
  postalCode?: string;
  city?: string;
  country?: string;
}

export interface OrderHubModifier {
  id?: string;
  name: string;
  price: number;
}

export interface OrderHubOrderItem {
  sku?: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  modifiersJson?: string;
}

export interface OrderHubOrder {
  id: number;
  sourceOrderId: number;
  payNSyncOrderNumber: string;
  status: OrderHubStatus;
  paymentStatus: string;
  currency: string;
  totalAmount: number;
  paidAtUtc: string;
  updatedAtUtc: string;
  hostName: string;
  hostPhone?: string;
  note?: string;
  deliveryAddress?: OrderHubDeliveryAddress;
  items: OrderHubOrderItem[];
}
