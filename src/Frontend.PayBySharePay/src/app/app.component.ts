import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BottomNavComponent } from './layout/bottom-nav/bottom-nav.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BottomNavComponent],
  template: `
    <div class="app-shell">
      <header class="app-header">
        <img class="app-header__logo" src="images/logo.png" alt="PayBySharePay" />
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
    }

    .app-header {
      display: flex;
      align-items: center;
      justify-content: flex-start;
      padding: 6px 20px 2px;
      background: #070B14;
    }

    .app-header__logo {
      width: 100%;
      max-width: 320px;
      height: auto;
      object-fit: contain;
      display: block;
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
}

