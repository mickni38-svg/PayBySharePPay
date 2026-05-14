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
  Collecting = 'Collecting',
  WaitingForPayment = 'WaitingForPayment',
  Ready = 'Ready',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
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
  Invited = 'Invited',
  Accepted = 'Accepted',
  Declined = 'Declined',
  Paid = 'Paid'
}

// API response DTOs – matcher Service.PayBySharePay.DTOs
export interface OrderApiDto {
  id: number;
  createdByParticipantId: number;
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
  type: string;
  status: string;
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
    default:         return OrderParticipantStatus.Invited;
  }
}

export interface CreateOrderRequest {
  createdByParticipantId: number;
  title: string;
  category?: string;
  message?: string;
  merchantParticipantId?: number;
  participantIds: number[];
}

export interface OrderSummaryApiDto {
  id: number;
  title: string;
  category?: string;
  status: string;
  createdAt: string;
  createdByParticipantId: number;
  participants: OrderParticipantApiDto[];
}

