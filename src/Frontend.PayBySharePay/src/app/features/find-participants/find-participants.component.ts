import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Participant, ParticipantType } from '../../core/models/participant.model';

interface ParticipantVM extends Participant {
  initials: string;
  avatarColor: string;
  selected: boolean;
}

const AVATAR_COLORS = [
  '#7c5cbf', '#2e7d32', '#1565c0', '#ad1457',
  '#00838f', '#558b2f', '#4527a0', '#6d4c41',
  '#d84315', '#37474f', '#00695c', '#283593'
];

function toInitials(name: string): string {
  return name.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase();
}

function avatarColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

const MOCK_PARTICIPANTS: Participant[] = [
  { id: 1,  name: 'Lucas Rasmussen',  email: '@lucas',   participantType: ParticipantType.Person },
  { id: 2,  name: 'Malthe Eriksen',   email: '@malthe',  participantType: ParticipantType.Person },
  { id: 3,  name: 'Nikolaj Holm',     email: '@nikolaj', participantType: ParticipantType.Person },
  { id: 4,  name: 'Noah Pedersen',    email: '@noah',    participantType: ParticipantType.Person },
  { id: 5,  name: 'Oliver Dahl',      email: '@oliver',  participantType: ParticipantType.Person },
  { id: 6,  name: 'Oscar Sørensen',   email: '@oscar',   participantType: ParticipantType.Person },
  { id: 7,  name: 'Sara Nørgaard',    email: '@sara',    participantType: ParticipantType.Person },
  { id: 8,  name: 'Sofie Lund',       email: '@sofie',   participantType: ParticipantType.Person },
  { id: 9,  name: 'Steff Warto',      email: '@steff',   participantType: ParticipantType.Person },
  { id: 10, name: 'Victor Møller',    email: '@victor',  participantType: ParticipantType.Person },
  { id: 11, name: 'Coffee Corner',    email: '@coffeecorner', participantType: ParticipantType.Merchant,
    merchantInfo: { companyName: 'Coffee Corner ApS', cvr: '12345678' } },
  { id: 12, name: 'Pizza House',      email: '@pizzahouse', participantType: ParticipantType.Merchant,
    merchantInfo: { companyName: 'Pizza House A/S', cvr: '87654321' } },
];

