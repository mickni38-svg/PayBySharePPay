import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderParticipantApiDto, OrderSummaryApiDto } from '../../core/models/order.model';

interface OrderLine { name: string; quantity: number; unitPrice: number; }

interface ParticipantRow extends OrderParticipantApiDto {
  expanded: boolean;
  orderLines: OrderLine[];
}

const MOCK_SETS: OrderLine[][] = [
  [
    { name: 'Pizza Margherita', quantity: 1, unitPrice: 100 },
    { name: 'Pommes frites', quantity: 1, unitPrice: 33 },
    { name: 'Coca Cola', quantity: 2, unitPrice: 16 },
  ],
  [
    { name: 'Burger Classic', quantity: 2, unitPrice: 89 },
    { name: 'Øl (0,5L)', quantity: 2, unitPrice: 45 },
  ],
  // index 2+ → ingen bestilling
];

interface ActiveOrderVM {
  id: number;
  title: string;
  category?: string;
  participants: ParticipantRow[];
}

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {

  orders = signal<OrderSummaryApiDto[]>([]);
  activeId = signal<number | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  // Aktiv ordre med expanded-state pr. deltager
  activeOrderVm = signal<ActiveOrderVM | null>(null);

  allPaid = computed(() => {
    const vm = this.activeOrderVm();
    if (!vm || vm.participants.length === 0) return false;
    return vm.participants.every(p => p.status === 'Paid');
  });

  private readonly AVATAR_COLORS = [
    '#7c5cbf','#2e7d32','#1565c0','#ad1457',
    '#00838f','#558b2f','#4527a0','#6d4c41'
  ];

  constructor(private orderService: OrderService, private router: Router, private auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.isLoading.set(true);
    this.orderService.getOrdersByParticipant(this.auth.currentUserId() ?? 0).subscribe({
      next: (list) => {
        this.orders.set(list);
        if (list.length > 0) this.setActive(list[0].id);
        this.isLoading.set(false);
      },
      error: () => { this.errorMessage.set('Kunne ikke hente ordrer.'); this.isLoading.set(false); }
    });
  }

  setActive(id: number): void {
    this.activeId.set(id);
    const order = this.orders().find(o => o.id === id);
    if (!order) return;
    this.activeOrderVm.set({
      id: order.id,
      title: order.title,
      category: order.category,
      participants: order.participants.map((p, i) => ({
        ...p,
        expanded: i < 2,
        orderLines: MOCK_SETS[i] ?? []
      }))
    });
  }

  toggleParticipant(participantId: number): void {
    const vm = this.activeOrderVm();
    if (!vm) return;
    this.activeOrderVm.set({
      ...vm,
      participants: vm.participants.map(p =>
        p.participantId === participantId ? { ...p, expanded: !p.expanded } : p
      )
    });
  }

  goCreate(): void { this.router.navigate(['/orders/create']); }

  finalPay(): void {
    alert('Alle har betalt — sender ordre til spisestedet (implementeres næste step)');
  }

  lineTotal(lines: OrderLine[]): number {
    return lines.reduce((sum, l) => sum + l.quantity * l.unitPrice, 0);
  }

  statusLabel(s: string): string {
    const map: Record<string, string> = {
      Invited: 'afventer', Accepted: 'accepteret', Declined: 'afvist', Paid: 'betalt'
    };
    return map[s] ?? s.toLowerCase();
  }

  statusClass(s: string): string {
    const map: Record<string, string> = {
      Invited: 'status-pending', Accepted: 'status-accepted',
      Paid: 'status-paid', Declined: 'status-declined'
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

  initials(name: string): string {
    return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
  }

  avatarColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
    return this.AVATAR_COLORS[Math.abs(hash) % this.AVATAR_COLORS.length];
  }
}
