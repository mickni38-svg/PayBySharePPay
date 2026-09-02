import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService, UpdateProfileRequest, VippsTestPersonDto } from '../../core/services/profile.service';
import { ParticipantApiDto } from '../../core/models/participant.model';
import { ThemeService } from '../../core/services/theme.service';
import { DirectoryService } from '../../core/services/directory.service';
import { DevService } from '../../core/services/dev.service';
import { DirectoryEntry } from '../../core/models/directory.model';

const NOTIF_KEY = 'sbys_notifications_enabled';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  isLoading = signal(true);
  isSaving = signal(false);
  saveSuccess = signal(false);
  saveError = signal(false);

  name = signal('');
  email = signal('');
  phone = signal('');
  notificationsEnabled = signal(true);

  vippsTestPersons = signal<VippsTestPersonDto[]>([]);
  selectedVippsTestUserId = signal<number | null>(null);
  vippsSaving = signal(false);
  vippsSaveSuccess = signal(false);

  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  loginError = signal<string | null>(null);
  loginLoading = signal(false);
  resetLoading = signal(false);
  resetMessage = signal<string | null>(null);
  devPanelOpen = signal(false);

  constructor(
    readonly auth: AuthService,
    private readonly router: Router,
    private readonly profileService: ProfileService,
    protected readonly themeService: ThemeService,
    private readonly directory: DirectoryService,
    private readonly devService: DevService
  ) {}

  ngOnInit(): void {
    this.directory.search('').subscribe({
      next: (list) => this.persons.set(list.filter(entry => entry.type === 'Person')),
      error: () => this.persons.set([])
    });

    const stored = localStorage.getItem(NOTIF_KEY);
    this.notificationsEnabled.set(stored !== 'false');

    const userId = this.auth.currentUserId();
    if (!userId) { this.isLoading.set(false); return; }

    this.profileService.getProfile(userId).subscribe({
      next: (p: ParticipantApiDto) => {
        this.name.set(p.name);
        this.email.set(p.email ?? '');
        this.phone.set(p.phone ?? '');
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });

    this.profileService.getVippsTestPersons().subscribe({
      next: (persons) => {
        this.vippsTestPersons.set(persons);
        const myMapping = persons.find(p => p.mappedByParticipantId === userId);
        this.selectedVippsTestUserId.set(myMapping?.id ?? null);
      }
    });
  }

  isVippsPersonDisabled(person: VippsTestPersonDto): boolean {
    const userId = this.auth.currentUserId();
    return person.mappedByParticipantId != null && person.mappedByParticipantId !== userId;
  }

  saveVippsMapping(): void {
    const userId = this.auth.currentUserId();
    if (!userId) return;
    this.vippsSaving.set(true);
    this.vippsSaveSuccess.set(false);
    this.profileService.setVippsTestUser(userId, this.selectedVippsTestUserId()).subscribe({
      next: () => {
        this.vippsSaving.set(false);
        this.vippsSaveSuccess.set(true);
        setTimeout(() => this.vippsSaveSuccess.set(false), 3000);
        // Opdatér local state så disable-logikken er korrekt
        this.profileService.getVippsTestPersons().subscribe(persons => this.vippsTestPersons.set(persons));
      },
      error: () => {
        this.vippsSaving.set(false);
      }
    });
  }

  toggleDevPanel(): void {
    this.devPanelOpen.update(open => !open);
  }

  devLogin(): void {
    if (!this.selectedEmail) return;

    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.login(this.selectedEmail, '').subscribe({
      next: () => {
        this.loginLoading.set(false);
        this.router.navigate(['/home']);
      },
      error: (error) => {
        this.loginLoading.set(false);
        this.loginError.set(error?.error?.message ?? 'Login fejlede');
      }
    });
  }

  devReset(): void {
    if (!confirm('Er du sikker? Dette sletter ALLE ordre og beskeder i databasen.')) return;

    this.resetLoading.set(true);
    this.resetMessage.set(null);
    this.devService.resetData().subscribe({
      next: () => {
        this.resetLoading.set(false);
        this.resetMessage.set('Alle ordre og beskeder er slettet.');
        setTimeout(() => this.resetMessage.set(null), 4000);
      },
      error: () => {
        this.resetLoading.set(false);
        this.resetMessage.set('Fejl ved sletning – prøv igen.');
      }
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  toggleNotifications(): void {
    const next = !this.notificationsEnabled();
    this.notificationsEnabled.set(next);
    localStorage.setItem(NOTIF_KEY, next ? 'true' : 'false');
  }

  save(): void {
    if (!this.name().trim()) return;

    const userId = this.auth.currentUserId();
    if (!userId) return;

    this.isSaving.set(true);
    this.saveSuccess.set(false);
    this.saveError.set(false);

    const request: UpdateProfileRequest = {
      name: this.name().trim(),
      email: this.email() || undefined,
      phone: this.phone() || undefined
    };

    this.profileService.updateProfile(userId, request).subscribe({
      next: () => {
        this.auth.updateStoredName(request.name);
        this.isSaving.set(false);
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 3000);
      },
      error: () => {
        this.isSaving.set(false);
        this.saveError.set(true);
      }
    });
  }
}
