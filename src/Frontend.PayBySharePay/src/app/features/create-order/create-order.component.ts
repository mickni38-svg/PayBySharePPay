import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { DirectoryService } from '../../core/services/directory.service';
import { AuthService } from '../../core/services/auth.service';
import { DirectoryEntry } from '../../core/models/directory.model';

interface ParticipantVM extends DirectoryEntry {
  initials: string;
  avatarColor: string;
  selected: boolean;
}

interface MerchantVM extends DirectoryEntry {
  initials: string;
  avatarColor: string;
}

const AVATAR_COLORS = [
  '#7c5cbf', '#2e7d32', '#1565c0', '#ad1457',
  '#00838f', '#558b2f', '#4527a0', '#6d4c41'
];


function toInitials(name: string): string {
  return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
}

function avatarColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-order.component.html',
  styleUrl: './create-order.component.scss'
})
export class CreateOrderComponent implements OnInit {

  // â”€â”€ Wizard state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  currentStep = signal(1);
  readonly totalSteps = 4;
  stepError = signal<string | null>(null);

  // â”€â”€ Trin 1: Grundinfo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  title = signal('');
  emoji = signal('');
  message = '';

  // â”€â”€ Trin 2: Spisested â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  merchants = signal<MerchantVM[]>([]);
  selectedMerchant = signal<MerchantVM | null>(null);
  merchantSearch = '';

  filteredMerchants = computed(() => {
    const term = this.merchantSearch.toLowerCase().trim();
    if (!term) return this.merchants();
    return this.merchants().filter(m =>
      m.displayName.toLowerCase().includes(term) ||
      (m.handle?.toLowerCase().includes(term) ?? false)
    );
  });

  // â”€â”€ Trin 3: Deltagere â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  persons = signal<ParticipantVM[]>([]);
  searchTerm = '';
  isLoading = signal(false);

  filtered = computed(() => {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.persons();
    return this.persons().filter(p =>
      p.displayName.toLowerCase().includes(term) ||
      (p.handle?.toLowerCase().includes(term) ?? false)
    );
  });

  selectedParticipants = computed(() => this.persons().filter(p => p.selected));

  // â”€â”€ Submit â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  canSubmit = computed(() =>
    this.title().trim().length > 0 &&
    this.emoji().trim().length > 0 &&
    this.selectedMerchant() !== null &&
    !this.isSubmitting()
  );

  constructor(
    private orderService: OrderService,
    private directoryService: DirectoryService,
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadFriends();
  }

  private loadFriends(): void {
    const userId = this.auth.currentUserId();
    if (userId == null) return;
    this.isLoading.set(true);
    this.directoryService.getFriends(userId).subscribe({
      next: (list) => {
        const persons = list
          .filter(e => e.type === 'Person')
          .map(e => ({
            ...e,
            initials: toInitials(e.displayName),
            avatarColor: avatarColor(e.displayName),
            selected: this.persons().find(p => p.id === e.id)?.selected ?? false
          }));
        this.persons.set(persons);

        const merchants = list
          .filter(e => e.type === 'Merchant')
          .map(e => ({
            ...e,
            initials: toInitials(e.displayName),
            avatarColor: avatarColor(e.displayName)
          }));
        this.merchants.set(merchants);

        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); }
    });
  }

  // â”€â”€ Wizard navigation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  validateCurrentStep(): boolean {
    this.stepError.set(null);
    if (this.currentStep() === 1) {
      if (!this.title().trim()) {
        this.stepError.set('Titel skal udfyldes');
        return false;
      }
      if (!this.emoji().trim()) {
        this.stepError.set('VÃ¦lg en emoji');
        return false;
      }
    }
    if (this.currentStep() === 2) {
      if (!this.selectedMerchant()) {
        this.stepError.set('Du skal vÃ¦lge et spisested for at oprette en gruppebetaling.');
        return false;
      }
    }
    if (this.currentStep() === 3) {
      if (this.selectedParticipants().length === 0) {
        this.stepError.set('VÃ¦lg mindst Ã©n deltager');
        return false;
      }
    }
    return true;
  }

  goNext(): void {
    if (!this.validateCurrentStep()) return;
    if (this.currentStep() < this.totalSteps) {
      this.currentStep.update(s => s + 1);
    }
  }

  goBack(): void {
    if (this.currentStep() > 1) {
      this.stepError.set(null);
      this.currentStep.update(s => s - 1);
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

  // â”€â”€ Merchant
  toggleMerchant(m: MerchantVM): void {
    this.selectedMerchant.update(current => current?.id === m.id ? null : m);
  }

  // â”€â”€ Deltagere â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  togglePerson(p: ParticipantVM): void {
    this.persons.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: !item.selected } : item)
    );
  }

  // â”€â”€ Submit â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  submit(): void {
    this.stepError.set(null);
    if (!this.title().trim() || !this.emoji().trim()) {
      this.stepError.set('Udfyld venligst titel og kategori.');
      return;
    }
    if (!this.selectedMerchant()) {
      this.stepError.set('Du skal vÃ¦lge et spisested for at oprette en gruppebetaling.');
      return;
    }
    if (this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const participantIds = this.selectedParticipants().map(p => p.id);

    this.orderService.createOrder({
      createdByParticipantId: this.auth.currentUserId() ?? 0,
      title: this.title().trim(),
      category: this.emoji().trim() || undefined,
      message: this.message.trim() || undefined,
      merchantParticipantId: this.selectedMerchant()?.id,
      participantIds
    }).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke oprette ordre. PrÃ¸v igen.');
        this.isSubmitting.set(false);
      }
    });
  }
}
