import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

declare const google: {
  accounts: {
    id: {
      initialize: (config: object) => void;
      renderButton: (parent: HTMLElement, options: object) => void;
    };
  };
};

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';
  showPassword = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    if (typeof google !== 'undefined') {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: { credential: string }) => this._handleGoogleResponse(response)
      });
      const btn = document.getElementById('google-signin-btn');
      if (btn) {
        google.accounts.id.renderButton(btn, {
          theme: 'outline',
          size: 'large',
          width: 312,
          text: 'continue_with',
          locale: 'da'
        });
      }
    }
  }

  private _handleGoogleResponse(response: { credential: string }): void {
    this.loading.set(true);
    this.error.set(null);
    this.auth.googleLogin(response.credential).subscribe({
      next: () => {
        window.location.href = '/home';
      },
      error: (err) => {
        const msg = err.error?.error;
        this.error.set(msg ?? 'Google-login mislykkedes. Prøv igen.');
        this.loading.set(false);
      }
    });
  }

  toggleShowPassword(): void {
    this.showPassword.update(v => !v);
  }

  submit(): void {
    if (!this.email) return;
    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.email, this.password || undefined).subscribe({
      next: () => {
        // Hard reload sikrer at alle komponenter starter fresh med login-state
        window.location.href = '/home';
      },
      error: (err) => {
        this.error.set(
          err.status === 401 || err.status === 404
            ? 'Ingen konto fundet med den e-mail eller forkert adgangskode.'
            : 'Noget gik galt. Prøv igen.'
        );
        this.loading.set(false);
      }
    });
  }
}
