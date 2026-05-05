// Lokal model
export interface Payment {
  id: number;
  orderId: number;
  participantId: number;
  participantName: string;
  amount: number;
  status: string;
  createdAt: Date;
}

// API request – matcher Api.PayBySharePay.DTOs.RegisterPaymentRequest
export interface RegisterPaymentRequest {
  orderId: number;
  participantId: number;
  amount: number;
}

