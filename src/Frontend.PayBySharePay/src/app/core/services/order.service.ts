import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OrderApiDto,
  OrderOverviewApiDto,
  OrderSummaryApiDto,
  CreateOrderRequest
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/api/orders`;

  constructor(private http: HttpClient) {}

  /** GET /api/orders?participantId=x – ordrer for én bruger */
  getOrdersByParticipant(participantId: number): Observable<OrderSummaryApiDto[]> {
    const params = new HttpParams().set('participantId', participantId);
    return this.http.get<OrderSummaryApiDto[]>(this.apiUrl, { params });
  }

  /** GET /api/orders – alle ordrer */
  getAllOrders(): Observable<OrderSummaryApiDto[]> {
    return this.http.get<OrderSummaryApiDto[]>(this.apiUrl);
  }

  /** POST /api/orders – opret ny ordre */
  createOrder(request: CreateOrderRequest): Observable<OrderApiDto> {
    return this.http.post<OrderApiDto>(this.apiUrl, request);
  }

  /** GET /api/orders/{id}/overview – detaljeret ordrevisning */
  getOrderOverview(id: number): Observable<OrderOverviewApiDto> {
    return this.http.get<OrderOverviewApiDto>(`${this.apiUrl}/${id}/overview`);
  }
}

