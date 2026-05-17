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

const CATEGORIES = [
  { key: 'pizza',   icon: '🍕', label: 'Pizza' },
  { key: 'burger',  icon: '🍔', label: 'Burger' },
  { key: 'sushi',   icon: '🍣', label: 'Sushi' },
  { key: 'tacos',   icon: '🌮', label: 'Tacos' },
  { key: 'drinks',  icon: '🍺', label: 'Drikke' },
  { key: 'ramen',   icon: '🍜', label: 'Ramen' },
  { key: 'kebab',   icon: '🥙', label: 'Kebab' },
  { key: 'chicken', icon: '🍗', label: 'Kylling' },
  { key: 'salad',   icon: '🥗', label: 'Salat' },
  { key: 'dessert', icon: '🍰', label: 'Dessert' },
  { key: 'coffee',  icon: '☕', label: 'Kaffe' },
  { key: 'thai',    icon: '🍛', label: 'Thai' },
  { key: 'indian',  icon: '🫕', label: 'Indisk' },
  { key: 'chinese', icon: '🥡', label: 'Kinesisk' },
  { key: 'italian', icon: '🍝', label: 'Italiensk' },
  { key: 'mexican', icon: '🌯', label: 'Mexicansk' },
  { key: 'greek',   icon: '🫒', label: 'Græsk' },
  { key: 'american',icon: '🥩', label: 'Amerikansk' },
  { key: 'vegan',   icon: '🌱', label: 'Vegansk' },
  { key: 'snacks',  icon: '🍿', label: 'Snacks' },
  { key: 'other',   icon: '📦', label: 'Andet' },
];

const SUGGESTED_CATEGORY_KEYS = ['pizza', 'burger', 'sushi', 'tacos', 'drinks'];

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

  // ── Wizard state ─────────────────────────────────────────────────────────
  currentStep = signal(1);
  readonly totalSteps = 4;
  stepError = signal<string | null>(null);

  // ── Trin 1: Grundinfo ────────────────────────────────────────────────────
  title = signal('');
  selectedCategory = signal<string | null>(null);
  message = '';
  categorySearch = '';
  showAllCategories = signal(false);

  categories = CATEGORIES;

  suggestedCategories = computed(() => {
    const term = this.categorySearch.toLowerCase().trim();
    if (term) {
      return this.categories.filter(c =>
        c.label.toLowerCase().includes(term) || c.key.includes(term)
      );
    }
    return this.categories.filter(c => SUGGESTED_CATEGORY_KEYS.includes(c.key));
  });

  filteredAllCategories = computed(() => {
    const term = this.categorySearch.toLowerCase().trim();
    if (!term) return this.categories;
    return this.categories.filter(c =>
      c.label.toLowerCase().includes(term) || c.key.includes(term)
    );
  });

  categoryLabel = computed(() =>
    this.categories.find(c => c.key === this.selectedCategory())?.label ?? ''
  );

  categoryIcon = computed(() =>
    this.categories.find(c => c.key === this.selectedCategory())?.icon ?? ''
  );

  // ── Trin 2: Spisested ────────────────────────────────────────────────────
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

  // ── Trin 3: Deltagere ────────────────────────────────────────────────────
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

  // ── Submit ────────────────────────────────────────────────────────────────
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  canSubmit = computed(() =>
    this.title().trim().length > 0 && !this.isSubmitting()
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

  // ── Wizard navigation ─────────────────────────────────────────────────────
  validateCurrentStep(): boolean {
    this.stepError.set(null);
    if (this.currentStep() === 1) {
      if (!this.title().trim()) {
        this.stepError.set('Titel skal udfyldes');
        return false;
      }
    }
    if (this.currentStep() === 3) {
      if (this.selectedParticipants().length === 0) {
        this.stepError.set('Vælg mindst én deltager');
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

  goToStep(step: number): void {
    if (step < this.currentStep()) {
      this.stepError.set(null);
      this.currentStep.set(step);
    }
  }

  isStepDone(step: number): boolean {
    return step < this.currentStep();
  }

  // ── Kategori ──────────────────────────────────────────────────────────────
  toggleCategory(key: string): void {
    this.selectedCategory.update(current => current === key ? null : key);
    this.showAllCategories.set(false);
  }

  openAllCategories(): void {
    this.categorySearch = '';
    this.showAllCategories.set(true);
  }

  closeAllCategories(): void {
    this.showAllCategories.set(false);
  }

  // ── Merchant ──────────────────────────────────────────────────────────────
  toggleMerchant(m: MerchantVM): void {
    this.selectedMerchant.update(current => current?.id === m.id ? null : m);
  }

  // ── Deltagere ─────────────────────────────────────────────────────────────
  togglePerson(p: ParticipantVM): void {
    this.persons.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: !item.selected } : item)
    );
  }

  // ── Submit ────────────────────────────────────────────────────────────────
  submit(): void {
    if (!this.canSubmit()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const participantIds = this.selectedParticipants().map(p => p.id);

    this.orderService.createOrder({
      createdByParticipantId: this.auth.currentUserId() ?? 0,
      title: this.title().trim(),
      category: this.selectedCategory() ?? undefined,
      message: this.message.trim() || undefined,
      merchantParticipantId: this.selectedMerchant()?.id,
      participantIds
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
