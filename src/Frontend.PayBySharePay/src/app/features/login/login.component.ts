import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = '';
  password = '';
  showPassword = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

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
            : 'Noget gik galt. PrÃÂ¸v igen.'
        );
        this.loading.set(false);
      }
    });
  }
}
