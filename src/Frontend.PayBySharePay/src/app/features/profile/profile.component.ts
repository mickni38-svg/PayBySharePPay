import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService, UpdateProfileRequest } from '../../core/services/profile.service';
import { ParticipantApiDto } from '../../core/models/participant.model';

const NOTIF_KEY = 'sbys_notifications_enabled';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  constructor(
    private readonly auth: AuthService,
    private readonly profileService: ProfileService
  ) {}

  ngOnInit(): void {
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
