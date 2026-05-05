import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="orders-container">
      <h1>Ordrer</h1>

      <div class="orders-list">
        <p>Ordreliste implementeres i næste step</p>
      </div>

      <button class="btn-primary create-order-btn">
        Opret ny ordre
      </button>
    </div>
  `,
  styles: [`
    .orders-container {
      padding: var(--spacing-lg);
      max-width: 390px;
      margin: 0 auto;
    }

    h1 {
      font-size: 24px;
      margin-bottom: var(--spacing-lg);
    }

    .orders-list {
      margin-bottom: var(--spacing-xl);
    }

    .create-order-btn {
      width: 100%;
    }
  `]
})
export class OrdersComponent {
}
