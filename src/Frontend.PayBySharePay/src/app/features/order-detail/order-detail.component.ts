import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Order, OrderParticipant, OrderParticipantStatus, OrderStatus } from '../../core/models/order.model';
import { ParticipantType } from '../../core/models/participant.model';

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

const MOCK_ORDER: Order = {
  id: 1,
  title: 'pizza aften',
  category: 'food',
  message: 'juhuu',
  createdDate: new Date(),
  createdByParticipantId: 1,
  status: OrderStatus.Pending,
  orderParticipants: [
    { id: 1, orderId: 1, participantId: 1, participantName: 'Mads Grønlund',  participantType: ParticipantType.Person,   status: OrderParticipantStatus.Pending },
    { id: 2, orderId: 1, participantId: 2, participantName: 'Adda Algren',    participantType: ParticipantType.Person,   status: OrderParticipantStatus.Pending },
    { id: 3, orderId: 1, participantId: 3, participantName: 'Clara Thomsen',  participantType: ParticipantType.Person,   status: OrderParticipantStatus.Pending },
    { id: 4, orderId: 1, participantId: 4, participantName: 'Pizza House',    participantType: ParticipantType.Merchant, status: OrderParticipantStatus.Accepted, amount: 280 },
  ]
};

const CATEGORIES: CategoryOption[] = [
  { key: 'food',     icon: '🎵', label: 'pizza aften' },
  { key: 'sushi',    icon: '🍴', label: 'sticks and sushi' },
  { key: 'drinks',   icon: '🍺', label: 'drinks' },
  { key: 'other',    icon: '📦', label: 'andet' },
];

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="detail">

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
            @if (order()?.category === cat.key) {
              <span class="cat-chip__del" (click)="$event.stopPropagation()">🗑</span>
            }
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
    }

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
      background: #1a2a5e;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 16px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      min-height: 52px;
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

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    // Brug MOCK_ORDER – udskift med: this.orderService.getOrderById(id).subscribe(...)
    const id = Number(this.route.snapshot.paramMap.get('id') ?? 1);
    void id;
    this.order.set({ ...MOCK_ORDER });
  }

  selectCategory(key: string): void {
    this.order.update(o => o ? { ...o, category: key } : o);
  }

  getInitials(name: string): string { return toInitials(name); }
  getAvatarColor(name: string): string { return avatarColor(name); }

  statusLabel(status: OrderParticipantStatus): string {
    switch (status) {
      case OrderParticipantStatus.Pending:  return 'afventer';
      case OrderParticipantStatus.Accepted: return 'accepteret';
      case OrderParticipantStatus.Declined: return 'afvist';
      case OrderParticipantStatus.Paid:     return 'betalt';
    }
  }

  statusClass(status: OrderParticipantStatus): string {
    switch (status) {
      case OrderParticipantStatus.Pending:  return 'status-pending';
      case OrderParticipantStatus.Accepted: return 'status-accepted';
      case OrderParticipantStatus.Declined: return 'status-declined';
      case OrderParticipantStatus.Paid:     return 'status-paid';
    }
  }

  openParticipant(p: OrderParticipant): void {
    // TODO: åbn deltager-detalje / betalingsdialog
    console.log('Åbn deltager', p);
  }

  payOrder(): void {
    // TODO: kald PaymentService.registerPayment(...)
    alert('Betaling registreres – implementeres i step 11');
  }
}

