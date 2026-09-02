import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { DirectoryService } from '../../core/services/directory.service';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryEntry } from '../../core/models/directory.model';
import { getStaticMerchantLogoUrl } from '../../core/utils/merchant-logo';
import { environment } from '../../../environments/environment';

export interface ParticipantVM extends DirectoryEntry {
  initials: string;
  avatarColor: string;
  selected: boolean;
}

export interface MerchantVM extends DirectoryEntry {
  initials: string;
  avatarColor: string;
  fallbackLogoUrl?: string;
}

interface PreselectedMerchantState {
  id: number;
  displayName: string;
  handle?: string;
  logoUrl: string | null;
  fallbackLogoUrl: string | null;
}

export interface CreateOrderWizardState {
  hostUserId: number | null;
  merchantId: number | null;
  merchant: MerchantVM | null;
  participantIds: number[];
  participants: ParticipantVM[];
}

const AVATAR_COLORS = [
  '#7c5cbf', '#2e7d32', '#1565c0', '#ad1457',
  '#00838f', '#558b2f', '#4527a0', '#6d4c41'
];

function toInitials(name: string): string {
  return name.split(' ').slice(0, 2).map(part => part[0] ?? '').join('').toUpperCase();
}

function avatarColor(name: string): string {
  let hash = 0;
  for (let index = 0; index < name.length; index++) {
    hash = name.charCodeAt(index) + ((hash << 5) - hash);
  }
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-order.component.html',
  styleUrl: './create-order.component.scss'
})
export class CreateOrderComponent implements OnInit {
  currentStep = signal(1);
  readonly totalSteps = 3;
  stepError = signal<string | null>(null);

  hostUserId = signal<number | null>(null);
  selectedMerchant = signal<MerchantVM | null>(null);

  persons = signal<ParticipantVM[]>([]);
  searchTerm = signal('');
  isLoading = signal(false);
  loadError = signal<string | null>(null);

  filtered = computed(() => {
    const term = this.searchTerm().toLocaleLowerCase('da').trim();
    if (!term) return this.persons();

    return this.persons().filter(person =>
      person.displayName.toLocaleLowerCase('da').includes(term) ||
      (person.handle?.toLocaleLowerCase('da').includes(term) ?? false)
    );
  });

  selectedParticipants = computed(() =>
    this.persons().filter(person => person.selected)
  );

  wizardState = computed<CreateOrderWizardState>(() => ({
    hostUserId: this.hostUserId(),
    merchantId: this.selectedMerchant()?.id ?? null,
    merchant: this.selectedMerchant(),
    participantIds: this.selectedParticipants().map(person => person.id),
    participants: this.selectedParticipants()
  }));

  title = signal('');
  emoji = signal('');
  message = '';

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  canContinue = computed(() => {
    if (this.currentStep() === 1) {
      return this.selectedParticipants().length > 0;
    }

    if (this.currentStep() === 2) {
      return this.title().trim().length > 0 && this.emoji().trim().length > 0;
    }

    return true;
  });

  canSubmit = computed(() =>
    this.title().trim().length > 0 &&
    this.emoji().trim().length > 0 &&
    this.selectedMerchant() !== null &&
    this.selectedParticipants().length > 0 &&
    !this.isSubmitting()
  );

  constructor(
    private orderService: OrderService,
    private directoryService: DirectoryService,
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const state = history.state as { merchant?: PreselectedMerchantState };
    const userId = this.auth.currentUserId();

    if (userId == null || !state?.merchant?.id) {
      this.router.navigate(['/home']);
      return;
    }

    this.hostUserId.set(userId);
    this.loadFriends(userId, state.merchant);
  }

