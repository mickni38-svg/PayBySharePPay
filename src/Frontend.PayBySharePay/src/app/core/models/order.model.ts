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
  createdByParticipantId: number;
  merchantName?: string;
  merchantAddress?: string;
  totalAmount: number;
  participants: OrderParticipantApiDto[];
  payments: PaymentApiDto[];
  messages: MessageApiDto[];
  participantOrderLines: ParticipantOrderLinesApiDto[];
}

export interface ParticipantOrderLinesApiDto {
  participantId: number;
  participantName: string;
  hasPaid: boolean;
  lines: OrderLineApiDto[];
}

export interface OrderLineApiDto {
  participantId?: number;
  lineId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
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
  totalAmount: number;
  merchantName?: string;
  participants: OrderParticipantApiDto[];
}

// Pending participants summary (beregnes client-side fra OrderSummaryApiDto)
export interface PendingParticipant {
  participantId: number;
  displayName: string;
  initials: string;
  pendingReason: string;
}

export interface PendingOrder {
  orderId: number;
  orderTitle: string;
  merchantName?: string;
  pendingCount: number;
  pendingParticipants: PendingParticipant[];
}

export interface PendingParticipantsSummary {
  pendingParticipantCount: number;
  affectedOrderCount: number;
  orders: PendingOrder[];
}

export function computePendingSummary(
  orders: OrderSummaryApiDto[],
  currentUserId: number
): PendingParticipantsSummary {
  const hostOrders = orders.filter(o => o.createdByParticipantId === currentUserId);
  const pendingOrders: PendingOrder[] = [];

  for (const order of hostOrders) {
    const pendingStatuses = ['Invited', 'Accepted'];
    const activePendingStatuses = order.status === 'Ready' ? ['Invited', 'Accepted'] : ['Invited'];

    const pendingPs = order.participants.filter(
      p => p.type !== 'Merchant' && activePendingStatuses.includes(p.status)
    );

    if (pendingPs.length === 0) continue;

    pendingOrders.push({
      orderId: order.id,
      orderTitle: order.title,
      merchantName: order.merchantName,
      pendingCount: pendingPs.length,
      pendingParticipants: pendingPs.map(p => ({
        participantId: p.participantId,
        displayName: p.name,
        initials: p.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2),
        pendingReason: p.status === 'Invited' ? 'Mangler at bekræfte deltagelse' : 'Mangler betaling'
      }))
    });
  }

  return {
    pendingParticipantCount: pendingOrders.reduce((s, o) => s + o.pendingCount, 0),
    affectedOrderCount: pendingOrders.length,
    orders: pendingOrders
  };
}

