import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Message, CreateMessageRequest } from '../models/message.model';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/api/messages`;

  /** Delt signal brugt af BottomNav, Home og Messages — ingen polling nødvendig */
  readonly unreadCount = signal(0);

  constructor(private http: HttpClient) {}

  /** Henter antal ulæste fra API og opdaterer signalet */
  refreshUnread(participantId: number): void {
    this.getUnreadCount(participantId).subscribe({
      next: (count) => this.unreadCount.set(count)
    });
  }

  /** Nulstiller badge synkront (kaldes når Messages-siden åbnes) */
  resetUnread(): void {
    this.unreadCount.set(0);
  }

  // GET /api/messages/order/{orderId}
  getMessagesByOrder(orderId: number): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/order/${orderId}`);
  }

  // GET /api/messages/by-participant/{participantId}
  getByParticipant(participantId: number): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/by-participant/${participantId}`);
  }

  // GET /api/messages/unread-count?participantId=x
  getUnreadCount(participantId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/unread-count?participantId=${participantId}`);
  }

  // POST /api/messages/mark-read?participantId=x
  markAllRead(participantId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/mark-read?participantId=${participantId}`, {});
  }

  // POST /api/messages
  createMessage(request: CreateMessageRequest): Observable<Message> {
    return this.http.post<Message>(this.apiUrl, request);
  }
}

