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
  title: string;
  message: string;
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

function createIdempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `${Date.now()}-${Math.random().toString(36).slice(2)}-${Math.random().toString(36).slice(2)}`;
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

  title = signal('');
  message = signal('');
  emoji = signal('');

  wizardState = computed<CreateOrderWizardState>(() => ({
    hostUserId: this.hostUserId(),
    merchantId: this.selectedMerchant()?.id ?? null,
    merchant: this.selectedMerchant(),
    participantIds: this.selectedParticipants().map(person => person.id),
    participants: this.selectedParticipants(),
    title: this.title(),
    message: this.message()
  }));

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  private readonly idempotencyKey = createIdempotencyKey();

  canContinue = computed(() => {
    if (this.currentStep() === 1) {
      return this.selectedParticipants().length > 0;
    }

    if (this.currentStep() === 2) {
      const trimmedTitle = this.title().trim();
      return trimmedTitle.length > 0 &&
        trimmedTitle.length <= 80 &&
        this.message().length <= 500 &&
        this.selectedMerchant() !== null &&
        this.selectedParticipants().length > 0;
    }

    return true;
  });

  canSubmit = computed(() =>
    this.title().trim().length > 0 &&
    this.title().trim().length <= 80 &&
    this.message().length <= 500 &&
    this.hostUserId() !== null &&
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
      if (!this.selectedMerchant()) {
        this.router.navigate(['/home']);
        return false;
      }
      if (this.selectedParticipants().length === 0) {
        this.currentStep.set(1);
        this.stepError.set('Vælg mindst én deltager');
        return false;
      }

      const trimmedTitle = this.title().trim();
      if (!trimmedTitle) {
        this.stepError.set('Titel skal udfyldes');
        return false;
      }
      if (trimmedTitle.length > 80) {
        this.stepError.set('Titel må højst være 80 tegn');
        return false;
      }
      if (this.message().length > 500) {
        this.stepError.set('Besked må højst være 500 tegn');
        return false;
      }

      this.title.set(trimmedTitle);
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

    const hostUserId = this.hostUserId();
    const merchant = this.selectedMerchant();
    const participantIds = this.selectedParticipants().map(person => person.id);
    const trimmedTitle = this.title().trim();

    if (!trimmedTitle || trimmedTitle.length > 80) {
      this.stepError.set('Udfyld venligst en gyldig titel.');
      return;
    }
    if (this.message().length > 500) {
      this.stepError.set('Besked må højst være 500 tegn');
      return;
    }
    if (hostUserId == null) {
      this.stepError.set('Din session er udløbet. Log ind igen.');
      return;
    }
    if (!merchant) {
      this.stepError.set('Du skal vælge et spisested for at oprette en gruppebetaling.');
      return;
    }
    if (participantIds.length === 0) {
      this.stepError.set('Vælg mindst én deltager');
      return;
    }
    if (participantIds.includes(hostUserId) || participantIds.includes(merchant.id) || new Set(participantIds).size !== participantIds.length) {
      this.stepError.set('Deltagerlisten er ugyldig. Gå tilbage og vælg deltagere igen.');
      return;
    }
    if (this.isSubmitting()) return;

    this.title.set(trimmedTitle);
    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.orderService.createOrder({
      createdByParticipantId: hostUserId,
      title: trimmedTitle,
      category: this.emoji().trim() || undefined,
      message: this.message() || undefined,
      merchantParticipantId: merchant.id,
      participantIds,
      idempotencyKey: this.idempotencyKey
    }).subscribe({
      next: (created) => {
        this.router.navigate(['/orders', created.id]);
      },
      error: (error) => {
        const apiMessage = error?.error?.message ?? error?.error?.title;
        this.errorMessage.set(apiMessage || 'Kunne ikke oprette gruppebetalingen. Prøv igen.');
        this.isSubmitting.set(false);
      }
    });
  }
}
