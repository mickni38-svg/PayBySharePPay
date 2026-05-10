import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="badgeClass">{{ displayLabel }}</span>`,
  styles: [`
    :host { display: inline-flex; }
    .badge {
      display: inline-flex; align-items: center;
      padding: 3px 10px; border-radius: 20px;
      font-size: 11px; font-weight: 600;
      text-transform: uppercase; letter-spacing: 0.4px;
    }
    .badge--pending  { color: #F59E0B; background: rgba(245,158,11,0.12); }
    .badge--accepted { color: #22C55E; background: rgba(34,197,94,0.12); }
    .badge--paid     { color: #06B6D4; background: rgba(6,182,212,0.12); }
    .badge--declined { color: #EF4444; background: rgba(239,68,68,0.12); }
    .badge--ordered  { color: #22C55E; background: rgba(34,197,94,0.12); }
    .badge--default  { color: #94A3B8; background: rgba(148,163,184,0.12); }
  `]
})
export class StatusBadgeComponent {
  @Input() status = '';
  @Input() label?: string;

  get badgeClass(): string {
    const map: Record<string, string> = {
      'Invited':  'badge--pending',
      'Accepted': 'badge--accepted',
      'Paid':     'badge--paid',
      'Declined': 'badge--declined',
      'pending':  'badge--pending',
      'accepted': 'badge--accepted',
      'paid':     'badge--paid',
      'declined': 'badge--declined',
      'bestilt':  'badge--ordered',
      'afventer': 'badge--pending',
    };
    return map[this.status] ?? 'badge--default';
  }

  get displayLabel(): string {
    if (this.label) return this.label;
    const map: Record<string, string> = {
      'Invited':  'afventer',
      'Accepted': 'accepteret',
      'Paid':     'betalt',
      'Declined': 'afvist',
    };
    return map[this.status] ?? this.status.toLowerCase();
  }
}
