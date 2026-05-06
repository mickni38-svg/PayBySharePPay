import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Order, OrderParticipant, OrderParticipantStatus, OrderStatus, OrderOverviewApiDto, mapOrderParticipantStatus } from '../../core/models/order.model';
import { ParticipantType } from '../../core/models/participant.model';
import { OrderService } from '../../core/services/order.service';

interface CategoryOption {
  icon: string;
  label: string;
  key: string;
}

const AVATAR_COLORS = [
  '#7c5cbf', '#2e7d32', '#1565c0', '#ad1457',
  '#00838f', '#558b2f', '#4527a0', '#6d4c41'
];

function avatarColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

function toInitials(name: string): string {
  return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
}

const CATEGORIES: CategoryOption[] = [
  { key: 'sushi',   icon: '🍣', label: 'Sushi' },
  { key: 'pizza',   icon: '🍕', label: 'Pizza' },
  { key: 'burger',  icon: '🍔', label: 'Burger' },
  { key: 'drinks',  icon: '🍺', label: 'Drinks' },
  { key: 'other',   icon: '📦', label: 'Andet' },
];

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="detail">

      <!-- Header -->
      <header class="detail__header">
        <button class="detail__back" (click)="goBack()">‹</button>
        <h1 class="detail__title">{{ order()?.title || 'Ordre' }}</h1>
      </header>

      @if (isLoading()) {
        <p class="detail__state">Henter ordre…</p>
      } @else if (errorMessage()) {
        <p class="detail__state detail__state--error">{{ errorMessage() }}</p>
      } @else {

      <!-- Kategori-tabs -->
      <div class="detail__cats">
        @for (cat of categories; track cat.key) {
          <button
            class="cat-chip"
            [class.cat-chip--active]="order()?.category === cat.key"
            (click)="selectCategory(cat.key)"
          >
            <span>{{ cat.icon }}</span>
            <span>{{ cat.label }}</span>
          </button>
        }
      </div>

      <!-- Besked -->
      <div class="detail__section">
        <div class="detail__label">Besked</div>
        <div class="detail__message">{{ order()?.message || '–' }}</div>
      </div>

      <!-- Deltagerliste -->
      <div class="detail__participants">
        @for (p of order()?.orderParticipants ?? []; track p.id) {
          <div class="participant" (click)="openParticipant(p)">
            <div class="participant__avatar" [style.background]="getAvatarColor(p.participantName)">
              {{ getInitials(p.participantName) }}
            </div>
            <div class="participant__info">
              <span class="participant__name">{{ p.participantName }}</span>
              <span class="participant__sub">
                @if (p.participantType === ParticipantType.Merchant) {
                  <span class="badge--merchant">Merchant</span>
                } @else {
                  <span>{{ '@' + p.participantName.split(' ')[0].toLowerCase() }}</span>
                }
              </span>
            </div>
            <div class="participant__right">
              <span class="status-chip" [ngClass]="statusClass(p.status)">{{ statusLabel(p.status) }}</span>
              <span class="participant__arrow">›</span>
            </div>
          </div>
        }
      </div>

      <!-- Betal-knap -->
      <div class="detail__footer">
        <button class="detail__pay-btn" (click)="payOrder()">Betal ordre</button>
      </div>

      } <!-- end @else -->

    </div>
  `,
  styles: [`
    .detail {
      display: flex;
      flex-direction: column;
      min-height: 100%;
      background: #0a0e1a;
      color: #fff;
      padding-bottom: 140px;
      font-family: -apple-system, BlinkMacSystemFont, 'SF Pro Text', 'Segoe UI', sans-serif;
    }

    .detail__header {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 16px 16px 8px;
    }

    .detail__back {
      background: transparent;
      border: none;
      color: #60a5fa;
      font-size: 28px;
      line-height: 1;
      cursor: pointer;
      padding: 0 6px 0 0;
      min-width: 32px;
      min-height: 44px;
    }

    .detail__title {
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .detail__state {
      padding: 40px 20px;
      text-align: center;
      color: #555;
      font-size: 15px;
      margin: 0;
    }

    .detail__state--error { color: #ff453a; }


    /* Kategori-chips */
    .detail__cats {
      display: flex;
      gap: 10px;
      padding: 16px 16px 8px;
      overflow-x: auto;
      scrollbar-width: none;
    }

    .detail__cats::-webkit-scrollbar { display: none; }

    .cat-chip {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 10px 14px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.15);
      background: transparent;
      color: #ccc;
      font-size: 14px;
      white-space: nowrap;
      cursor: pointer;
      transition: all 0.15s;
      min-height: 44px;
    }

    .cat-chip--active {
      border-color: #2ecc71;
      color: #fff;
      background: rgba(46,204,113,0.1);
    }

    .cat-chip__del {
      font-size: 12px;
      opacity: 0.7;
    }

    /* Besked */
    .detail__section {
      margin: 8px 16px;
      background: #111827;
      border-radius: 12px;
      padding: 12px 14px;
    }

    .detail__label {
      font-size: 12px;
      color: #6b7280;
      margin-bottom: 4px;
    }

    .detail__message {
      font-size: 15px;
      color: #e5e7eb;
    }

    /* Deltagerliste */
    .detail__participants {
      margin-top: 8px;
    }

    .participant {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 14px 16px;
      border-bottom: 1px solid rgba(255,255,255,0.06);
      cursor: pointer;
      min-height: 60px;
      transition: background 0.15s;
    }

    .participant:active {
      background: rgba(255,255,255,0.03);
    }

    .participant__avatar {
      width: 44px;
      height: 44px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 15px;
      font-weight: 700;
      color: #fff;
      flex-shrink: 0;
    }

    .participant__info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 2px;
      overflow: hidden;
    }

    .participant__name {
      font-size: 15px;
      font-weight: 600;
      color: #fff;
    }

    .participant__sub {
      font-size: 13px;
      color: #6b7280;
    }

    .badge--merchant {
      background: rgba(234,179,8,0.2);
      color: #fbbf24;
      border: 1px solid rgba(234,179,8,0.4);
      font-size: 11px;
      font-weight: 600;
      padding: 2px 7px;
      border-radius: 20px;
    }

    .participant__right {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-shrink: 0;
    }

    .status-chip {
      font-size: 13px;
      font-weight: 500;
      padding: 3px 10px;
      border-radius: 20px;
    }

    .status-pending  { color: #9ca3af; background: rgba(156,163,175,0.1); }
    .status-accepted { color: #34d399; background: rgba(52,211,153,0.1); }
    .status-declined { color: #f87171; background: rgba(248,113,113,0.1); }
    .status-paid     { color: #60a5fa; background: rgba(96,165,250,0.1); }

    .participant__arrow {
      color: #6b7280;
      font-size: 20px;
      line-height: 1;
    }

    /* Betal-knap */
    .detail__footer {
      position: fixed;
      bottom: 68px;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 390px;
      padding: 12px 16px;
      background: linear-gradient(to top, #0a0e1a 80%, transparent);
    }

    .detail__pay-btn {
      width: 100%;
      background: linear-gradient(135deg, #0d2a4a 0%, #1a3a6e 100%);
      color: #fff;
      border: 1px solid rgba(0,120,200,0.4);
      border-radius: 14px;
      padding: 16px;
      font-size: 17px;
      font-weight: 700;
      cursor: pointer;
      min-height: 54px;
      letter-spacing: 0.3px;
      transition: opacity 0.2s;
    }

    .detail__pay-btn:active {
      opacity: 0.8;
    }
  `]
})
export class OrderDetailComponent implements OnInit {
  readonly ParticipantType = ParticipantType;
  readonly OrderParticipantStatus = OrderParticipantStatus;

  categories = CATEGORIES;
  order = signal<Order | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/orders']); return; }

    this.orderService.getOrderOverview(id).subscribe({
      next: (dto: OrderOverviewApiDto) => {
        this.order.set({
          id: dto.orderId,
          title: dto.title,
          category: dto.category,
          message: dto.message,
          createdDate: new Date(dto.createdAt),
          createdByParticipantId: 0,
          status: dto.status as OrderStatus,
          orderParticipants: dto.participants.map((p, i) => ({
            id: i + 1,
            orderId: dto.orderId,
            participantId: p.participantId,
            participantName: p.name,
            participantType: p.type === 'Merchant' ? ParticipantType.Merchant : ParticipantType.Person,
            status: mapOrderParticipantStatus(p.status)
          }))
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke hente ordre.');
        this.isLoading.set(false);
      }
    });
  }

  selectCategory(key: string): void {
    this.order.update(o => o ? { ...o, category: key } : o);
  }

  getInitials(name: string): string { return toInitials(name); }
  getAvatarColor(name: string): string { return avatarColor(name); }

  statusLabel(status: OrderParticipantStatus): string {
    switch (status) {
      case OrderParticipantStatus.Invited:  return 'inviteret';
      case OrderParticipantStatus.Accepted: return 'accepteret';
      case OrderParticipantStatus.Declined: return 'afvist';
      case OrderParticipantStatus.Paid:     return 'betalt';
      default: return status;
    }
  }

  statusClass(status: OrderParticipantStatus): string {
    switch (status) {
      case OrderParticipantStatus.Invited:  return 'status-pending';
      case OrderParticipantStatus.Accepted: return 'status-accepted';
      case OrderParticipantStatus.Declined: return 'status-declined';
      case OrderParticipantStatus.Paid:     return 'status-paid';
      default: return '';
    }
  }

  openParticipant(p: OrderParticipant): void {
    console.log('Åbn deltager', p);
  }

  payOrder(): void {
    alert('Betaling registreres – implementeres i næste step');
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }
}
