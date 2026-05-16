import { Component, OnInit, OnDestroy, signal, effect, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MessageService } from '../../core/services/message.service';
import { AuthService } from '../../core/services/auth.service';
import { Subscription, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-bottom-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  template: `
    <nav class="bottom-nav">
      <a routerLink="/home" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" class="bottom-nav__item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M3 9.5L12 3l9 6.5V20a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1z"/>
          <polyline points="9 21 9 12 15 12 15 21"/>
        </svg>
        <span>Forside</span>
      </a>
      <a routerLink="/orders" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" class="bottom-nav__item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/>
          <rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>
        </svg>
        <span>Overblik</span>
      </a>
      <a routerLink="/orders/create" routerLinkActive="active" class="bottom-nav__item bottom-nav__item--create">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
          <circle cx="12" cy="12" r="10"/>
          <line x1="12" y1="7" x2="12" y2="17"/>
          <line x1="7" y1="12" x2="17" y2="12"/>
        </svg>
        <span>Opret</span>
      </a>
      <a routerLink="/find-participants" routerLinkActive="active" class="bottom-nav__item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
          <circle cx="9" cy="7" r="4"/>
          <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
          <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
        </svg>
        <span>Brugere</span>
      </a>
      <a routerLink="/messages" routerLinkActive="active" class="bottom-nav__item" (click)="clearBadge()">
        <div class="bottom-nav__icon-wrap">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
            <circle cx="9" cy="10" r="1" fill="currentColor"/>
            <circle cx="12" cy="10" r="1" fill="currentColor"/>
            <circle cx="15" cy="10" r="1" fill="currentColor"/>
          </svg>
          @if (unreadCount() > 0) {
            <span class="bottom-nav__badge">{{ unreadCount() > 9 ? '9+' : unreadCount() }}</span>
          }
        </div>
        <span>Beskeder</span>
      </a>
    </nav>
  `,
  styles: [`
    .bottom-nav {
      position: fixed;
      bottom: 0;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 390px;
      height: 68px;
      background: #0d1020;
      border-top: 1px solid rgba(0, 200, 200, 0.15);
      display: flex;
      justify-content: space-around;
      align-items: center;
      z-index: 1000;
      padding-bottom: env(safe-area-inset-bottom, 0);
    }
    .bottom-nav__item {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      flex: 1;
      height: 100%;
      text-decoration: none;
      color: #4b5563;
      transition: color 0.2s;
      gap: 3px;
      min-height: 44px;
    }
    .bottom-nav__item svg { width: 22px; height: 22px; }
    .bottom-nav__item span { font-size: 10px; font-weight: 500; letter-spacing: 0.2px; }
    .bottom-nav__item.active { color: #ffffff; }
    .bottom-nav__item--create svg { width: 26px; height: 26px; }
    .bottom-nav__item--create.active { color: #2ecc71; }
    .bottom-nav__icon-wrap {
      position: relative;
      display: inline-flex;
    }
    .bottom-nav__badge {
      position: absolute;
      top: -6px;
      right: -8px;
      background: #e74c3c;
      color: #fff;
      font-size: 9px;
      font-weight: 700;
      min-width: 16px;
      height: 16px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 3px;
    }
  `]
})
export class BottomNavComponent implements OnInit, OnDestroy {
  unreadCount = signal(0);
  private pollSub?: Subscription;

  constructor(
    private messageService: MessageService,
    private auth: AuthService
  ) {
    // Reagér med det samme når bruger logger ind/ud
    effect(() => {
      const userId = this.auth.currentUserId();
      if (userId != null) {
        this.fetchUnread(userId);
      } else {
        this.unreadCount.set(0);
      }
    });
  }

  ngOnInit(): void {
    // Poll hvert 60. sekund
    this.pollSub = interval(60_000).subscribe(() => {
      const userId = this.auth.currentUserId();
      if (userId != null) this.fetchUnread(userId);
    });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  private fetchUnread(userId: number): void {
    this.messageService.getUnreadCount(userId).subscribe({
      next: (count) => this.unreadCount.set(count)
    });
  }

  clearBadge(): void {
    this.unreadCount.set(0);
  }
}

