import { inject } from '@angular/core';
import { Router, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'login',
    redirectTo: () => inject(Router).createUrlTree(['/profile'], {
      queryParams: { mode: 'login' }
    })
  },
  {
    path: 'register',
    loadComponent: () => import('./features/onboarding/onboarding.component').then(m => m.OnboardingComponent)
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'find-participants',
    canActivate: [authGuard],
    loadComponent: () => import('./features/find-participants/find-participants.component').then(m => m.FindParticipantsComponent)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/orders.component').then(m => m.OrdersComponent)
  },
  {
    path: 'orders/create',
    canActivate: [authGuard],
    loadComponent: () => import('./features/create-order/create-order.component').then(m => m.CreateOrderComponent)
  },
  {
    path: 'orders/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/order-detail/order-detail.component').then(m => m.OrderDetailComponent)
  },
  {
    path: 'messages',
    canActivate: [authGuard],
    loadComponent: () => import('./features/messages/messages.component').then(m => m.MessagesComponent)
  },
  {
    path: 'profile',
    canActivate: [
      (route) => route.queryParamMap.get('mode') === 'register'
        ? inject(Router).createUrlTree(['/register'])
        : true
    ],
    runGuardsAndResolvers: 'paramsOrQueryParamsChange',
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'pending-participants',
    canActivate: [authGuard],
    loadComponent: () => import('./features/pending-participants/pending-participants.component').then(m => m.PendingParticipantsComponent)
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];
