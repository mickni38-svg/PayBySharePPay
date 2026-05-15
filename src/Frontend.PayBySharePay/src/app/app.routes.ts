import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'find-participants',
    loadComponent: () => import('./features/find-participants/find-participants.component').then(m => m.FindParticipantsComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/orders/orders.component').then(m => m.OrdersComponent)
  },
  {
    path: 'orders/create',
    loadComponent: () => import('./features/create-order/create-order.component').then(m => m.CreateOrderComponent)
  },
  {
    path: 'orders/:id',
    loadComponent: () => import('./features/order-detail/order-detail.component').then(m => m.OrderDetailComponent)
  },
  {
    path: 'messages',
    loadComponent: () => import('./features/messages/messages.component').then(m => m.MessagesComponent)
  },
  {
    path: 'activity',
    loadComponent: () => import('./features/activity/activity.component').then(m => m.ActivityComponent)
  },
  {
    path: 'pending-participants',
    loadComponent: () => import('./features/pending-participants/pending-participants.component').then(m => m.PendingParticipantsComponent)
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];
