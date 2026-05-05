import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgFor } from '@angular/common';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-bottom-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, NgFor],
  template: `
    <nav class="bottom-nav">
      <a
        *ngFor="let item of navItems"
        [routerLink]="item.path"
        routerLinkActive="active"
        [routerLinkActiveOptions]="{ exact: item.path === '/home' }"
        class="bottom-nav__item"
      >
        <span class="bottom-nav__icon" [innerHTML]="item.icon"></span>
        <span class="bottom-nav__label">{{ item.label }}</span>
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
      background: #10152b;
      border-top: 1px solid rgba(255,255,255,0.08);
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
      color: #6b7280;
      transition: color 0.2s ease;
      gap: 4px;
      min-height: 44px;
    }

    .bottom-nav__item.active {
      color: #ffffff;
    }

    .bottom-nav__icon {
      font-size: 22px;
      line-height: 1;
    }

    .bottom-nav__label {
      font-size: 11px;
      font-weight: 500;
      letter-spacing: 0.2px;
    }
  `]
})
export class BottomNavComponent {
  navItems: NavItem[] = [
    { label: 'Forside',   path: '/home',               icon: '🏠' },
    { label: 'Overblik',  path: '/orders',             icon: '⊞' },
    { label: 'Opret',     path: '/orders/create',      icon: '⊕' },
    { label: 'Brugere',   path: '/find-participants',  icon: '♡' },
    { label: 'Beskeder',  path: '/messages',           icon: '💬' }
  ];
}

