import { Component, OnInit, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { PendingParticipantsSummary, PendingOrder, PendingParticipant, computePendingSummary } from '../../core/models/order.model';

export const REMINDER_MESSAGE =
  `Hej 👋\nVi mangler stadig, at du færdiggør din handling i PayBySharePay.\nSå vi kan gøre ordren klar.\nTak! 🙂`;

@Component({
  selector: 'app-pending-participants',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pending-participants.component.html',
  styleUrl: './pending-participants.component.scss'
})
export class PendingParticipantsComponent implements OnInit {
  loading = signal(true);
  summary = signal<PendingParticipantsSummary | null>(null);
  showReminderDialog = signal(false);
  reminderSent = signal(false);

  readonly reminderMessage = REMINDER_MESSAGE;

  constructor(
    private auth: AuthService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const userId = this.auth.currentUserId();
    if (!userId) { this.loading.set(false); return; }

    this.orderService.getOrdersByParticipant(userId).subscribe({
      next: (orders) => {
        this.summary.set(computePendingSummary(orders, userId));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
  }

  goToOrder(orderId: number): void {
    this.router.navigate(['/orders', orderId]);
  }

  goBack(): void {
    this.router.navigate(['/home']);
  }

  openReminderDialog(): void {
    this.reminderSent.set(false);
    this.showReminderDialog.set(true);
  }

  closeReminderDialog(): void {
    this.showReminderDialog.set(false);
  }

  sendReminders(): void {
    // Placeholder: kobles på backend notification-service når den er klar
    console.log('Sender påmindelser til:', this.allPendingParticipants());
    this.reminderSent.set(true);
    setTimeout(() => this.showReminderDialog.set(false), 1500);
  }

  allPendingParticipants(): Array<{ name: string; orderTitle: string }> {
    const s = this.summary();
    if (!s) return [];
    return s.orders.flatMap(o =>
      o.pendingParticipants.map(p => ({ name: p.displayName, orderTitle: o.orderTitle }))
    );
  }
}
