import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="payment-return" aria-live="polite">
      <div class="payment-return__icon">✓</div>
      <h1>Betalingen er sendt</h1>
      <p>Vi registrerer din betaling. Du kan nu gå tilbage til PayNSync.</p>
      <a routerLink="/home">Til forsiden</a>
    </section>
  `,
  styles: [`
    :host { display:block; min-height:100%; background:var(--color-bg); color:var(--color-text-primary); }
    .payment-return {
      min-height: calc(100dvh - 120px);
      box-sizing: border-box;
      display:flex;
      flex-direction:column;
      align-items:center;
      justify-content:center;
      gap:14px;
      padding:32px 24px;
      text-align:center;
    }
    .payment-return__icon {
      display:grid;
      place-items:center;
      width:64px;
      height:64px;
      border-radius:50%;
      background:rgba(34,197,94,.15);
      color:#34d173;
      font-size:32px;
      font-weight:800;
    }
    h1 { margin:0; font-size:24px; }
    p { margin:0; max-width:320px; color:var(--color-text-secondary); line-height:1.5; }
    a {
      margin-top:8px;
      width:min(100%,320px);
      min-height:48px;
      display:flex;
      align-items:center;
      justify-content:center;
      border-radius:12px;
      background:var(--color-primary);
      color:#03130a;
      font-weight:800;
      text-decoration:none;
    }
  `]
})
export class PaymentReturnComponent implements OnInit {
  constructor(private readonly router: Router) {}

  ngOnInit(): void {
    window.setTimeout(() => this.router.navigate(['/home']), 1800);
  }
}
