// Lokal model
export interface Message {
  id: number;
  participantId: number;
  participantName: string;
  content: string;
  createdAt: Date;
}

// API request – matcher Api.PayBySharePay.DTOs.CreateMessageRequest
export interface CreateMessageRequest {
  orderId: number;
  participantId: number;
  content: string;
}

