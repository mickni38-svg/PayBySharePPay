import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription, forkJoin, of } from 'rxjs';
import { catchError, filter } from 'rxjs/operators';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { MessageService } from '../../core/services/message.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { Message } from '../../core/models/message.model';
import { OrderOverviewApiDto } from '../../core/models/order.model';

export type MessageFilter = 'alle' | 'bestillinger' | 'beskeder';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, EmptyStateComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  messages = signal<Message[]>([]);
  orderDetails = signal<Record<number, OrderOverviewApiDto>>({});
  isLoading = signal(true);
  activeFilter = signal<MessageFilter>('alle');

  filteredMessages = computed(() => {
    const f = this.activeFilter();
    const all = this.messages();
    if (f === 'alle') return all;
    if (f === 'bestillinger') return all.filter(m => this.getCategory(m.content) === 'bestilling');
    if (f === 'beskeder') return all.filter(m => this.getCategory(m.content) !== 'bestilling');
    return all;
  });

  private routerSub?: Subscription;

  constructor(
    private readonly messageService: MessageService,
    private readonly orderService: OrderService,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadMessages();

    this.routerSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const nav = e as NavigationEnd;
      if (nav.urlAfterRedirects.startsWith('/messages')) {
        this.loadMessages();
      }
    });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  setFilter(f: MessageFilter): void {
    this.activeFilter.set(f);
  }

  getCategory(content: string): 'bestilling' | 'besked' | 'system' {
    const lower = content.toLowerCase();
    if (
      lower.includes('bestil') ||
      lower.includes('oprettet') ||
      lower.includes('ordre') ||
      lower.includes('reservation') ||
      lower.includes('betaling') ||
      lower.includes('godkend') ||
      lower.includes('captured') ||
      lower.includes('reserveret')
    ) return 'bestilling';

    if (
      lower.includes('skrev') ||
      lower.includes('skriver') ||
      lower.includes('besked') ||
      lower.includes('tilføjet') ||
      lower.includes('invit')
    ) return 'besked';

    return 'system';
  }

  onCardClick(msg: Message): void {
    if (!msg.isRead) {
      this.messageService.markRead(msg.id).subscribe({
        next: () => {
          this.messages.update(list =>
            list.map(m => m.id === msg.id ? { ...m, isRead: true } : m)
          );
          const current = this.messageService.unreadCount();
          if (current > 0) this.messageService.unreadCount.set(current - 1);
        }
      });
    }

    const url = this.extractUrl(msg.content);
    const path = url ? this.internalPath(url) : null;
    if (path) {
      void this.router.navigateByUrl(path);
    }
  }

  extractUrl(content: string): string | null {
    const match = content.match(/https?:\/\/\S+/);
    return match ? match[0].replace(/[),.;]+$/, '') : null;
  }

  internalPath(url: string): string | null {
    try {
      const parsed = new URL(url);
      if (parsed.origin === window.location.origin) {
        return parsed.pathname + parsed.search + parsed.hash;
      }
    } catch {}
    return null;
  }

  merchantOrderUrl(msg: Message): string | null {
    const url = this.extractUrl(msg.content);
    if (!url || this.internalPath(url)) return null;

    const detail = this.orderDetails()[msg.orderId];
    if (detail && ['Completed', 'Cancelled'].includes(detail.status)) return null;

    return url;
  }

  merchantLogoUrl(msg: Message): string | null {
    return this.orderDetails()[msg.orderId]?.merchantLogoUrl ?? null;
  }

  merchantName(msg: Message): string | null {
    return this.orderDetails()[msg.orderId]?.merchantName ?? null;
  }

  messageTitle(msg: Message): string | null {
    const detail = this.orderDetails()[msg.orderId];
    if (!detail || this.getCategory(msg.content) !== 'bestilling') return null;
    return `Gruppebestilling: ${detail.title}`;
  }

  textWithoutUrl(content: string): string {
    return content
      .replace(/https?:\/\/\S+/, '')
      .replace(/bestil din mad her\s*:/i, '')
      .trim();
  }

  private loadMessages(): void {
    const userId = this.auth.currentUserId();
    if (userId == null) {
      this.isLoading.set(false);
      return;
    }

    this.isLoading.set(true);

    this.messageService.getByParticipant(userId).subscribe({
      next: (msgs) => {
        this.messages.set(msgs);
        this.loadOrderDetails(msgs);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private loadOrderDetails(messages: Message[]): void {
    const orderIds = [...new Set(
      messages
        .filter(m => m.orderId > 0 && this.getCategory(m.content) === 'bestilling')
        .map(m => m.orderId)
    )];

    if (orderIds.length === 0) {
      this.orderDetails.set({});
      return;
    }

    forkJoin(
      orderIds.map(orderId =>
        this.orderService.getOrderOverview(orderId).pipe(catchError(() => of(null)))
      )
    ).subscribe(details => {
      const byId: Record<number, OrderOverviewApiDto> = {};
      details.forEach(detail => {
        if (detail) byId[detail.orderId] = detail;
      });
      this.orderDetails.set(byId);
    });
  }
}