  private loadFriends(userId: number, requestedMerchant: PreselectedMerchantState): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.directoryService.getFriends(userId).subscribe({
      next: (list) => {
        const merchantEntry = list.find(entry =>
          entry.type === 'Merchant' && entry.id === requestedMerchant.id
        );

        if (!merchantEntry) {
          this.isLoading.set(false);
          this.router.navigate(['/home']);
          return;
        }

        const staticLogoUrl = getStaticMerchantLogoUrl(merchantEntry);
        const apiLogoUrl = merchantEntry.logoUrl
          ? `${environment.apiUrl}${merchantEntry.logoUrl}`
          : undefined;

        this.selectedMerchant.set({
          ...merchantEntry,
          initials: toInitials(merchantEntry.displayName),
          avatarColor: avatarColor(merchantEntry.displayName),
          logoUrl: staticLogoUrl ?? apiLogoUrl,
          fallbackLogoUrl: staticLogoUrl ? apiLogoUrl : undefined
        });

        const currentSelection = new Set(
          this.selectedParticipants().map(person => person.id)
        );
        const seenIds = new Set<number>();
        const persons = list
          .filter(entry => {
            if (entry.type !== 'Person') return false;
            if (entry.id === userId || entry.id === merchantEntry.id) return false;
            if (seenIds.has(entry.id)) return false;
            seenIds.add(entry.id);
            return true;
          })
          .map(entry => ({
            ...entry,
            initials: toInitials(entry.displayName),
            avatarColor: avatarColor(entry.displayName),
            selected: currentSelection.has(entry.id)
          }));

        this.persons.set(persons);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.loadError.set('Kunne ikke hente deltagere. Prøv igen.');
        this.persons.set([]);
      }
    });
  }

  validateCurrentStep(): boolean {
    this.stepError.set(null);

    if (this.currentStep() === 1 && this.selectedParticipants().length === 0) {
      this.stepError.set('Vælg mindst én deltager');
      return false;
    }

    if (this.currentStep() === 2) {
      if (!this.title().trim()) {
        this.stepError.set('Titel skal udfyldes');
        return false;
      }
      if (!this.emoji().trim()) {
        this.stepError.set('Vælg en emoji');
        return false;
      }
    }

    return true;
  }

  goNext(): void {
    if (!this.validateCurrentStep()) return;
    if (this.currentStep() < this.totalSteps) {
      this.currentStep.update(step => step + 1);
    }
  }

  goBack(): void {
    if (this.currentStep() > 1) {
      this.stepError.set(null);
      this.currentStep.update(step => step - 1);
    }
  }

  goToStep(step: number): void {
    if (step < this.currentStep()) {
      this.stepError.set(null);
      this.currentStep.set(step);
    }
  }

  isStepDone(step: number): boolean {
    return step < this.currentStep();
  }

  onMerchantLogoError(): void {
    this.selectedMerchant.update(merchant =>
      merchant
        ? {
            ...merchant,
            logoUrl: merchant.fallbackLogoUrl,
            fallbackLogoUrl: undefined
          }
        : merchant
    );
  }

  togglePerson(person: ParticipantVM): void {
    const merchantId = this.selectedMerchant()?.id;
    if (person.id === this.hostUserId() || person.id === merchantId) return;

    this.persons.update(list =>
      list.map(item =>
        item.id === person.id ? { ...item, selected: !item.selected } : item
      )
    );
  }

  submit(): void {
    this.stepError.set(null);

    if (!this.title().trim() || !this.emoji().trim()) {
      this.stepError.set('Udfyld venligst titel og kategori.');
      return;
    }
    if (!this.selectedMerchant()) {
      this.stepError.set('Du skal vælge et spisested for at oprette en gruppebetaling.');
      return;
    }
    if (this.selectedParticipants().length === 0) {
      this.stepError.set('Vælg mindst én deltager');
      return;
    }
    if (this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.orderService.createOrder({
      createdByParticipantId: this.hostUserId() ?? 0,
      title: this.title().trim(),
      category: this.emoji().trim() || undefined,
      message: this.message.trim() || undefined,
      merchantParticipantId: this.selectedMerchant()?.id,
      participantIds: this.selectedParticipants().map(person => person.id)
    }).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke oprette ordre. Prøv igen.');
        this.isSubmitting.set(false);
      }
    });
  }
}
