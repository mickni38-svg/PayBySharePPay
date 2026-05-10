import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { DirectoryEntry } from '../../core/models/directory.model';

interface ActionCard {
  label: string;
  subtitle: string;
  icon: string;
  route: string;
  iconBg: string;
  accent: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  actionCards: ActionCard[] = [
    { label: 'Overblik',  subtitle: 'Se regninger',      route: '/orders',            accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',   icon: 'chart' },
    { label: 'Opret',     subtitle: 'Ny gruppebetaling', route: '/orders/create',      accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)',  icon: 'plus'  },
    { label: 'Brugere',   subtitle: 'Find personer',     route: '/find-participants',  accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',   icon: 'users' },
    { label: 'Beskeder',  subtitle: 'Dine beskeder',     route: '/messages',           accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)',  icon: 'chat'  },
  ];

  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);

  constructor(readonly auth: AuthService, private directory: DirectoryService) {}

  ngOnInit(): void {
    this.directory.search('').subscribe({
      next: (list) => this.persons.set(list.filter(e => e.type === 'Person')),
      error: () => {}
    });
  }

  devLogin(): void {
    if (!this.selectedEmail) return;
    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.login(this.selectedEmail).subscribe({
      next: () => this.loginLoading.set(false),
      error: () => {
        this.loginError.set('Login fejlede – prøv igen.');
        this.loginLoading.set(false);
      }
    });
  }
}
