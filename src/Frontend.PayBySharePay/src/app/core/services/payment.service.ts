import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment, RegisterPaymentRequest } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/api/payments`;

  constructor(private http: HttpClient) {}

  // POST /api/payments
  registerPayment(request: RegisterPaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(this.apiUrl, request);
  }
}

