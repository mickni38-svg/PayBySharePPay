import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state">
      <div class="empty-state__icon">{{ icon }}</div>
      <h2 class="empty-state__title">{{ title }}</h2>
      @if (subtitle) {
        <p class="empty-state__subtitle">{{ subtitle }}</p>
      }
      @if (actionLabel) {
        <button class="empty-state__action" (click)="actionClick.emit()">
          {{ actionLabel }}
        </button>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex; flex-direction: column;
      align-items: center; text-align: center;
      padding: 40px 32px; gap: 12px;
    }
    .empty-state__icon { font-size: 56px; line-height: 1; margin-bottom: 4px; }
    .empty-state__title {
      font-size: 18px; font-weight: 700;
      color: #FFFFFF; margin: 0;
    }
    .empty-state__subtitle {
      font-size: 14px; color: #94A3B8;
      margin: 0; line-height: 1.5; max-width: 280px;
    }
    .empty-state__action {
      margin-top: 8px;
      background: #22C55E; color: #000;
      border: none; border-radius: 14px;
      padding: 12px 28px; font-size: 15px; font-weight: 700;
      cursor: pointer; transition: opacity 0.15s;
      min-height: 48px;
    }
    .empty-state__action:active { opacity: 0.8; }
  `]
})
export class EmptyStateComponent {
  @Input() icon = '📋';
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() actionLabel?: string;
  @Output() actionClick = new EventEmitter<void>();
}
