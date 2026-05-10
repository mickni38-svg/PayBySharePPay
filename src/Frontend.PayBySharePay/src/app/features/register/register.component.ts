import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

type Tab = 'person' | 'merchant';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  tab = signal<Tab>('person');

  // Person
  personName = '';
  personEmail = '';
  personPhone = '';

  // Merchant
  merchantName = '';
  merchantCompany = '';
  merchantCvr = '';
  merchantContact = '';
  merchantEmail = '';
  merchantPhone = '';
  merchantAddress = '';

  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  setTab(tab: Tab): void {
    this.tab.set(tab);
    this.error.set(null);
  }

  submit(): void {
    this.loading.set(true);
    this.error.set(null);

    if (this.tab() === 'person') {
      this.auth.register({
        name: this.personName,
        email: this.personEmail,
        phone: this.personPhone || undefined
      }).subscribe({
        next: () => this.router.navigate(['/home']),
        error: (err) => {
          this.error.set(err.status === 409
            ? 'En bruger med denne e-mail eksisterer allerede.'
            : 'Noget gik galt. Prøv igen.');
          this.loading.set(false);
        }
      });
    } else {
      this.auth.registerMerchant({
        name: this.merchantName,
        companyName: this.merchantCompany,
        cvrNumber: this.merchantCvr || undefined,
        contactPerson: this.merchantContact || undefined,
        contactEmail: this.merchantEmail || undefined,
        contactPhone: this.merchantPhone || undefined,
        companyAddress: this.merchantAddress || undefined
      }).subscribe({
        next: () => this.router.navigate(['/home']),
        error: (err) => {
          this.error.set(err.status === 409
            ? 'Et spisested med denne e-mail eksisterer allerede.'
            : 'Noget gik galt. Prøv igen.');
          this.loading.set(false);
        }
      });
    }
  }
}
