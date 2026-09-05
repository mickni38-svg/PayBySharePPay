import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrderHubOrder, OrderHubSettings, OrderHubStatus } from '../models/order-hub.model';

@Injectable({ providedIn: 'root' })
export class OrderHubService {
  private readonly apiUrl = `${environment.apiUrl}/api/order-hub`;

  constructor(private readonly http: HttpClient) {}

  getSettings(): Observable<OrderHubSettings> {
    return this.http.get<OrderHubSettings>(`${this.apiUrl}/settings`);
  }

  setEnabled(enabled: boolean): Observable<OrderHubSettings> {
    return this.http.put<OrderHubSettings>(`${this.apiUrl}/settings`, { enabled });
  }

  getActiveOrders(): Observable<OrderHubOrder[]> {
    return this.http.get<OrderHubOrder[]>(`${this.apiUrl}/orders`);
  }

  getHistory(): Observable<OrderHubOrder[]> {
    return this.http.get<OrderHubOrder[]>(`${this.apiUrl}/history`);
  }

  updateStatus(orderId: number, status: OrderHubStatus): Observable<OrderHubOrder> {
    return this.http.put<OrderHubOrder>(`${this.apiUrl}/orders/${orderId}/status`, { status });
  }
}
