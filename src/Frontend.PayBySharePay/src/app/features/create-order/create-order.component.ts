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
  { key: 'sushi',   icon: '🍣', label: 'Sushi' },
  { key: 'pizza',   icon: '🍕', label: 'Pizza' },
  { key: 'burger',  icon: '🍔', label: 'Burger' },
  { key: 'drinks',  icon: '🍺', label: 'Drinks' },
  { key: 'tacos',   icon: '🌮', label: 'Tacos' },
  { key: 'ramen',   icon: '🍜', label: 'Ramen' },
  { key: 'kebab',   icon: '🥙', label: 'Kebab' },
  { key: 'chicken', icon: '🍗', label: 'Kylling' },
  { key: 'salad',   icon: '🥗', label: 'Salat' },
  { key: 'dessert', icon: '🍰', label: 'Dessert' },
  { key: 'coffee',  icon: '☕', label: 'Kaffe' },
  { key: 'other',   icon: '📦', label: 'Andet' },
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

  title = signal('');
  selectedCategory = signal<string | null>(null);
  message = '';
  searchTerm = '';

  categories = CATEGORIES;

  persons = signal<ParticipantVM[]>([]);
  merchants = signal<MerchantVM[]>([]);
  selectedMerchant = signal<MerchantVM | null>(null);
  isLoading = signal(false);
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  filtered = computed(() => {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.persons();
    return this.persons().filter(p =>
      p.displayName.toLowerCase().includes(term) ||
      (p.handle?.toLowerCase().includes(term) ?? false)
    );
  });

  selectedParticipants = computed(() => this.persons().filter(p => p.selected));

  categoryLabel = computed(() =>
    this.categories.find(c => c.key === this.selectedCategory())?.label ?? ''
  );

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

  onSearch(): void {
    // Filtrering sker lokalt via filtered() computed
  }

  toggleCategory(key: string): void {
    this.selectedCategory.update(current => current === key ? null : key);
  }

  toggleMerchant(m: MerchantVM): void {
    this.selectedMerchant.update(current => current?.id === m.id ? null : m);
  }

  togglePerson(p: ParticipantVM): void {
    this.persons.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: !item.selected } : item)
    );
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }

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
      next: (order) => {
        this.router.navigate(['/orders', order.id]);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke oprette ordre. Prøv igen.');
        this.isSubmitting.set(false);
      }
    });
  }
}
