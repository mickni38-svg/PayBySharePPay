import { Component, OnInit, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { OrderService } from '../../core/services/order.service';
import { ActivityService } from '../../core/services/activity.service';
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
export class HomeComponent implements OnInit {
  actionCards: ActionCard[] = [
    { label: 'Overblik',  subtitle: 'Se regninger',      route: '/orders',           accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',  icon: 'chart' },
    { label: 'Opret',     subtitle: 'Ny gruppebetaling', route: '/orders/create',     accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)', icon: 'plus'  },
    { label: 'Brugere',   subtitle: 'Find personer',     route: '/find-participants', accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',  icon: 'users' },
    { label: 'Beskeder',  subtitle: 'Dine beskeder',     route: '/messages',          accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)', icon: 'chat'  },
  ];

  statusCards = signal<StatusCard[]>([]);
  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);

  constructor(
    readonly auth: AuthService,
    private directory: DirectoryService,
    private orderService: OrderService,
    private activityService: ActivityService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.directory.search('').subscribe({
      next: (list) => this.persons.set(list.filter(e => e.type === 'Person')),
      error: () => {}
    });
    const userId = this.auth.currentUserId();
    if (userId) this.loadStatusCards(userId);
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

        if (feed.unreadCount > 0) {
          const preview = feed.items.slice(0, 2).map(i => i.title).join(' · ');
          cards.push({
            type: 'activity',
            title: `${feed.unreadCount} ny${feed.unreadCount === 1 ? '' : 'e'} aktivitet${feed.unreadCount === 1 ? '' : 'er'}`,
            subtitle: preview.length > 60 ? preview.slice(0, 57) + '…' : preview
          });
        } else {
          cards.push({
            type: 'activity',
            title: 'Seneste aktivitet',
            subtitle: 'Ingen nye aktiviteter'
          });
        }

        this.statusCards.set(cards);
      },
      error: () => {
        this.statusCards.set([{ type: 'activity', title: 'Seneste aktivitet', subtitle: 'Ingen nye aktiviteter' }]);
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

interface ActionCard {
  label: string;
  subtitle: string;
  icon: string;
  route: string;
  iconBg: string;
  accent: string;
}

interface StatusCard {
  type: 'pending' | 'updated';
  title: string;
  subtitle: string;
  orderId: number | null;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  actionCards: ActionCard[] = [
    { label: 'Overblik',  subtitle: 'Se regninger',      route: '/orders',           accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',  icon: 'chart' },
    { label: 'Opret',     subtitle: 'Ny gruppebetaling', route: '/orders/create',     accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)', icon: 'plus'  },
    { label: 'Brugere',   subtitle: 'Find personer',     route: '/find-participants', accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',  icon: 'users' },
    { label: 'Beskeder',  subtitle: 'Dine beskeder',     route: '/messages',          accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)', icon: 'chat'  },
  ];

  statusCards = signal<StatusCard[]>([]);
  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);

  constructor(
    readonly auth: AuthService,
    private directory: DirectoryService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.directory.search('').subscribe({
      next: (list) => this.persons.set(list.filter(e => e.type === 'Person')),
      error: () => {}
    });
    const userId = this.auth.currentUserId();
    if (userId) this.loadStatusCards(userId);
  }

  private loadStatusCards(userId: number): void {
    this.orderService.getOrdersByParticipant(userId).subscribe({
      next: (orders) => {
        const summary = computePendingSummary(orders, userId);
        const cards: StatusCard[] = [];
        if (summary.pendingParticipantCount > 0) {
          cards.push({
            type: 'pending',
            title: `${summary.pendingParticipantCount} deltager${summary.pendingParticipantCount === 1 ? '' : 'e'} afventer`,
            subtitle: `På tværs af ${summary.affectedOrderCount} ordre`,
            orderId: null
          });
        }
        cards.push({
          type: 'updated',
          title: summary.pendingParticipantCount > 0 ? 'Ellers er du opdateret' : 'Du er opdateret',
          subtitle: summary.pendingParticipantCount > 0 ? '' : 'Ingen afventende handlinger',
          orderId: null
        });
        this.statusCards.set(cards);
      },
      error: () => {
        this.statusCards.set([{ type: 'updated', title: 'Du er opdateret', subtitle: 'Ingen nye aktiviteter', orderId: null }]);
      }
    });
  }

  onStatusCardClick(card: StatusCard): void {
    if (card.type === 'pending') {
      this.router.navigate(['/pending-participants']);
    } else {
      this.router.navigate(['/messages']);
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
