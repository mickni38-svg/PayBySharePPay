import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login">
      <div class="login__card">

        <header class="login__header">
          <div class="login__logo">💳</div>
          <h1 class="login__title">ShareBySharePay</h1>
          <p class="login__sub">Log ind med din e-mail</p>
        </header>

        <form class="login__form" (ngSubmit)="submit()">
          <input
            class="login__input"
            type="email"
            placeholder="din@email.dk"
            [(ngModel)]="email"
            name="email"
            autocomplete="email"
            required
          />

          @if (error()) {
            <p class="login__error">{{ error() }}</p>
          }

          <button
            class="login__btn"
            type="submit"
            [disabled]="loading() || !email"
          >
            @if (loading()) { Logger ind… } @else { Log ind }
          </button>
        </form>

      </div>
    </div>
  `,
  styles: [`
    .login {
      min-height: 100dvh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #0f0f12;
      padding: 1.5rem;
    }

    .login__card {
      width: 100%;
      max-width: 360px;
      background: #1a1a24;
      border-radius: 1rem;
      padding: 2rem 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .login__header {
      text-align: center;
    }

    .login__logo {
      font-size: 2.5rem;
      margin-bottom: .5rem;
    }

    .login__title {
      color: #e0e0e0;
      font-size: 1.2rem;
      font-weight: 700;
      margin: 0 0 .25rem;
    }

    .login__sub {
      color: #888;
      font-size: .85rem;
      margin: 0;
    }

    .login__form {
      display: flex;
      flex-direction: column;
      gap: .75rem;
    }

    .login__input {
      background: #252530;
      border: 1px solid #333;
      border-radius: .5rem;
      color: #e0e0e0;
      font-size: 1rem;
      padding: .75rem 1rem;
      outline: none;
      width: 100%;
      box-sizing: border-box;
    }

    .login__input:focus {
      border-color: #7c5cbf;
    }

    .login__error {
      color: #e57373;
      font-size: .82rem;
      margin: 0;
    }

    .login__btn {
      background: #7c5cbf;
      border: none;
      border-radius: .5rem;
      color: #fff;
      cursor: pointer;
      font-size: 1rem;
      font-weight: 600;
      padding: .75rem;
      transition: background .2s;
    }

    .login__btn:disabled {
      background: #4a3a70;
      cursor: not-allowed;
    }
  `]
})
export class LoginComponent {
  email = '';
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (!this.email) return;
    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.email).subscribe({
      next: () => this.router.navigate(['/home']),
      error: (err) => {
        this.error.set(
          err.status === 401 || err.status === 404
            ? 'Ingen konto fundet med den e-mail.'
            : 'Noget gik galt. Prøv igen.'
        );
        this.loading.set(false);
      }
    });
  }
}
