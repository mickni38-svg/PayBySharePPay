import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderSummaryApiDto } from '../../core/models/order.model';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="orders">

      <header class="orders__header">
        <h1 class="orders__title">Overblik</h1>
        <button class="orders__create" (click)="goCreate()">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
            <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
          </svg>
          Ny
        </button>
      </header>

      @if (isLoading()) {
        <p class="orders__state">Henter ordrer…</p>
      } @else if (errorMessage()) {
        <p class="orders__state orders__state--error">{{ errorMessage() }}</p>
      } @else if (orders().length === 0) {
        <div class="orders__empty">
          <p>Ingen gruppebetalinger endnu.</p>
          <button class="orders__empty-btn" (click)="goCreate()">Opret den første</button>
        </div>
      } @else {

        <!-- Horizontal scroll chips (like the order tabs in screenshot) -->
        <div class="orders__scroll-row">
          @for (o of orders(); track o.id) {
            <button class="scroll-chip" [class.scroll-chip--active]="activeId() === o.id" (click)="setActive(o.id)">
              <span class="scroll-chip__icon">{{ categoryIcon(o.category) }}</span>
              <span class="scroll-chip__label">{{ o.title }}</span>
            </button>
          }
        </div>

        <!-- Active order detail card with teal glow border -->
        @if (activeOrder()) {
          <div class="order-panel">

            <!-- Participant rows -->
            <div class="order-panel__participants">
              @for (p of activeOrder()!.participants; track p.participantId) {
                <div class="participant">
                  <div class="participant__avatar" [style.background]="avatarColor(p.name)">
                    {{ initials(p.name) }}
                  </div>
                  <div class="participant__info">
                    <span class="participant__name">{{ p.name }}</span>
                    <span class="participant__sub">Id: {{ p.participantId }}</span>
                  </div>
                  <div class="participant__right">
                    <span class="status-chip" [ngClass]="statusClass(p.status)">{{ statusLabel(p.status) }}</span>
                    <span class="participant__arrow">›</span>
                  </div>
                </div>
              }
            </div>

          </div>
        }

        <!-- Betal ordre knap (fixed bottom) -->
        <div class="orders__footer">
          <button class="orders__pay-btn" (click)="openOrder(activeId()!)">Betal ordre</button>
        </div>

      }

    </div>
  `,
  styles: [`
    .orders {
      display: flex;
      flex-direction: column;
      min-height: 100%;
      background: #0a0e1a;
      color: #fff;
      font-family: -apple-system, BlinkMacSystemFont, 'SF Pro Text', 'Segoe UI', sans-serif;
      padding-bottom: 140px;
    }

    .orders__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 20px 16px 12px;
    }

    .orders__title {
      margin: 0;
      font-size: 24px;
      font-weight: 700;
    }

    .orders__create {
      display: flex;
      align-items: center;
      gap: 4px;
      background: rgba(0,200,200,0.1);
      color: #00c8c8;
      border: 1px solid rgba(0,200,200,0.3);
      border-radius: 20px;
      padding: 7px 14px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
    }

    .orders__create svg { width: 14px; height: 14px; }

    .orders__state {
      padding: 40px 20px;
      text-align: center;
      color: #555;
      font-size: 15px;
      margin: 0;
    }
    .orders__state--error { color: #ff453a; }

    .orders__empty {
      padding: 60px 20px;
      text-align: center;
      color: #555;
    }
    .orders__empty-btn {
      margin-top: 16px;
      background: #1a2a5e;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 12px 24px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
    }

    /* Horizontal scroll tabs — matches screenshot order chips */
    .orders__scroll-row {
      display: flex;
      gap: 10px;
      padding: 0 14px 14px;
      overflow-x: auto;
      scrollbar-width: none;
    }
    .orders__scroll-row::-webkit-scrollbar { display: none; }

    .scroll-chip {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 14px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.15);
      background: rgba(255,255,255,0.04);
      color: #ccc;
      font-size: 13px;
      font-weight: 500;
      white-space: nowrap;
      cursor: pointer;
      flex-shrink: 0;
      transition: all 0.15s;
    }

    .scroll-chip--active {
      border-color: #2ecc71;
      background: rgba(46,204,113,0.12);
      color: #fff;
    }

    .scroll-chip__icon { font-size: 16px; }

    /* Main order panel with teal glow border — matches screenshot 1 */
    .order-panel {
      margin: 0 12px;
      border: 1.5px solid rgba(0, 200, 200, 0.5);
      border-radius: 18px;
      background: #0d1220;
      box-shadow: 0 0 18px rgba(0, 200, 200, 0.12), inset 0 0 10px rgba(0,0,0,0.3);
      overflow: hidden;
    }

    .order-panel__message {
      padding: 12px 16px;
      background: rgba(255,255,255,0.03);
      border-bottom: 1px solid rgba(255,255,255,0.06);
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .order-panel__message-label { font-size: 11px; color: #6b7280; }
    .order-panel__message-text { font-size: 14px; color: #d1d5db; }

    /* Participant rows */
    .order-panel__participants { display: flex; flex-direction: column; }

    .participant {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 13px 16px;
      border-bottom: 1px solid rgba(255,255,255,0.05);
      cursor: pointer;
      min-height: 58px;
      transition: background 0.12s;
    }
    .participant:last-child { border-bottom: none; }
    .participant:active { background: rgba(255,255,255,0.03); }

    .participant__avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 13px;
      font-weight: 700;
      color: #fff;
      flex-shrink: 0;
    }

    .participant__info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 1px;
      overflow: hidden;
    }
    .participant__name { font-size: 15px; font-weight: 600; color: #fff; }
    .participant__sub { font-size: 12px; color: #6b7280; }

    .participant__right {
      display: flex;
      align-items: center;
      gap: 6px;
      flex-shrink: 0;
    }

    .status-chip {
      font-size: 12px;
      font-weight: 500;
      padding: 3px 10px;
      border-radius: 20px;
    }
    .chip--collecting   { background: rgba(59,130,246,0.15); color: #60a5fa; }
    .chip--waiting      { background: rgba(234,179,8,0.15);  color: #fbbf24; }
    .chip--ready        { background: rgba(52,211,153,0.15); color: #34d399; }
    .chip--completed    { background: rgba(156,163,175,0.1); color: #9ca3af; }
    .chip--cancelled    { background: rgba(248,113,113,0.1); color: #f87171; }
    .status-pending     { background: rgba(156,163,175,0.1); color: #9ca3af; }
    .status-accepted    { background: rgba(52,211,153,0.12); color: #34d399; }
    .status-paid        { background: rgba(96,165,250,0.12); color: #60a5fa; }
    .status-declined    { background: rgba(248,113,113,0.1); color: #f87171; }

    .participant__arrow { color: #4b5563; font-size: 20px; }

    /* Fixed bottom pay button */
    .orders__footer {
      position: fixed;
      bottom: 68px;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 390px;
      padding: 10px 16px 12px;
      background: linear-gradient(to top, #0a0e1a 75%, transparent);
    }

    .orders__pay-btn {
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
    .orders__pay-btn:active { opacity: 0.8; }
  `]
})
export class OrdersComponent implements OnInit {

  orders = signal<OrderSummaryApiDto[]>([]);
  activeId = signal<number | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  activeOrder = computed(() => this.orders().find(o => o.id === this.activeId()) ?? null);

  private readonly AVATAR_COLORS = [
    '#7c5cbf','#2e7d32','#1565c0','#ad1457',
    '#00838f','#558b2f','#4527a0','#6d4c41'
  ];

  constructor(private orderService: OrderService, private router: Router, private auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.orderService.getOrdersByParticipant(this.auth.currentUserId() ?? 0).subscribe({
      next: (list) => {
        this.orders.set(list);
        if (list.length > 0) this.activeId.set(list[0].id);
        this.isLoading.set(false);
      },
      error: () => { this.errorMessage.set('Kunne ikke hente ordrer.'); this.isLoading.set(false); }
    });
  }

  setActive(id: number): void { this.activeId.set(id); }
  goCreate(): void { this.router.navigate(['/orders/create']); }
  openOrder(id: number): void { this.router.navigate(['/orders', id]); }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Invited: 'afventer', Accepted: 'accepteret', Declined: 'afvist', Paid: 'betalt',
      Collecting: 'indsamler', WaitingForPayment: 'afventer betaling', Ready: 'klar',
      Completed: 'afsluttet', Cancelled: 'annulleret'
    };
    return map[s] ?? s.toLowerCase();
  }

  statusClass(s: string): string {
    const map: Record<string, string> = {
      Invited: 'status-pending', Accepted: 'status-accepted', Paid: 'status-paid', Declined: 'status-declined',
      Collecting: 'chip--collecting', WaitingForPayment: 'chip--waiting', Ready: 'chip--ready',
      Completed: 'chip--completed', Cancelled: 'chip--cancelled'
    };
    return map[s] ?? '';
  }

  categoryIcon(cat?: string): string {
    const map: Record<string, string> = { sushi: '🍣', pizza: '🍕', burger: '🍔', drinks: '🍺', other: '📦' };
    return map[cat ?? ''] ?? '🍴';
  }

  initials(name: string): string {
    return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
  }

  avatarColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
    return this.AVATAR_COLORS[Math.abs(hash) % this.AVATAR_COLORS.length];
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('da-DK', { day: 'numeric', month: 'short' });
  }
}
