import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { MessageService } from '../../core/services/message.service';
import { FriendService } from '../../core/services/friend.service';
import { ThemeService } from '../../core/services/theme.service';
import { getStaticMerchantLogoUrl } from '../../core/utils/merchant-logo';
import { computePendingSummary } from '../../core/models/order.model';
import {
  filterAndLimitMerchants,
  getCarouselScrollTarget,
  markMerchantAsRecentlyUsed,
  parseRecentMerchantIds,
  sortMerchantsByRecentUse
} from './merchant-carousel.utils';
import { environment } from '../../../environments/environment';

const RECENT_MERCHANTS_KEY_PREFIX = 'paynsync_recent_merchants';
const MERCHANT_CARD_SCROLL_STEP = 102;

interface MerchantCard {
  id: number;
  displayName: string;
  handle?: string;
  initials: string;
  logoUrl: string | null;
  fallbackLogoUrl: string | null;
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
  type: 'pending' | 'allPaid' | 'activity';
  title: string;
  subtitle: string;
  orderId?: number;
}

function toInitials(name: string): string {
  return name.split(' ').slice(0, 2).map(p => p[0] ?? '').join('').toUpperCase();
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
    { label: 'Overblik',  subtitle: 'Se igangværende gruppebetalinger', route: '/orders',           accent: '#38BDF8', iconBg: 'rgba(56,189,248,0.15)',  icon: 'chart'    },
    { label: 'Beskeder',  subtitle: 'Se dine anmodninger',             route: '/messages',          accent: '#F472B6', iconBg: 'rgba(244,114,182,0.15)', icon: 'chat'     },
    { label: 'Deltagere', subtitle: 'Find og tilføj venner',           route: '/find-participants', accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',   icon: 'users'    },
    { label: 'Profil',    subtitle: 'Se og rediger dine oplysninger',   route: '/profile',           accent: '#FFCCFF', iconBg: 'rgba(255,204,255,0.15)', icon: 'activity' },
  ];

  statusCards = signal<StatusCard[]>([]);
  dismissedAllPaidIds = signal<Set<number>>(new Set());
  readyToPayCount = signal<number>(0);
  friendCount = signal<number | null>(null);

  // Merchant carousel
  allMerchants = signal<MerchantCard[]>([]);
  merchantSearch = signal('');

  filteredMerchants = computed(() =>
    filterAndLimitMerchants(this.allMerchants(), this.merchantSearch())
  );

  private routerSub?: Subscription;
  private recentMerchantIds: number[] = [];
  private activeUserId: number | null = null;

  constructor(
    readonly auth: AuthService,
    private orderService: OrderService,
    readonly messageService: MessageService,
    protected readonly themeService: ThemeService,
    private friendService: FriendService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.refreshData();

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
      this.loadMerchants(userId);
    }
  }

  private loadMerchants(userId: number): void {
    this.activeUserId = userId;
    this.recentMerchantIds = this.readRecentMerchantIds(userId);

    this.friendService.getFriends(userId).subscribe({
      next: (list) => {
        this.friendCount.set(list.length);
        const merchants: MerchantCard[] = list
          .filter(e => e.type === 'Merchant')
          .map(e => {
            const staticLogoUrl = getStaticMerchantLogoUrl(e);
            const apiLogoUrl = e.logoUrl ? `${environment.apiUrl}${e.logoUrl}` : null;

            return {
              id: e.id,
              displayName: e.displayName,
              handle: e.handle,
              initials: toInitials(e.displayName),
              logoUrl: staticLogoUrl ?? apiLogoUrl,
              fallbackLogoUrl: staticLogoUrl ? apiLogoUrl : null
            };
          });

        this.allMerchants.set(
          sortMerchantsByRecentUse(merchants, this.recentMerchantIds)
        );
      },
      error: () => {
        this.friendCount.set(0);
        this.allMerchants.set([]);
      }
    });
  }

  selectMerchant(m: MerchantCard): void {
    this.rememberMerchantUse(m.id);

    this.router.navigate(['/orders/create'], {
      state: {
        merchant: {
          id: m.id,
          displayName: m.displayName,
          handle: m.handle,
          logoUrl: m.logoUrl,
          fallbackLogoUrl: m.fallbackLogoUrl
        }
      }
    });
  }

  onMerchantLogoError(merchantId: number): void {
    this.allMerchants.update(merchants => merchants.map(merchant =>
      merchant.id === merchantId
        ? { ...merchant, logoUrl: merchant.fallbackLogoUrl, fallbackLogoUrl: null }
        : merchant
    ));
  }

  onCarouselKeydown(event: KeyboardEvent, carousel: HTMLElement): void {
    const maximumLeft = Math.max(0, carousel.scrollWidth - carousel.clientWidth);
    const target = getCarouselScrollTarget(
      event.key,
      carousel.scrollLeft,
      maximumLeft,
      MERCHANT_CARD_SCROLL_STEP
    );

    if (target == null) return;

    event.preventDefault();
    carousel.scrollTo({ left: target, behavior: 'smooth' });
  }

  private rememberMerchantUse(merchantId: number): void {
    if (this.activeUserId == null) return;

    this.recentMerchantIds = markMerchantAsRecentlyUsed(
      this.recentMerchantIds,
      merchantId
    );

    try {
      localStorage.setItem(
        this.recentMerchantsStorageKey(this.activeUserId),
        JSON.stringify(this.recentMerchantIds)
      );
    } catch {
      // Storage can be unavailable in privacy-restricted browsers.
    }

    this.allMerchants.update(merchants =>
      sortMerchantsByRecentUse(merchants, this.recentMerchantIds)
    );
  }

  private readRecentMerchantIds(userId: number): number[] {
    try {
      return parseRecentMerchantIds(
        localStorage.getItem(this.recentMerchantsStorageKey(userId))
      );
    } catch {
      return [];
    }
  }

  private recentMerchantsStorageKey(userId: number): string {
    return `${RECENT_MERCHANTS_KEY_PREFIX}:${userId}`;
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

}
