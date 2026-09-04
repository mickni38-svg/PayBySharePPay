import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderOverviewApiDto, OrderParticipantApiDto, OrderSummaryApiDto, ParticipantOrderLinesApiDto } from '../../core/models/order.model';

interface OrderCardVM {
  id: number;
  title: string;
  category?: string;
  status: string;
  createdAt: string;
  createdByParticipantId: number;
  isHost: boolean;
  totalAmount: number;
  merchantName?: string;
  merchantAddress?: string;
  participantCount: number;
  paidParticipantCount: number;
  canPayTotalOrder: boolean;
  allPaid: boolean;
  canShowOrderLines: boolean;
  participants: OrderParticipantApiDto[];
  participantOrderLines: ParticipantOrderLinesApiDto[];
  detailsLoaded: boolean;
  /** Deltagerens eget beløb (null = ingen bestilling endnu) */
  myOwnAmount: number | null;
  /** Sum af betalte deltageres ordrelinjer */
  paidAmount: number;
  /** Sum af alle ordrelinjer uanset betalingsstatus */
  totalOrderedAmount: number;
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit, OnDestroy {

  allOrders = signal<OrderSummaryApiDto[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  activeTab = signal<'active' | 'completed'>('active');
  filterPending = signal(false);
  payingOrderId = signal<number | null>(null);
  payError = signal<string | null>(null);

  private _detailsCache = signal<Map<number, OrderOverviewApiDto>>(new Map());
  private _expandedIds = signal<Set<number>>(new Set());
  private _loadingIds = signal<Set<number>>(new Set());

  private readonly AVATAR_COLORS = [
    '#7c5cbf','#2e7d32','#1565c0','#ad1457',
    '#00838f','#558b2f','#4527a0','#6d4c41'
  ];

  private routerSub?: Subscription;

  constructor(
    private orderService: OrderService,
    private router: Router,
    private auth: AuthService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.load();
    this.routerSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const nav = e as NavigationEnd;
      if (nav.urlAfterRedirects === '/orders' || nav.urlAfterRedirects.startsWith('/orders?')) {
        this.load();
      }
    });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  private load(): void {
    this.isLoading.set(true);
    const filter = this.route.snapshot.queryParamMap.get('filter');
    if (filter === 'pending-payments') {
      this.filterPending.set(true);
    }

    this.orderService.getOrdersByParticipant(this.auth.currentUserId() ?? 0).subscribe({
      next: (list) => {
        this.allOrders.set(list);

        // UC-18: Alle ordre-accordions starter lukkede på load/navigation/refresh.
        this._expandedIds.set(new Set());

        // Detaljer preloades fortsat, så eksisterende betalingsbeløb og statuslogik bevares.
        list.forEach(o => {
          if (!this._detailsCache().has(o.id)) {
            this.loadDetails(o.id);
          }
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke hente ordrer.');
        this.isLoading.set(false);
      }
    });
  }

  private buildVM(o: OrderSummaryApiDto): OrderCardVM {
    const userId = this.auth.currentUserId() ?? 0;
    const isHost = o.createdByParticipantId === userId;
    const nonMerchant = o.participants.filter(p => p.type !== 'Merchant');

    // Eksisterende domæneregel: OrderSubmitted eller Paid tæller som "har bestilt".
    const submittedStatuses = ['OrderSubmitted', 'Paid'];
    const paidCount = nonMerchant.filter(p => submittedStatuses.includes(p.status)).length;
    const allPaid = o.status === 'ReadyToPay' || o.status === 'Completed' ||
      (nonMerchant.length > 0 && nonMerchant.every(p => submittedStatuses.includes(p.status)));

    const cached = this._detailsCache().get(o.id);
    const anyoneHasLines = cached?.participantOrderLines.some(g => g.lines.length > 0) ?? false;
    const myLines = cached?.participantOrderLines.find(g => g.participantId === userId);
    const canShow = anyoneHasLines || (myLines?.lines?.length ?? 0) > 0;
    const visibleLines = cached?.participantOrderLines.filter(g => g.lines.length > 0) ?? [];

    const myOwnLines = cached?.participantOrderLines.find(g => g.participantId === userId);
    const myOwnAmount = (myOwnLines?.lines?.length ?? 0) > 0
      ? myOwnLines!.lines.reduce((sum, l) => sum + l.lineTotal, 0)
      : null;

    const paidAmount = cached?.participantOrderLines
      .filter(g => g.hasPaid)
      .reduce((sum, g) => sum + g.lines.reduce((s, l) => s + l.lineTotal, 0), 0) ?? 0;

    const totalOrderedAmount = cached?.participantOrderLines
      .reduce((sum, g) => sum + g.lines.reduce((s, l) => s + l.lineTotal, 0), 0) ?? 0;

    return {
      id: o.id,
      title: o.title,
      category: o.category,
      status: o.status,
      createdAt: o.createdAt,
      createdByParticipantId: o.createdByParticipantId,
      isHost,
      totalAmount: cached?.totalAmount ?? o.totalAmount,
      merchantName: cached?.merchantName ?? o.merchantName,
      merchantAddress: cached?.merchantAddress,
      participantCount: nonMerchant.length,
      paidParticipantCount: paidCount,
      canPayTotalOrder: isHost,
      allPaid,
      canShowOrderLines: canShow,
      participants: nonMerchant,
      participantOrderLines: visibleLines,
      detailsLoaded: !!cached,
      myOwnAmount,
      paidAmount,
      totalOrderedAmount
    };
  }

  hostOrders = computed(() => {
    this._detailsCache();
    const userId = this.auth.currentUserId() ?? 0;
    let list = this.allOrders()
      .filter(o => o.createdByParticipantId === userId)
      .map(o => this.buildVM(o));
    if (this.filterPending()) {
      list = list.filter(vm => vm.canPayTotalOrder || !vm.canShowOrderLines);
    }
    return list;
  });

  participantOrders = computed(() => {
    this._detailsCache();
    const userId = this.auth.currentUserId() ?? 0;
    let list = this.allOrders()
      .filter(o => o.createdByParticipantId !== userId &&
        o.participants.some(p => p.participantId === userId))
      .map(o => this.buildVM(o));
    if (this.filterPending()) {
      list = list.filter(vm => !vm.allPaid);
    }
    return list;
  });

  readonly COMPLETED_STATUSES = ['Completed', 'Cancelled'];

  allVMs = computed(() => {
    this._detailsCache();
    return [...this.hostOrders(), ...this.participantOrders()];
  });

  inProgressOrders = computed(() =>
    this.allVMs().filter(vm => !this.COMPLETED_STATUSES.includes(vm.status))
  );

  completedOrders = computed(() =>
    this.allVMs().filter(vm => this.COMPLETED_STATUSES.includes(vm.status))
  );

  activeOrders = computed(() =>
    this.activeTab() === 'active' ? this.inProgressOrders() : this.completedOrders()
  );

  setTab(tab: 'active' | 'completed'): void {
    this.activeTab.set(tab);
  }

  clearFilter(): void {
    this.filterPending.set(false);
  }

  toggleExpand(id: number): void {
    const current = this._expandedIds();
    const next = new Set(current);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
      if (!this._detailsCache().has(id)) {
        this.loadDetails(id);
      }
    }
    this._expandedIds.set(next);
  }

  private loadDetails(id: number): void {
    const loading = new Set(this._loadingIds());
    loading.add(id);
    this._loadingIds.set(loading);

    this.orderService.getOrderOverview(id).subscribe({
      next: (overview) => {
        const cache = new Map(this._detailsCache());
        cache.set(id, overview);
        this._detailsCache.set(cache);

        const l = new Set(this._loadingIds());
        l.delete(id);
        this._loadingIds.set(l);
      },
      error: () => {
        const l = new Set(this._loadingIds());
        l.delete(id);
        this._loadingIds.set(l);
      }
    });
  }

  isExpanded(id: number): boolean {
    return this._expandedIds().has(id);
  }

  isLoadingDetails(id: number): boolean {
    return this._loadingIds().has(id);
  }

  payOrder(vm: { id: number; totalOrderedAmount: number }): void {
    const userId = this.auth.currentUserId();
    if (!userId) return;

    this.payingOrderId.set(vm.id);
    this.payError.set(null);

    this.orderService.payOrder(vm.id, userId, vm.totalOrderedAmount).subscribe({
      next: () => {
        this.payingOrderId.set(null);
        this.activeTab.set('completed');
        this.load();
      },
      error: (err) => {
        this.payingOrderId.set(null);
        this.payError.set(
          err.status === 402 ? 'Betaling afvist — prøv igen.' :
          err.status === 403 ? 'Kun værten kan betale.' :
          'Noget gik galt under betalingen.'
        );
      }
    });
  }

  goCreate(): void { this.router.navigate(['/orders/create']); }

  categoryIcon(cat?: string): string {
    const map: Record<string, string> = {
      sushi: '🍣', pizza: '🍕', burger: '🍔', drinks: '🍺',
      tacos: '🌮', ramen: '🍜', kebab: '🥙', chicken: '🍗',
      salad: '🥗', dessert: '🍰', coffee: '☕', other: '📦'
    };
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

  isCurrentUser(participantId: number): boolean {
    return participantId === (this.auth.currentUserId() ?? -1);
  }

  getParticipantLines(vm: OrderCardVM, participantId: number): ParticipantOrderLinesApiDto | null {
    return vm.participantOrderLines.find(g => g.participantId === participantId) ?? null;
  }

  participantSubtotal(vm: OrderCardVM, participantId: number): number {
    const group = this.getParticipantLines(vm, participantId);
    return group?.lines.reduce((sum, line) => sum + line.lineTotal, 0) ?? 0;
  }

  progressPercent(vm: OrderCardVM): number {
    if (vm.participantCount <= 0) return 0;
    return Math.min(100, Math.round((vm.paidParticipantCount / vm.participantCount) * 100));
  }
}
