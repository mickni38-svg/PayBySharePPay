import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
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
