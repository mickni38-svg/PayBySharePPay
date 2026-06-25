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
  ReadyToPay = 'ReadyToPay',
  HostApproved = 'HostApproved',
  Capturing = 'Capturing',
  Paid = 'Paid',
  PartiallyFailed = 'PartiallyFailed',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

/** Betalingsstatus fra ParticipantPayment (backend: ParticipantPaymentStatus) */
export enum ParticipantPaymentStatus {
  Created = 'Created',
  ReservationStarted = 'ReservationStarted',
  Reserved = 'Reserved',
  ReservationFailed = 'ReservationFailed',
  CapturePending = 'CapturePending',
  Captured = 'Captured',
  CaptureFailed = 'CaptureFailed',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  Refunded = 'Refunded'
}

/** Doc 06 ÃÂ§5: Danske statuslabels for betalingsstatus */
export function paymentStatusLabel(status: string): string {
  switch (status) {
    case ParticipantPaymentStatus.Created:           return 'Afventer';
    case ParticipantPaymentStatus.ReservationStarted: return 'Har ÃÂ¥bnet menukort';
    case ParticipantPaymentStatus.Reserved:          return 'Betaling reserveret';
    case ParticipantPaymentStatus.ReservationFailed: return 'Fejlet';
    case ParticipantPaymentStatus.CapturePending:    return 'Afventer Host';
    case ParticipantPaymentStatus.Captured:          return 'Betaling gennemfÃÂ¸rt';
    case ParticipantPaymentStatus.CaptureFailed:     return 'Fejlet';
    case ParticipantPaymentStatus.Cancelled:         return 'Annulleret';
    case ParticipantPaymentStatus.Expired:           return 'UdlÃÂ¸bet';
    case ParticipantPaymentStatus.Refunded:          return 'Refunderet';
    default:                                          return 'Ukendt';
  }
}

/** Doc 06 ÃÂ§5: Danske statuslabels for deltager-deltagelse */
export function participantStatusLabel(status: string): string {
  switch (status) {
    case 'Invited':         return 'Inviteret';
    case 'Accepted':        return 'Bestilling modtaget';
    case 'OrderSubmitted':  return 'Bestilling modtaget';
    case 'Paid':            return 'Betaling gennemfÃÂ¸rt';
    case 'Declined':        return 'Afvist';
    default:                return status;
  }
}

/** Dansk ordrestatus-label */
export function orderStatusLabel(status: string): string {
  switch (status) {
    case OrderStatus.Collecting:     return 'Samler bestillinger';
    case OrderStatus.ReadyToPay:     return 'Klar til betaling';
    case OrderStatus.HostApproved:   return 'Godkendt af vÃÂ¦rt';
    case OrderStatus.Capturing:      return 'GennemfÃÂ¸rer betalinger...';
    case OrderStatus.Paid:           return 'Betalt';
    case OrderStatus.PartiallyFailed: return 'Delvis fejlet';
    case OrderStatus.Cancelled:      return 'Annulleret';
    case OrderStatus.Completed:      return 'Afsluttet';
    default:                          return status;
  }
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

// API response DTOs Ã¢â¬â matcher Service.PayBySharePay.DTOs
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
  participantPayments: ParticipantPaymentApiDto[];
  messages: MessageApiDto[];
  participantOrderLines: ParticipantOrderLinesApiDto[];
}

/** Betalingsstatus pr. deltager fra ParticipantPayment-tabellen (backend: ParticipantPaymentSummaryDto) */
export interface ParticipantPaymentApiDto {
  participantPaymentId: number;
  participantId: number;
  participantName: string;
  amountMinorUnits: number;
  currency: string;
  status: string;
  providerPaymentId?: string | null;
  reservedAtUtc?: string | null;
  capturedAtUtc?: string | null;
  lastErrorCode?: string | null;
  lastErrorMessage?: string | null;
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
    // Kun 'Invited' tÃÂ¦ller som afventende Ã¢â¬â OrderSubmitted og Paid er fÃÂ¦rdige
    const pendingPs = order.participants.filter(
      p => p.type !== 'Merchant' && p.status === 'Invited'
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
        pendingReason: p.status === 'Invited' ? 'Mangler at bekrÃÂ¦fte deltagelse' : 'Mangler betaling'
      }))
    });
  }

  return {
    pendingParticipantCount: pendingOrders.reduce((s, o) => s + o.pendingCount, 0),
    affectedOrderCount: pendingOrders.length,
    orders: pendingOrders
  };
}

/** Svar fra POST /api/orders/{id}/approve */
export interface ApproveAndCaptureResult {
  allCaptured: boolean;
  orderStatus: string;
  results: ParticipantCaptureResult[];
}

export interface ParticipantCaptureResult {
  participantId: number;
  participantName: string;
  participantPaymentId: number;
  success: boolean;
  providerCaptureId?: string | null;
  errorCode?: string | null;
  errorMessage?: string | null;
}