@Component({
  selector: 'app-find-participants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="find">
      <header class="find__header">
        <h1 class="find__title">Find deltagere</h1>
      </header>

      <div class="find__search">
        <input
          class="find__input"
          type="search"
          placeholder="Søg navn..."
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearch()"
        />
        <button class="find__search-btn" (click)="onSearch()">Find</button>
      </div>

      <div class="find__list">
        @for (p of filtered(); track p.id) {
          <div class="participant" [class.participant--selected]="p.selected" (click)="toggleSelect(p)">
            <div class="participant__avatar" [style.background]="p.avatarColor">
              {{ p.initials }}
            </div>
            <div class="participant__info">
              <span class="participant__name">{{ p.name }}</span>
              <span class="participant__sub">{{ p.email }}</span>
            </div>
            <div class="participant__right">
              @if (p.participantType === ParticipantType.Merchant) {
                <span class="badge badge--merchant">Merchant</span>
              }
              <div class="participant__check" [class.participant__check--active]="p.selected"></div>
            </div>
          </div>
        }

        @if (filtered().length === 0) {
          <p class="find__empty">Ingen deltagere fundet.</p>
        }
      </div>

      <div class="find__footer">
        <button class="find__add-btn" [disabled]="selectedCount() === 0" (click)="addSelected()">
          @if (selectedCount() > 0) {
            Tilføj {{ selectedCount() }} deltager{{ selectedCount() === 1 ? '' : 'e' }}
          } @else {
            Tilføj person til vennelisten
          }
        </button>
      </div>
    </div>
  `,
  styles: [`
    .find {
      display: flex;
      flex-direction: column;
      min-height: 100%;
      background: #0a0e1a;
      color: #fff;
    }

    .find__header {
      padding: 20px 20px 8px;
    }

    .find__title {
      font-size: 24px;
      font-weight: 700;
      color: #fff;
    }

    .find__search {
      display: flex;
      gap: 10px;
      padding: 10px 16px 12px;
    }

    .find__input {
      flex: 1;
      background: #1a2035;
      border: none;
      border-radius: 10px;
      padding: 12px 14px;
      color: #fff;
      font-size: 15px;
      outline: none;
      min-height: 44px;
    }

    .find__input::placeholder {
      color: #6b7280;
    }

    .find__search-btn {
      background: #1a2035;
      color: #fff;
      border: none;
      border-radius: 10px;
      padding: 12px 18px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      min-height: 44px;
      transition: opacity 0.15s;
    }

    .find__search-btn:active {
      opacity: 0.7;
    }

    .find__list {
      flex: 1;
      overflow-y: auto;
      padding-bottom: 80px;
    }

    .participant {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 12px 16px;
      border-bottom: 1px solid rgba(255,255,255,0.06);
      cursor: pointer;
      transition: background 0.15s;
      min-height: 60px;
    }

    .participant:active {
      background: rgba(255,255,255,0.04);
    }

    .participant--selected {
      background: rgba(99, 102, 241, 0.08);
    }

    .participant__avatar {
      width: 44px;
      height: 44px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 15px;
      font-weight: 700;
      color: #fff;
      flex-shrink: 0;
    }

    .participant__info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 2px;
      overflow: hidden;
    }

    .participant__name {
      font-size: 15px;
      font-weight: 600;
      color: #fff;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .participant__sub {
      font-size: 13px;
      color: #6b7280;
    }

    .participant__right {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-shrink: 0;
    }

    .badge--merchant {
      background: rgba(234, 179, 8, 0.2);
      color: #fbbf24;
      border: 1px solid rgba(234,179,8,0.4);
      font-size: 11px;
      font-weight: 600;
      padding: 3px 8px;
      border-radius: 20px;
    }

    .participant__check {
      width: 22px;
      height: 22px;
      border-radius: 50%;
      border: 2px solid #374151;
      transition: all 0.15s;
    }

    .participant__check--active {
      background: #6366f1;
      border-color: #6366f1;
    }

    .participant__check--active::after {
      content: '✓';
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      font-size: 13px;
      height: 100%;
    }

    .find__empty {
      text-align: center;
      color: #6b7280;
      padding: 40px 16px;
      font-size: 15px;
    }

    .find__footer {
      position: fixed;
      bottom: 68px;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 390px;
      padding: 12px 16px;
      background: linear-gradient(to top, #0a0e1a 80%, transparent);
    }

    .find__add-btn {
      width: 100%;
      background: #1a2a5e;
      color: #fff;
      border: none;
      border-radius: 12px;
      padding: 15px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      min-height: 50px;
      transition: opacity 0.2s;
    }

    .find__add-btn:disabled {
      opacity: 0.5;
      cursor: default;
    }

    .find__add-btn:not(:disabled):active {
      opacity: 0.8;
    }
  `]
})
export class FindParticipantsComponent implements OnInit {
  readonly ParticipantType = ParticipantType;

  searchTerm = '';
  participants = signal<ParticipantVM[]>([]);

  filtered = computed(() => {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.participants();
    return this.participants().filter(p =>
      p.name.toLowerCase().includes(term) ||
      p.email.toLowerCase().includes(term) ||
      (p.merchantInfo?.companyName?.toLowerCase().includes(term) ?? false)
    );
  });

  selectedCount = computed(() => this.participants().filter(p => p.selected).length);

  ngOnInit(): void {
    // Brug MOCK_PARTICIPANTS – udskift med: this.participantService.searchParticipants().subscribe(...)
    this.participants.set(
      MOCK_PARTICIPANTS.map(p => ({
        ...p,
        initials: toInitials(p.name),
        avatarColor: avatarColor(p.name),
        selected: false
      }))
    );
  }

  onSearch(): void {
    // Computed signal håndterer filtrering reaktivt – intet ekstra behov her.
    // Ved backend-integration: this.participantService.searchParticipants(this.searchTerm).subscribe(...)
  }

  toggleSelect(p: ParticipantVM): void {
    this.participants.update(list =>
      list.map(item => item.id === p.id ? { ...item, selected: !item.selected } : item)
    );
  }

  addSelected(): void {
    const selected = this.participants().filter(p => p.selected);
    // TODO: kald FriendService.addFriend() eller emit valgte deltagere til ordre
    alert(`Tilføjer: ${selected.map(p => p.name).join(', ')}`);
    this.participants.update(list => list.map(p => ({ ...p, selected: false })));
  }
}

