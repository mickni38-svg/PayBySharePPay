import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { OrderService } from '../../core/services/order.service';
import { MessageService } from '../../core/services/message.service';
import { DevService } from '../../core/services/dev.service';
import { FriendService } from '../../core/services/friend.service';
import { DirectoryEntry } from '../../core/models/directory.model';
import { computePendingSummary } from '../../core/models/order.model';

interface ActionCard {
  label: string;
  subtitle: string;
  icon: string;
  route: string;
  iconBg: string;
  accent: string;
}

interface StatusCard {
  type: 'pending' | 'allPaid' | 'activity';
  title: string;
  subtitle: string;
  orderId?: number;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit, OnDestroy {
  actionCards: ActionCard[] = [
    { label: 'Overblik',    subtitle: 'Se igangværende gruppebetalinger', route: '/orders',           accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',  icon: 'chart'     },
    { label: 'Beskeder',    subtitle: 'Se dine anmodninger',             route: '/messages',          accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)', icon: 'chat'      },
    { label: 'Deltagere',   subtitle: 'Find og tilføj venner',           route: '/find-participants', accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',  icon: 'users'     },
    { label: 'Profil',       subtitle: 'Se og rediger dine oplysninger', route: '/profile',   accent: '#FFCCFF', iconBg: 'rgba(255,204,255,0.15)', icon: 'activity'  },
  ];

  statusCards = signal<StatusCard[]>([]);
  dismissedAllPaidIds = signal<Set<number>>(new Set());
  readyToPayCount = signal<number>(0);
  persons = signal<DirectoryEntry[]>([]);
  friendCount = signal<number | null>(null);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);
  resetLoading = signal(false);
  resetMessage = signal<string | null>(null);

  private routerSub?: Subscription;

  constructor(
    readonly auth: AuthService,
    private directory: DirectoryService,
    private orderService: OrderService,
    readonly messageService: MessageService,
    private friendService: FriendService,
    private devService: DevService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.directory.search('').subscribe({
      next: (list) => this.persons.set(list.filter(e => e.type === 'Person')),
      error: () => {}
    });
    this.refreshData();

    // Reload status og unread-count ved hvert besøg på /home
    this.routerSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const nav = e as NavigationEnd;
      if (nav.urlAfterRedirects === '/home' || nav.urlAfterRedirects === '/') {
        this.refreshData();
      }
    });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  private refreshData(): void {
    const userId = this.auth.currentUserId();
    if (userId) {
      this.loadStatusCards(userId);
      this.messageService.refreshUnread(userId);
      this.friendService.getFriends(userId).subscribe({
        next: (list) => this.friendCount.set(list.length),
        error: () => this.friendCount.set(0)
      });
    }
  }

  private loadStatusCards(userId: number): void {
    this.orderService.getOrdersByParticipant(userId).subscribe({
      next: (orders) => {
        const pending = computePendingSummary(orders, userId);
        const cards: StatusCard[] = [];

        if (pending.pendingParticipantCount > 0) {
          cards.push({
            type: 'pending',
            title: `${pending.pendingParticipantCount} deltager${pending.pendingParticipantCount === 1 ? '' : 'e'} afventer`,
            subtitle: `På tværs af ${pending.affectedOrderCount} ordre`
          });
        }

        // Tilføj kort for host-ordrer hvor alle har betalt (og ikke dismissed)
        const dismissed = this.dismissedAllPaidIds();
        const allPaidOrders = orders.filter(o =>
          o.createdByParticipantId === userId &&
          o.status !== 'Completed' && o.status !== 'Cancelled' &&
          !dismissed.has(o.id) &&
          o.participants.filter(p => p.type !== 'Merchant').length > 0 &&
          o.participants.filter(p => p.type !== 'Merchant').every(p => p.status === 'Paid')
        );
        for (const o of allPaidOrders) {
          cards.push({
            type: 'allPaid',
            title: `? Alle har betalt – ${o.title}`,
            subtitle: 'Ordren er sendt til spisestedet',
            orderId: o.id
          });
        }

        this.statusCards.set(cards);

        // Tæl ordrer der venter på host-betaling
        const readyToPay = orders.filter(o =>
          o.createdByParticipantId === userId &&
          o.status === 'ReadyToPay'
        );
        this.readyToPayCount.set(readyToPay.length);
      },
      error: () => {
        this.statusCards.set([]);
      }
    });
  }

  onStatusCardClick(card: StatusCard): void {
    if (card.type === 'pending') {
      this.router.navigate(['/pending-participants']);
    } else if (card.type === 'allPaid') {
      this.router.navigate(['/orders']);
    } else {
      this.router.navigate(['/profile']);
    }
  }

  dismissAllPaidCard(event: Event, card: StatusCard): void {
    event.stopPropagation();
    if (card.orderId == null) return;
    const next = new Set(this.dismissedAllPaidIds());
    next.add(card.orderId);
    this.dismissedAllPaidIds.set(next);
    this.statusCards.set(this.statusCards().filter(c => c.orderId !== card.orderId));
  }

  devReset(): void {
    if (!confirm('Er du sikker? Dette sletter ALLE ordre og beskeder i databasen.')) return;
    this.resetLoading.set(true);
    this.resetMessage.set(null);
    this.devService.resetData().subscribe({
      next: () => {
        this.resetLoading.set(false);
        this.resetMessage.set('? Alle ordre og beskeder er slettet.');
        this.refreshData();
        setTimeout(() => this.resetMessage.set(null), 4000);
      },
      error: () => {
        this.resetLoading.set(false);
        this.resetMessage.set('? Fejl ved sletning – prøv igen.');
      }
    });
  }

  devLogin(): void {
    if (!this.selectedEmail) return;
    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.login(this.selectedEmail).subscribe({
      next: () => {
        // Hard reload sikrer at alle komponenter starter fresh med login-state
        window.location.reload();
      },
      error: () => {
        this.loginError.set('Login fejlede – prøv igen.');
        this.loginLoading.set(false);
      }
    });
  }
}
