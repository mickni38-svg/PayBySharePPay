import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { OrderHubService } from '../../core/services/order-hub.service';
import { OrderHubModifier, OrderHubOrder, OrderHubStatus } from '../../core/models/order-hub.model';

const SOUND_KEY = 'paynsync_order_hub_sound';

@Component({
  selector: 'app-order-hub',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-hub.component.html',
  styleUrl: './order-hub.component.scss'
})
export class OrderHubComponent implements OnInit, OnDestroy {
  enabled = signal(false);
  loading = signal(true);
  error = signal<string | null>(null);
  activeOrders = signal<OrderHubOrder[]>([]);
  history = signal<OrderHubOrder[]>([]);
  showHistory = signal(false);
  soundEnabled = signal(localStorage.getItem(SOUND_KEY) === 'true');
  updatingId = signal<number | null>(null);

  private pollHandle: number | null = null;
  private knownOrderIds = new Set<number>();
  private initializedOrders = false;

  constructor(
    readonly auth: AuthService,
    private readonly hub: OrderHubService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    if (this.auth.currentUserType() !== 'Merchant') {
      this.router.navigate(['/profile']);
      return;
    }

    this.hub.getSettings().subscribe({
      next: settings => {
        this.enabled.set(settings.enabled);
        this.loading.set(false);
        if (settings.enabled) this.startHub();
      },
      error: () => {
        this.error.set('Order Hub-indstillinger kunne ikke hentes.');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.pollHandle !== null) window.clearInterval(this.pollHandle);
  }

  enableHub(): void {
    this.hub.setEnabled(true).subscribe({
      next: settings => {
        this.enabled.set(settings.enabled);
        if (settings.enabled) this.startHub();
      },
      error: () => this.error.set('Order Hub kunne ikke aktiveres.')
    });
  }

  toggleSound(): void {
    const enabled = !this.soundEnabled();
    this.soundEnabled.set(enabled);
    localStorage.setItem(SOUND_KEY, enabled ? 'true' : 'false');
    if (enabled) this.playSound();
  }

  toggleHistory(): void {
    this.showHistory.update(value => !value);
    if (this.showHistory()) this.loadHistory();
  }

  advance(order: OrderHubOrder): void {
    const next = this.nextStatus(order.status);
    if (!next) return;

    this.updatingId.set(order.id);
    this.hub.updateStatus(order.id, next).subscribe({
      next: updated => {
        this.updatingId.set(null);
        if (updated.status === 'Completed') {
          this.activeOrders.update(orders => orders.filter(item => item.id !== updated.id));
          this.history.update(orders => [updated, ...orders.filter(item => item.id !== updated.id)]);
        } else {
          this.activeOrders.update(orders => orders.map(item => item.id === updated.id ? updated : item));
        }
      },
      error: () => {
        this.updatingId.set(null);
        this.error.set('Ordrestatus kunne ikke opdateres.');
      }
    });
  }

  nextStatus(status: OrderHubStatus): OrderHubStatus | null {
    const transitions: Record<OrderHubStatus, OrderHubStatus | null> = {
      New: 'Accepted',
      Accepted: 'Preparing',
      Preparing: 'Ready',
      Ready: 'Completed',
      Completed: null
    };
    return transitions[status];
  }

  actionLabel(status: OrderHubStatus): string {
    const labels: Record<OrderHubStatus, string> = {
      New: 'Accepter ordre',
      Accepted: 'Start tilberedning',
      Preparing: 'Markér klar',
      Ready: 'Afslut ordre',
      Completed: 'Afsluttet'
    };
    return labels[status];
  }

  statusLabel(status: OrderHubStatus): string {
    const labels: Record<OrderHubStatus, string> = {
      New: 'Ny',
      Accepted: 'Accepteret',
      Preparing: 'Tilberedes',
      Ready: 'Klar',
      Completed: 'Afsluttet'
    };
    return labels[status];
  }

  modifiers(item: { modifiersJson?: string }): OrderHubModifier[] {
    if (!item.modifiersJson) return [];
    try {
      const value = JSON.parse(item.modifiersJson);
      return Array.isArray(value) ? value : [];
    } catch {
      return [];
    }
  }

  private startHub(): void {
    this.loadActive();
    this.pollHandle = window.setInterval(() => this.loadActive(), 10000);
  }

  private loadActive(): void {
    this.hub.getActiveOrders().subscribe({
      next: orders => {
        const incoming = orders.filter(order => !this.knownOrderIds.has(order.id));
        this.activeOrders.set(orders);
        this.knownOrderIds = new Set(orders.map(order => order.id));

        if (this.initializedOrders && incoming.length > 0 && this.soundEnabled())
          this.playSound();

        this.initializedOrders = true;
      },
      error: () => this.error.set('Ordrekøen kunne ikke opdateres.')
    });
  }

  private loadHistory(): void {
    forkJoin({ history: this.hub.getHistory() }).subscribe({
      next: result => this.history.set(result.history),
      error: () => this.error.set('Ordrehistorikken kunne ikke hentes.')
    });
  }

  private playSound(): void {
    try {
      const audioContext = new AudioContext();
      const oscillator = audioContext.createOscillator();
      const gain = audioContext.createGain();
      oscillator.connect(gain);
      gain.connect(audioContext.destination);
      oscillator.frequency.value = 880;
      gain.gain.value = 0.12;
      oscillator.start();
      oscillator.stop(audioContext.currentTime + 0.18);
    } catch {
      // Browseren kan blokere lyd indtil første brugerinteraktion.
    }
  }
}
