import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BottomNavComponent } from './layout/bottom-nav/bottom-nav.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, BottomNavComponent],
  template: `
    <div class="app-shell">
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
      background: #0a0e1a;
      position: relative;
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

