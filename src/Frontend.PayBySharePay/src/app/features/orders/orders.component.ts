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
  canPayOwnShare: boolean;
  allPaid: boolean;
  canShowOrderLines: boolean;
  participants: OrderParticipantApiDto[];
  participantOrderLines: ParticipantOrderLinesApiDto[];
  detailsLoaded: boolean;
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
  activeTab = signal<'host' | 'participant'>('host');
  filterPending = signal(false);

  // Cache af ordredetaljer hentet fra API
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
        this.isLoading.set(false);
      },
      error: () => { this.errorMessage.set('Kunne ikke hente ordrer.'); this.isLoading.set(false); }
    });
  }

  private buildVM(o: OrderSummaryApiDto): OrderCardVM {
    const userId = this.auth.currentUserId() ?? 0;
    const isHost = o.createdByParticipantId === userId;
    const nonMerchant = o.participants.filter(p => p.type !== 'Merchant');
    const paidCount = nonMerchant.filter(p => p.status === 'Paid').length;
    const isPending = o.status === 'Collecting' || o.status === 'WaitingForPayment' ||
      nonMerchant.some(p => p.status === 'Invited');
    const myPart = o.participants.find(p => p.participantId === userId);

    // Hent detaljer fra cache hvis tilgængeligt
    const cached = this._detailsCache().get(o.id);

    // canShowOrderLines: vis linjer så snart de er tilgængelige (bestilling indsendt)
    const anyoneHasLines = cached?.participantOrderLines.some(g => g.lines.length > 0) ?? false;
    const myLines = cached?.participantOrderLines.find(g => g.participantId === userId);
    const canShow = isHost ? anyoneHasLines : (myLines?.lines?.length ?? 0) > 0;

    // Filtrer ordrelinjer: vært ser alle med linjer, deltager kun egne
    const visibleLines = cached?.participantOrderLines.filter(g =>
      isHost ? g.lines.length > 0 : g.participantId === userId && g.lines.length > 0
    ) ?? [];

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
      canPayTotalOrder: isHost && (isPending || paidCount === nonMerchant.length),
      canPayOwnShare: !isHost && (myPart?.status === 'Invited' || myPart?.status === 'Accepted'),
      allPaid: nonMerchant.length > 0 && paidCount === nonMerchant.length,
      canShowOrderLines: canShow,
      participants: nonMerchant,
      participantOrderLines: visibleLines,
      detailsLoaded: !!cached
    };
  }

  hostOrders = computed(() => {
    // Touch cache signal for reaktivitet
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
      list = list.filter(vm => vm.canPayOwnShare);
    }
    return list;
  });

  activeOrders = computed(() =>
    this.activeTab() === 'host' ? this.hostOrders() : this.participantOrders()
  );

  setTab(tab: 'host' | 'participant'): void {
    this.activeTab.set(tab);
  }

  clearFilter(): void {
    this.filterPending.set(false);
  }

  toggleExpand(id: number): void {
    const current = this._expandedIds();
    if (current.has(id)) {
      const next = new Set(current);
      next.delete(id);
      this._expandedIds.set(next);
    } else {
      this._expandedIds.set(new Set([...current, id]));
      // Hent detaljer hvis ikke cachet
      if (!this._detailsCache().has(id)) {
        this.loadDetails(id);
      }
    }
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

  payOrder(id: number): void {
    alert(`Betal ordre #${id} til spisestedet — implementeres i næste step`);
  }

  payShare(id: number): void {
    alert(`Betal din andel for ordre #${id} — implementeres i næste step`);
  }

  goCreate(): void { this.router.navigate(['/orders/create']); }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Collecting: 'Samler',
      WaitingForPayment: 'Afventer betaling',
      Ready: 'Klar',
      Completed: 'Betalt',
      Cancelled: 'Annulleret'
    };
    return map[s] ?? s;
  }

  statusClass(s: string): string {
    const map: Record<string, string> = {
      Collecting: 'badge--pending',
      WaitingForPayment: 'badge--pending',
      Ready: 'badge--ready',
      Completed: 'badge--paid',
      Cancelled: 'badge--declined'
    };
    return map[s] ?? '';
  }

  categoryIcon(cat?: string): string {
    const map: Record<string, string> = {
      sushi: '🍣', pizza: '🍕', burger: '🍔', drinks: '🍺',
      tacos: '🌮', ramen: '🍜', kebab: '🥙', chicken: '🍗',
      salad: '🥗', dessert: '🍰', coffee: '☕', other: '📦'
    };
    return map[cat ?? ''] ?? '🍴';
  }

  formatDate(dateStr: string): string {
    try {
      const d = new Date(dateStr);
      return d.toLocaleDateString('da-DK', { day: 'numeric', month: 'long', year: 'numeric' });
    } catch {
      return dateStr;
    }
  }

  initials(name: string): string {
    return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
  }

  avatarColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
    return this.AVATAR_COLORS[Math.abs(hash) % this.AVATAR_COLORS.length];
  }

  visibleAvatars(participants: OrderParticipantApiDto[]): OrderParticipantApiDto[] {
    return participants.slice(0, 4);
  }

  extraAvatarCount(participants: OrderParticipantApiDto[]): number {
    return Math.max(0, participants.length - 4);
  }

  isCurrentUser(participantId: number): boolean {
    return participantId === (this.auth.currentUserId() ?? -1);
  }
}

