import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ActionCard {
  label: string;
  icon: string;
  route: string;
  colorStart: string;
  colorEnd: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="home">
      <header class="home__header">
        <h1 class="home__title">Forside</h1>
      </header>

      <section class="home__cards">
        @for (card of actionCards; track card.route) {
          <a class="action-card" [routerLink]="card.route" [style.--c1]="card.colorStart" [style.--c2]="card.colorEnd">
            <div class="action-card__icon-wrap">
              <span class="action-card__icon">{{ card.icon }}</span>
            </div>
            <span class="action-card__label">{{ card.label }}</span>
          </a>
        }
      </section>

      <section class="home__quick">
        <div class="quick-login">
          <div class="quick-login__field">Adda Algren</div>
          <button class="quick-login__btn">Log ind</button>
          <div class="quick-login__links">
            <a class="quick-login__link quick-login__link--green" href="#">Opret dig som bruger</a>
            <a class="quick-login__link quick-login__link--orange" href="#">Log ud</a>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .home {
      min-height: 100%;
      background: #0a0e1a;
      color: #fff;
    }

    .home__header {
      padding: 20px 20px 8px;
    }

    .home__title {
      font-size: 24px;
      font-weight: 700;
      color: #fff;
    }

    .home__cards {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      padding: 16px 20px;
    }

    .action-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 24px 12px;
      border-radius: 20px;
      border: 2px solid transparent;
      background:
        linear-gradient(#0f1628, #0f1628) padding-box,
        linear-gradient(135deg, var(--c1), var(--c2)) border-box;
      text-decoration: none;
      cursor: pointer;
      transition: transform 0.15s ease, opacity 0.15s ease;
      min-height: 140px;
    }

    .action-card:active {
      transform: scale(0.96);
      opacity: 0.85;
    }

    .action-card__icon-wrap {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.05);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .action-card__icon {
      font-size: 30px;
      line-height: 1;
    }

    .action-card__label {
      font-size: 15px;
      font-weight: 600;
      color: #fff;
    }

    .home__quick {
      padding: 8px 20px 32px;
    }

    .quick-login {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .quick-login__field {
      background: #1a2035;
      border-radius: 12px;
      padding: 14px 16px;
      color: #ccc;
      font-size: 15px;
      min-height: 48px;
    }

    .quick-login__btn {
      background: #1a2a5e;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 14px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      min-height: 48px;
      transition: opacity 0.2s;
    }

    .quick-login__btn:active {
      opacity: 0.8;
    }

    .quick-login__links {
      display: flex;
      justify-content: space-between;
      padding: 0 4px;
    }

    .quick-login__link {
      font-size: 14px;
      font-weight: 500;
      text-decoration: none;
    }

    .quick-login__link--green {
      color: #2ecc71;
    }

    .quick-login__link--orange {
      color: #e67e22;
    }
  `]
})
export class HomeComponent {
  actionCards: ActionCard[] = [
    { label: 'Overblik',  icon: '📊', route: '/orders',            colorStart: '#f0a500', colorEnd: '#2ecc71' },
    { label: 'Opret',     icon: '➕', route: '/orders/create',      colorStart: '#2ecc71', colorEnd: '#1abc9c' },
    { label: 'Brugere',   icon: '👥', route: '/find-participants',  colorStart: '#3498db', colorEnd: '#9b59b6' },
    { label: 'Beskeder',  icon: '💬', route: '/messages',           colorStart: '#9b59b6', colorEnd: '#3498db' }
  ];
}

