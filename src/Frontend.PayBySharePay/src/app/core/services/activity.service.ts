import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { OrderService } from './order.service';
import { ActivityFeed, ActivityItem, ActivityType } from '../models/activity.model';
import { OrderSummaryApiDto, OrderParticipantApiDto } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class ActivityService {

  constructor(private orderService: OrderService) {}

  /**
   * Bygger en aktivitetsfeed baseret på eksisterende ordredata for brugeren.
   * Aktiviteter er sorteret nyeste først.
   */
  getActivityFeed(userId: number): Observable<ActivityFeed> {
    return this.orderService.getOrdersByParticipant(userId).pipe(
      map(orders => this.buildFeed(orders, userId)),
      catchError(() => of({ items: [], unreadCount: 0 }))
    );
  }

  private buildFeed(orders: OrderSummaryApiDto[], userId: number): ActivityFeed {
    const items: ActivityItem[] = [];

    for (const order of orders) {
      // Ordrestatus-aktivitet
      items.push(this.orderActivity(order));

      // Deltager-aktiviteter
      for (const p of order.participants) {
        if (p.type === 'Merchant') continue;
        const a = this.participantActivity(p, order);
        if (a) items.push(a);
      }
    }

    // Sortér nyeste ordre-dato først (ordre.createdAt bruges som proxy)
    items.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

    // Ulæste = aktiviteter inden for de seneste 24 timer
    const cutoff = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
    const unreadCount = items.filter(i => i.createdAt > cutoff).length;

    return { items, unreadCount };
  }

  private orderActivity(order: OrderSummaryApiDto): ActivityItem {
    let icon = '📋';
    let title = '';

    switch (order.status) {
      case 'Ready':       icon = '✅'; title = `${order.title} er klar til betaling`; break;
      case 'Completed':   icon = '🎉'; title = `${order.title} er betalt`; break;
      case 'Cancelled':   icon = '❌'; title = `${order.title} blev annulleret`; break;
      case 'Collecting':  icon = '🛒'; title = `${order.title} samler bestillinger`; break;
      default:            icon = '📋'; title = `${order.title} afventer`; break;
    }

    return {
      id: `order-${order.id}`,
      type: 'order',
      icon,
      title,
      orderName: order.title,
      orderId: order.id,
      createdAt: order.createdAt
    };
  }

  private participantActivity(p: OrderParticipantApiDto, order: OrderSummaryApiDto): ActivityItem | null {
    let icon = '';
    let title = '';
    const type: ActivityType = p.status === 'Paid' ? 'payment' : 'participant';

    switch (p.status) {
      case 'Paid':
        icon = '💳'; title = `${p.name} har betalt til ${order.title}`; break;
      case 'Accepted':
        icon = '✓'; title = `${p.name} har accepteret invitation til ${order.title}`; break;
      case 'Invited':
        icon = '📨'; title = `${p.name} er inviteret til ${order.title}`; break;
      default:
        return null;
    }

    return {
      id: `participant-${order.id}-${p.participantId}`,
      type,
      icon,
      title,
      orderName: order.title,
      orderId: order.id,
      createdAt: order.createdAt
    };
  }
}
