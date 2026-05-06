import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { DirectoryEntry } from '../../core/models/directory.model';

interface ActionCard {
  label: string;
  icon: string;
  route: string;
  iconBg: string;
  borderColor: string;
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
    { label: 'Overblik',  icon: 'chart',  route: '/orders',           iconBg: 'rgba(240,165,0,0.12)',  borderColor: '#f0a500' },
    { label: 'Opret',     icon: 'plus',   route: '/orders/create',     iconBg: 'rgba(46,204,113,0.12)', borderColor: '#2ecc71' },
    { label: 'Brugere',   icon: 'users',  route: '/find-participants', iconBg: 'rgba(0,200,255,0.12)',  borderColor: '#00c8ff' },
    { label: 'Beskeder',  icon: 'chat',   route: '/messages',          iconBg: 'rgba(155,89,182,0.12)', borderColor: '#9b59b6' },
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
