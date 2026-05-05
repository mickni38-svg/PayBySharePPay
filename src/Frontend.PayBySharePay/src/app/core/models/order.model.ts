export interface Order {
  id: number;
  title: string;
  category?: string;
  message?: string;
  createdDate: Date;
  createdByParticipantId: number;
  status: OrderStatus;
  orderParticipants: OrderParticipant[];
}

export enum OrderStatus {
  Created = 0,
  Pending = 1,
  Completed = 2,
  Cancelled = 3
}

export interface OrderParticipant {
  id: number;
  orderId: number;
  participantId: number;
  participantName: string;
  participantType: number;
  status: OrderParticipantStatus;
  amount?: number;
}

export enum OrderParticipantStatus {
  Pending = 0,
  Accepted = 1,
  Declined = 2,
  Paid = 3
}

// API response DTOs – matcher Service.PayBySharePay.DTOs
export interface OrderApiDto {
  id: number;
  title: string;
  category?: string;
  message?: string;
  status: string;
  createdAt: string;
}

export interface OrderOverviewApiDto {
  orderId: number;
  title: string;
  category?: string;
  message?: string;
  status: string;
  createdAt: string;
  participants: OrderParticipantApiDto[];
  payments: PaymentApiDto[];
  messages: MessageApiDto[];
}

export interface OrderParticipantApiDto {
  participantId: number;
  name: string;
  type: string; // "Person" | "Merchant"
  status: string; // "Pending" | "Accepted" | "Declined" | "Paid"
}

export interface PaymentApiDto {
  id: number;
  participantId: number;
  participantName: string;
  amount: number;
  status: string;
  createdAt: string;
}

export interface MessageApiDto {
  id: number;
  participantId: number;
  participantName: string;
  content: string;
  createdAt: string;
}

export function mapOrderParticipantStatus(status: string): OrderParticipantStatus {
  switch (status) {
    case 'Accepted': return OrderParticipantStatus.Accepted;
    case 'Declined': return OrderParticipantStatus.Declined;
    case 'Paid':     return OrderParticipantStatus.Paid;
    default:         return OrderParticipantStatus.Pending;
  }
}

export interface CreateOrderRequest {
  title: string;
  category?: string;
  message?: string;
  participantIds: number[];
}

