// Lokal model
export interface Message {
  id: number;
  orderId: number;
  participantId: number;
  participantName: string;
  content: string;
  createdAt: Date;
  isRead: boolean;
}

// API request – matcher Api.PayBySharePay.DTOs.CreateMessageRequest
export interface CreateMessageRequest {
  orderId: number;
  participantId: number;
  content: string;
}

