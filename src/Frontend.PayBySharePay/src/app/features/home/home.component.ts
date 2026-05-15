import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { OrderService } from '../../core/services/order.service';
import { DirectoryEntry } from '../../core/models/directory.model';
import { OrderSummaryApiDto } from '../../core/models/order.model';

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
    { label: 'Overblik',  subtitle: 'Se regninger',      route: '/orders',            accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',   icon: 'chart' },
    { label: 'Opret',     subtitle: 'Ny gruppebetaling', route: '/orders/create',      accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)',  icon: 'plus'  },
    { label: 'Brugere',   subtitle: 'Find personer',     route: '/find-participants',  accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',   icon: 'users' },
    { label: 'Beskeder',  subtitle: 'Dine beskeder',     route: '/messages',           accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)',  icon: 'chat'  },
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
    if (userId) {
      this.loadStatusCards(userId);
    }
  }

  private loadStatusCards(userId: number): void {
    this.orderService.getOrdersByParticipant(userId).subscribe({
      next: (orders) => {
        const cards: StatusCard[] = [];

        // Find ordre med afventende deltagere
        const pendingOrder = orders.find(o =>
          o.participants.some(p => p.status === 'Invited' && p.type !== 'Merchant')
        );

        if (pendingOrder) {
          const pendingCount = pendingOrder.participants.filter(
            p => p.status === 'Invited' && p.type !== 'Merchant'
          ).length;
          cards.push({
            type: 'pending',
            title: `${pendingCount} deltager${pendingCount === 1 ? '' : 'e'} afventer`,
            subtitle: `${pendingOrder.title} mangler svar`,
            orderId: pendingOrder.id
          });
        }

        // "Du er opdateret"-kort
        cards.push({
          type: 'updated',
          title: pendingOrder ? 'Ellers er du opdateret' : 'Du er opdateret',
          subtitle: pendingOrder ? '' : 'Ingen nye aktiviteter',
          orderId: null
        });

        this.statusCards.set(cards);
      },
      error: () => {
        this.statusCards.set([{
          type: 'updated',
          title: 'Du er opdateret',
          subtitle: 'Ingen nye aktiviteter',
          orderId: null
        }]);
      }
    });
  }

  onStatusCardClick(card: StatusCard): void {
    if (card.type === 'pending' && card.orderId !== null) {
      this.router.navigate(['/orders', card.orderId]);
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

