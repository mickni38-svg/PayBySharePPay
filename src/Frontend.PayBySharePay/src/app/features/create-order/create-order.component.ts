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

const AVATAR_COLORS = [
  '#7c5cbf', '#2e7d32', '#1565c0', '#ad1457',
  '#00838f', '#558b2f', '#4527a0', '#6d4c41'
];

const CATEGORIES = [
  { key: 'sushi',   icon: '🍣', label: 'Sushi' },
  { key: 'pizza',   icon: '🍕', label: 'Pizza' },
  { key: 'burger',  icon: '🍔', label: 'Burger' },
  { key: 'drinks',  icon: '🍺', label: 'Drinks' },
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
  template: `
    <div class="create">

      <header class="create__header">
        <button class="create__back" (click)="goBack()">‹</button>
        <h1 class="create__title">Ny gruppebetaling</h1>
      </header>

      <!-- Titel -->
      <div class="create__section">
        <label class="create__label">Titel</label>
        <input
          class="create__input"
          type="text"
          placeholder="fx pizza aften"
          [(ngModel)]="title"
          maxlength="80"
        />
      </div>

      <!-- Kategori -->
      <div class="create__section">
        <label class="create__label">Kategori</label>
        <div class="create__cats">
          @for (cat of categories; track cat.key) {
            <button
              class="cat-chip"
              [class.cat-chip--active]="selectedCategory === cat.key"
              (click)="toggleCategory(cat.key)"
            >
              <span>{{ cat.icon }}</span>
              <span>{{ cat.label }}</span>
            </button>
          }
        </div>
      </div>

      <!-- Besked -->
      <div class="create__section">
        <label class="create__label">Besked (valgfri)</label>
        <textarea
          class="create__textarea"
          placeholder="Skriv en kort besked til deltagerne…"
          [(ngModel)]="message"
          maxlength="200"
          rows="3"
        ></textarea>
      </div>

      <!-- Deltagere -->
      <div class="create__section">
        <label class="create__label">Tilføj deltagere</label>
        <div class="create__search-wrap">
          <input
            class="create__input"
            type="search"
            placeholder="Søg navn…"
            [(ngModel)]="searchTerm"
            (ngModelChange)="onSearch()"
          />
        </div>

        @if (isLoading()) {
          <p class="create__state">Henter…</p>
        } @else if (filtered().length > 0) {
          <div class="create__list">
            @for (p of filtered(); track p.id) {
              <div class="row" [class.row--selected]="p.selected" (click)="togglePerson(p)">
                <div class="row__avatar" [style.background]="p.avatarColor">{{ p.initials }}</div>
                <div class="row__info">
                  <span class="row__name">{{ p.displayName }}</span>
                  @if (p.handle) { <span class="row__sub">{{ p.handle }}</span> }
                </div>
                <div class="row__check" [class.row__check--on]="p.selected">
                  @if (p.selected) {
                    <svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.8"
                         stroke-linecap="round" stroke-linejoin="round">
                      <polyline points="20 6 9 17 4 12"/>
                    </svg>
                  }
                </div>
              </div>
            }
          </div>
        }

        <!-- Valgte deltagere -->
        @if (selectedParticipants().length > 0) {
          <div class="create__selected">
            @for (p of selectedParticipants(); track p.id) {
              <div class="tag">
                <span>{{ p.displayName }}</span>
                <button class="tag__remove" (click)="removeSelected(p)">×</button>
              </div>
            }
          </div>
        }
      </div>

      @if (errorMessage()) {
        <p class="create__error">{{ errorMessage() }}</p>
      }

      <!-- Opret-knap -->
      <div class="create__footer">
        <button
          class="create__submit"
          [disabled]="!canSubmit() || isSubmitting()"
          (click)="submit()"
        >
          @if (isSubmitting()) { Opretter… } @else { Opret gruppebetaling }
        </button>
      </div>

    </div>
  `,
  styles: [`
    .create {
      display: flex;
      flex-direction: column;
      min-height: 100%;
      background: #0a0e1a;
      color: #fff;
      font-family: -apple-system, BlinkMacSystemFont, 'SF Pro Text', 'Segoe UI', sans-serif;
      padding-bottom: 100px;
    }

    .create__header {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 16px 16px 8px;
    }

    .create__back {
      background: transparent;
      border: none;
      color: #60a5fa;
      font-size: 28px;
      line-height: 1;
      cursor: pointer;
      padding: 0 6px 0 0;
      min-width: 32px;
      min-height: 44px;
    }

    .create__title {
      margin: 0;
      font-size: 20px;
      font-weight: 700;
    }

    .create__section {
      padding: 8px 16px;
    }

    .create__label {
      display: block;
      font-size: 12px;
      color: #6b7280;
      margin-bottom: 6px;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .create__input {
      width: 100%;
      box-sizing: border-box;
      background: #111827;
      border: 1px solid #1f2937;
      border-radius: 10px;
      padding: 12px 14px;
      color: #fff;
      font-size: 15px;
      outline: none;
      min-height: 44px;
      -webkit-appearance: none;
      transition: border-color 0.15s;
    }

    .create__input:focus { border-color: #3b82f6; }
    .create__input::placeholder { color: #4b5563; }

    .create__textarea {
      width: 100%;
      box-sizing: border-box;
      background: #111827;
      border: 1px solid #1f2937;
      border-radius: 10px;
      padding: 12px 14px;
      color: #fff;
      font-size: 15px;
      outline: none;
      resize: none;
      font-family: inherit;
      transition: border-color 0.15s;
    }

    .create__textarea:focus { border-color: #3b82f6; }
    .create__textarea::placeholder { color: #4b5563; }

    /* Kategori-chips */
    .create__cats {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .cat-chip {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 14px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.12);
      background: transparent;
      color: #9ca3af;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.15s;
      min-height: 40px;
    }

    .cat-chip--active {
      border-color: #2ecc71;
      color: #fff;
      background: rgba(46,204,113,0.1);
    }

    .create__search-wrap { margin-bottom: 6px; }

    .create__state {
      color: #555;
      font-size: 14px;
      padding: 8px 0;
      margin: 0;
    }

    /* Deltager-liste */
    .create__list {
      border-radius: 10px;
      overflow: hidden;
      border: 1px solid #1f2937;
      margin-bottom: 8px;
    }

    .row {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 12px;
      min-height: 52px;
      cursor: pointer;
      background: #111827;
      border-bottom: 1px solid #1f2937;
      transition: background 0.1s;
    }

    .row:last-child { border-bottom: none; }
    .row:active { background: #1a2236; }

    .row__avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 700;
      color: rgba(255,255,255,0.9);
      flex-shrink: 0;
    }

    .row__info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 1px;
      min-width: 0;
    }

    .row__name {
      font-size: 15px;
      color: #fff;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .row__sub { font-size: 12px; color: #6b7280; }

    .row__check {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      border: 1.5px solid rgba(255,255,255,0.3);
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background 0.15s, border-color 0.15s;
    }

    .row__check--on { background: #2e7d32; border-color: #2e7d32; }
    .row__check svg { width: 12px; height: 12px; }

    /* Valgte tags */
    .create__selected {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      margin-top: 6px;
    }

    .tag {
      display: flex;
      align-items: center;
      gap: 4px;
      background: rgba(59,130,246,0.15);
      border: 1px solid rgba(59,130,246,0.3);
      color: #60a5fa;
      border-radius: 20px;
      padding: 4px 10px 4px 12px;
      font-size: 13px;
      font-weight: 500;
    }

    .tag__remove {
      background: transparent;
      border: none;
      color: #60a5fa;
      cursor: pointer;
      font-size: 16px;
      line-height: 1;
      padding: 0 0 0 2px;
    }

    .create__error {
      color: #ff453a;
      font-size: 14px;
      padding: 0 16px;
      margin: 0;
    }

    /* Opret-knap */
    .create__footer {
      position: fixed;
      bottom: 68px;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 390px;
      padding: 10px 16px 12px;
      background: linear-gradient(to top, #0a0e1a 70%, transparent);
    }

    .create__submit {
      width: 100%;
      background: #1a3a6e;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 15px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      min-height: 52px;
      transition: background 0.15s, opacity 0.15s;
    }

    .create__submit:not(:disabled):active { background: #1e4a8a; }

    .create__submit:disabled {
      background: #1a1a2e;
      color: #4b5563;
      cursor: default;
    }
  `]
})
export class CreateOrderComponent implements OnInit {

