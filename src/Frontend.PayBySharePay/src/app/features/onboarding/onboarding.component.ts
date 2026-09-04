import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryService } from '../../core/services/directory.service';
import { FriendService } from '../../core/services/friend.service';
import { DirectoryEntry } from '../../core/models/directory.model';

type OnboardingStep = 1 | 2 | 3 | 4;

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './onboarding.component.html',
  styleUrl: './onboarding.component.scss'
})
export class OnboardingComponent {
  step = signal<OnboardingStep>(1);
  accountCreated = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  personName = '';
  personEmail = '';
  personPhone = '';
  personPassword = '';
  personPasswordConfirm = '';

  searchTerm = signal('');
  directoryEntries = signal<DirectoryEntry[]>([]);
  selectedFriendIds = signal<number[]>([]);
  selectedMerchantIds = signal<number[]>([]);

  readonly friendCandidates = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    return this.directoryEntries().filter(entry =>
      entry.type === 'Person' &&
      (!query || entry.displayName.toLowerCase().includes(query) || (entry.handle ?? '').toLowerCase().includes(query))
    );
  });

  readonly merchantCandidates = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    return this.directoryEntries().filter(entry =>
      entry.type === 'Merchant' &&
      (!query || entry.displayName.toLowerCase().includes(query) || (entry.subtitle ?? '').toLowerCase().includes(query))
    );
  });

  readonly selectedFriends = computed(() => {
    const ids = new Set(this.selectedFriendIds());
    return this.directoryEntries().filter(entry => entry.type === 'Person' && ids.has(entry.id));
  });

  readonly selectedMerchants = computed(() => {
    const ids = new Set(this.selectedMerchantIds());
    return this.directoryEntries().filter(entry => entry.type === 'Merchant' && ids.has(entry.id));
  });

  constructor(
    readonly auth: AuthService,
    private readonly directory: DirectoryService,
    private readonly friends: FriendService,
    private readonly router: Router
  ) {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/home']);
    }
  }

  canContinueProfile(): boolean {
    return !!this.personName.trim() &&
      !!this.personEmail.trim() &&
      this.personPassword.length >= 6 &&
      this.personPassword === this.personPasswordConfirm;
  }

  next(): void {
    this.errorMessage.set(null);

    if (this.step() === 1) {
      if (!this.canContinueProfile()) return;
      if (this.accountCreated()) {
        this.step.set(2);
        return;
      }
      this.createAccountAndContinue();
      return;
    }

    if (this.step() === 2) {
      this.searchTerm.set('');
      this.step.set(3);
      return;
    }

    if (this.step() === 3) {
      this.searchTerm.set('');
      this.step.set(4);
    }
  }

  back(): void {
    const current = this.step();
    if (current > 1) this.step.set((current - 1) as OnboardingStep);
    this.errorMessage.set(null);
  }

  toggleFriend(id: number): void {
    this.selectedFriendIds.update(ids => ids.includes(id) ? ids.filter(x => x !== id) : [...ids, id]);
  }

  toggleMerchant(id: number): void {
    this.selectedMerchantIds.update(ids => ids.includes(id) ? ids.filter(x => x !== id) : [...ids, id]);
  }

  isFriendSelected(id: number): boolean {
    return this.selectedFriendIds().includes(id);
  }

  isMerchantSelected(id: number): boolean {
    return this.selectedMerchantIds().includes(id);
  }

  complete(): void {
    const userId = this.auth.currentUserId();
    if (!userId || this.isLoading()) return;

    const selectedIds = [...this.selectedFriendIds(), ...this.selectedMerchantIds()];
    if (selectedIds.length === 0) {
      this.router.navigate(['/home']);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const requests = selectedIds.map(receiverId =>
      this.friends.addFriend({ initiatorId: userId, receiverId })
    );

    forkJoin(requests).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/home']);
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Profilen er oprettet, men en eller flere relationer kunne ikke gemmes. Prøv igen.');
      }
    });
  }

  private createAccountAndContinue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.auth.register({
      name: this.personName.trim(),
      email: this.personEmail.trim(),
      phone: this.personPhone.trim() || undefined,
      password: this.personPassword
    }).subscribe({
      next: () => {
        this.accountCreated.set(true);
        this.isLoading.set(false);
        this.step.set(2);
        this.loadDirectory();
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.status === 409
          ? 'Der findes allerede en konto med denne email.'
          : 'Kontoen kunne ikke oprettes. Prøv igen.');
      }
    });
  }

  private loadDirectory(): void {
    const userId = this.auth.currentUserId();
    if (!userId) return;

    this.isLoading.set(true);
    this.directory.search('', userId).subscribe({
      next: entries => {
        this.directoryEntries.set(entries.filter(entry => entry.id !== userId));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Venner og spisesteder kunne ikke hentes. Du kan stadig fortsætte uden valg.');
      }
    });
  }
}
