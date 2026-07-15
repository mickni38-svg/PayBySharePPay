import { Component, OnInit, OnDestroy, effect } from '@angular/core';
import { RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MessageService } from '../../core/services/message.service';
import { AuthService } from '../../core/services/auth.service';
import { Subscription, interval } from 'rxjs';
import { filter } from 'rxjs/operators';

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
      <a routerLink="/find-participants" routerLinkActive="active" class="bottom-nav__item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
          <circle cx="9" cy="7" r="4"/>
          <line x1="19" y1="8" x2="19" y2="14"/>
          <line x1="16" y1="11" x2="22" y2="11"/>
        </svg>
        <span>Venner</span>
      </a>
      <a routerLink="/messages" routerLinkActive="active" class="bottom-nav__item">
        <div class="bottom-nav__icon-wrap">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
            <circle cx="9" cy="10" r="1" fill="currentColor"/>
            <circle cx="12" cy="10" r="1" fill="currentColor"/>
            <circle cx="15" cy="10" r="1" fill="currentColor"/>
          </svg>
          @if (messageService.unreadCount() > 0) {
            <span class="bottom-nav__badge">{{ messageService.unreadCount() > 9 ? '9+' : messageService.unreadCount() }}</span>
          }
        </div>
        <span>Beskeder</span>
      </a>
      <a routerLink="/profile" routerLinkActive="active" class="bottom-nav__item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="8" r="4"/>
          <path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
        </svg>
        <span>Profil</span>
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
      background: var(--color-nav-bg, #0a0c1a);
      border-top: 1px solid var(--color-nav-border, rgba(34,197,94,0.18));
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
      color: var(--color-nav-item, #4b5563);
      transition: color 0.2s;
      gap: 3px;
      min-height: 44px;
    }
    .bottom-nav__item svg { width: 22px; height: 22px; }
    .bottom-nav__item span { font-size: 10px; font-weight: 500; letter-spacing: 0.2px; }
    .bottom-nav__item.active { color: var(--color-nav-item-active, #FFFFFF); }
    .bottom-nav__item--create svg { width: 26px; height: 26px; }
    .bottom-nav__item--create.active { color: var(--color-nav-create-active, #22C55E); }
    .bottom-nav__icon-wrap {
      position: relative;
      display: inline-flex;
    }
    .bottom-nav__badge {
      position: absolute;
      top: -6px;
      right: -8px;
      background: var(--color-badge-bg, #e74c3c);
      color: var(--color-badge-text, #fff);
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
  private pollSub?: Subscription;
  private routerSub?: Subscription;

  constructor(
    readonly messageService: MessageService,
    private auth: AuthService,
    private router: Router
  ) {
    effect(() => {
      const userId = this.auth.currentUserId();
      if (userId != null) {
        this.messageService.refreshUnread(userId);
      } else {
        this.messageService.resetUnread();
      }
    });
  }

  ngOnInit(): void {
    // Poll hvert 60. sekund
    this.pollSub = interval(60_000).subscribe(() => {
      const userId = this.auth.currentUserId();
      if (userId != null) this.messageService.refreshUnread(userId);
    });

    // Nulstil badge synkront når brugeren navigerer til /messages
    this.routerSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const nav = e as NavigationEnd;
      if (nav.urlAfterRedirects.startsWith('/messages')) {
        this.messageService.resetUnread();
      }
    });
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
    this.routerSub?.unsubscribe();
  }
}