  title = '';
  selectedCategory: string | null = null;
  message = '';
  searchTerm = '';

  categories = CATEGORIES;

  persons = signal<ParticipantVM[]>([]);
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

  canSubmit = computed(() =>
    this.title.trim().length > 0 && !this.isSubmitting()
  );

  constructor(
    private orderService: OrderService,
    private directoryService: DirectoryService,
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadPersons('');
  }

  private loadPersons(query: string): void {
    this.isLoading.set(true);
    this.directoryService.search(query).subscribe({
      next: (list) => {
        const persons = list
          .filter(e => e.type === 'Person' && e.id !== this.auth.currentUserId())
          .map(e => ({
            ...e,
            initials: toInitials(e.displayName),
            avatarColor: avatarColor(e.displayName),
            selected: this.persons().find(p => p.id === e.id)?.selected ?? false
          }));
        this.persons.set(persons);
        this.isLoading.set(false);
      },
      error: () => { this.isLoading.set(false); }
    });
  }

  onSearch(): void {
    this.loadPersons(this.searchTerm.trim());
  }

  toggleCategory(key: string): void {
    this.selectedCategory = this.selectedCategory === key ? null : key;
  }

  togglePerson(p: ParticipantVM): void {
    this.persons.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: !item.selected } : item)
    );
  }

  removeSelected(p: ParticipantVM): void {
    this.persons.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: false } : item)
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
      title: this.title.trim(),
      category: this.selectedCategory ?? undefined,
      message: this.message.trim() || undefined,
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
