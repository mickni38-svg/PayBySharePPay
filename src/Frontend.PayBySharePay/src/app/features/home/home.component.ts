import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { OrderService } from '../../core/services/order.service';
import { ActivityService } from '../../core/services/activity.service';
import { MessageService } from '../../core/services/message.service';
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
  type: 'pending' | 'activity';
  title: string;
  subtitle: string;
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
    { label: 'Overblik',    subtitle: 'Se regninger',       route: '/orders',           accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',  icon: 'chart'     },
    { label: 'Beskeder',    subtitle: 'Dine beskeder',      route: '/messages',          accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)', icon: 'chat'      },
    { label: 'Brugere',     subtitle: 'Find personer',      route: '/find-participants', accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',  icon: 'users'     },
    { label: 'Aktiviteter', subtitle: 'Se dine aktiviteter',route: '/activity',          accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)', icon: 'activity'  },
  ];

  statusCards = signal<StatusCard[]>([]);
  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);

  private routerSub?: Subscription;

  constructor(
    readonly auth: AuthService,
    private directory: DirectoryService,
    private orderService: OrderService,
    private activityService: ActivityService,
    readonly messageService: MessageService,
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
    }
  }

  private loadStatusCards(userId: number): void {
    forkJoin({
      orders: this.orderService.getOrdersByParticipant(userId),
      feed: this.activityService.getActivityFeed(userId)
    }).subscribe({
      next: ({ orders, feed }) => {
        const pending = computePendingSummary(orders, userId);
        const cards: StatusCard[] = [];

        if (pending.pendingParticipantCount > 0) {
          cards.push({
            type: 'pending',
            title: `${pending.pendingParticipantCount} deltager${pending.pendingParticipantCount === 1 ? '' : 'e'} afventer`,
            subtitle: `På tværs af ${pending.affectedOrderCount} ordre`
          });
        }

        this.statusCards.set(cards);
      },
      error: () => {
        this.statusCards.set([]);
      }
    });
  }

  onStatusCardClick(card: StatusCard): void {
    if (card.type === 'pending') {
      this.router.navigate(['/pending-participants']);
    } else {
      this.router.navigate(['/activity']);
    }
  }

  devLogin(): void {
    if (!this.selectedEmail) return;
    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.login(this.selectedEmail).subscribe({
      next: () => {
        this.loginLoading.set(false);
        const userId = this.auth.currentUserId();
        if (userId) this.loadStatusCards(userId);
      },
      error: () => {
        this.loginError.set('Login fejlede – prøv igen.');
        this.loginLoading.set(false);
      }
    });
  }
}
