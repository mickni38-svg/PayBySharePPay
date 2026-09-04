import { Component, ViewEncapsulation } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BottomNavComponent } from './layout/bottom-nav/bottom-nav.component';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BottomNavComponent],
  encapsulation: ViewEncapsulation.None,
  template: `
    <div class="app-shell" [class.app-shell--public]="!auth.isLoggedIn()">
      <header class="app-header">
        <img class="app-header__logo" src="images/logo.png" alt="PayNSync" />
      </header>
      <main class="app-shell__content">
        <router-outlet></router-outlet>
      </main>
      <app-bottom-nav></app-bottom-nav>
    </div>
  `,
  styles: [`
    .app-shell {
      display: flex;
      flex-direction: column;
      min-height: 100dvh;
      max-width: 390px;
      margin: 0 auto;
      background: #070B14;
      position: relative;
      font-family: var(--font-family);
      font-size: 16px;
      line-height: 1.5;
    }

    .app-shell button,
    .app-shell input,
    .app-shell select,
    .app-shell textarea,
    .app-shell a {
      font-family: var(--font-family);
    }

    .app-header {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2px 12px 0;
      background: #070B14;
    }

    .app-header__logo {
      width: 100%;
      max-width: 250px;
      height: auto;
      object-fit: contain;
      display: block;
    }

    /* BUG-04: offentlig forside skal være én viewport uden intern scroll. */
    .app-shell--public {
      height: 100dvh;
      min-height: 100dvh;
      overflow: hidden;
    }

    .app-shell--public .app-header {
      flex: 0 0 auto;
      padding-top: 0;
    }

    .app-shell--public .app-header__logo {
      max-width: 290px;
    }

    .app-shell--public .app-shell__content {
      flex: 1 1 auto;
      min-height: 0;
      overflow: hidden;
      padding-bottom: 0;
    }

    /* Forsiden bruger 15px/700 til primære korttitler og 12px til sekundær tekst.
       Brug samme typografiske skala i kontocenteret, så siden matcher resten af appen. */
    .account-center .accordion-title,
    .account-center .card-heading h2,
    .account-center .identity-card h2 {
      font-size: 15px;
      font-weight: 700;
      line-height: 1.2;
    }

    .account-center .accordion-meta,
    .account-center .accordion-description,
    .account-center .card-heading p,
    .account-center .identity-card p,
    .account-center .form-field small,
    .account-center .setting-row span {
      font-size: 12px;
      line-height: 1.35;
    }

    .account-center .form-field label,
    .account-center .theme-picker legend,
    .account-center .form-section-title,
    .account-center .main-tabs__tab,
    .account-center .mode-tabs__tab,
    .account-center .account-type-toggle__button {
      font-size: 13px;
      font-weight: 600;
    }

    .account-center .form-field input,
    .account-center .form-field select,
    .account-center .primary-btn,
    .account-center .secondary-btn,
    .account-center .danger-btn,
    .account-center .danger-outline-btn {
      font-family: var(--font-family);
      font-size: 15px;
    }

    /* Bottom navigation provides the home action, so page-level Hjem links are redundant. */
    .page-back {
      display: none !important;
    }

    .app-shell__content {
      flex: 1;
      overflow-y: auto;
      padding-bottom: 64px;
    }
  `]
})
export class AppComponent {
  title = 'PayBySharePay';

  constructor(readonly auth: AuthService) {}
}
