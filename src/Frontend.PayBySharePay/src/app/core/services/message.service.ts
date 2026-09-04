import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subscription, timer } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Message, CreateMessageRequest } from '../models/message.model';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/api/messages`;
  private monitorSub?: Subscription;
  private monitoredParticipantId?: number;

  /** Delt signal brugt af BottomNav, Home og Messages */
  readonly unreadCount = signal(0);

  constructor(private http: HttpClient) {}

  /** Henter antal ulæste fra API og opdaterer signalet */
  refreshUnread(participantId: number): void {
    this.getUnreadCount(participantId).subscribe({
      next: (count) => this.unreadCount.set(count)
    });
  }

  /** Nulstiller badge synkront */
  resetUnread(): void {
    this.unreadCount.set(0);
  }

  /**
   * Overvåger nye beskeder for den indloggede bruger.
   * Første kald synkroniserer kun tælleren; lyd afspilles først ved en senere stigning.
   */
  startMonitoring(participantId: number): void {
    if (this.monitoredParticipantId === participantId && this.monitorSub) return;

    this.stopMonitoring();
    this.monitoredParticipantId = participantId;

    let initialized = false;
    this.monitorSub = timer(0, 15000).pipe(
      switchMap(() => this.getUnreadCount(participantId))
    ).subscribe({
      next: (count) => {
        const previous = this.unreadCount();
        if (initialized && count > previous) {
          this.playNotificationSound();
        }
        this.unreadCount.set(count);
        initialized = true;
      },
      error: () => {
        // En midlertidig netværksfejl må ikke påvirke resten af appen.
      }
    });
  }

  stopMonitoring(): void {
    this.monitorSub?.unsubscribe();
    this.monitorSub = undefined;
    this.monitoredParticipantId = undefined;
  }

  private playNotificationSound(): void {
    try {
      const AudioContextCtor = window.AudioContext || (window as any).webkitAudioContext;
      if (!AudioContextCtor) return;

      const context: AudioContext = new AudioContextCtor();
      const oscillator = context.createOscillator();
      const gain = context.createGain();

      oscillator.type = 'sine';
      oscillator.frequency.setValueAtTime(740, context.currentTime);
      gain.gain.setValueAtTime(0.0001, context.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.16, context.currentTime + 0.01);
      gain.gain.exponentialRampToValueAtTime(0.0001, context.currentTime + 0.18);

      oscillator.connect(gain);
      gain.connect(context.destination);
      oscillator.start();
      oscillator.stop(context.currentTime + 0.2);
      oscillator.onended = () => void context.close();
    } catch {
      // Safari/iOS kan blokere lyd indtil brugerinteraktion. Beskeden modtages stadig.
    }
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

  // POST /api/messages/{id}/mark-read
  markRead(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/mark-read`, {});
  }

  // POST /api/messages
  createMessage(request: CreateMessageRequest): Observable<Message> {
    return this.http.post<Message>(this.apiUrl, request).pipe(
      tap(() => {
        // Den faktiske tæller synkroniseres af monitoreringen.
      })
    );
  }
}
