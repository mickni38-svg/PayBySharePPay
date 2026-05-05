import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OrderApiDto,
  OrderOverviewApiDto,
  CreateOrderRequest
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/api/orders`;

  constructor(private http: HttpClient) {}

  // POST /api/orders
  createOrder(request: CreateOrderRequest): Observable<OrderApiDto> {
    return this.http.post<OrderApiDto>(this.apiUrl, request);
  }

  // GET /api/orders/{id}/overview
  getOrderOverview(id: number): Observable<OrderOverviewApiDto> {
    return this.http.get<OrderOverviewApiDto>(`${this.apiUrl}/${id}/overview`);
  }
}

