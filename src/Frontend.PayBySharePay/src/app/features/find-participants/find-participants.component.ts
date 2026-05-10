import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DirectoryEntry } from '../../core/models/directory.model';
import { DirectoryService } from '../../core/services/directory.service';
import { FriendService } from '../../core/services/friend.service';
import { AuthService } from '../../core/services/auth.service';

interface DirectoryEntryVM extends DirectoryEntry {
  initials: string;
  avatarColor: string;
  selected: boolean;
}

const AVATAR_COLORS = [
  '#5c4a8a', '#2d6a4f', '#1a4f7a', '#7a2d5c',
  '#1a6b72', '#4a6741', '#3d2a8a', '#5c3d2e'
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
  selector: 'app-find-participants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './find-participants.component.html',
  styleUrl: './find-participants.component.css'
})
export class FindParticipantsComponent implements OnInit {
  searchTerm = '';
  entries = signal<DirectoryEntryVM[]>([]);
  activeTab = signal<'persons' | 'merchants'>('persons');
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  filtered = computed(() => {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.entries();
    return this.entries().filter(e =>
      e.displayName.toLowerCase().includes(term) ||
      (e.handle?.toLowerCase().includes(term) ?? false) ||
      (e.subtitle?.toLowerCase().includes(term) ?? false)
    );
  });

  filteredByTab = computed(() => {
    const tab = this.activeTab();
    return this.filtered().filter(e =>
      tab === 'merchants' ? e.type === 'Merchant' : e.type === 'Person'
    );
  });

  selectedCount = computed(() =>
    this.entries().filter(e => e.selected).length
  );

  setTab(tab: 'persons' | 'merchants'): void {
    this.activeTab.set(tab);
  }

  constructor(
    private directoryService: DirectoryService,
    private friendService: FriendService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load('');
  }

  private load(query: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const currentUserId = this.auth.currentUserId() ?? undefined;
    this.directoryService.search(query, currentUserId).subscribe({
      next: (list) => {
        this.entries.set(
          list.map(e => ({
            ...e,
            initials: toInitials(e.displayName),
            avatarColor: avatarColor(e.displayName),
            selected: false
          }))
        );
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Kunne ikke hente deltagere. Prøv igen.');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    this.load(this.searchTerm.trim());
  }

  toggleSelect(e: DirectoryEntryVM): void {
    this.entries.update(list =>
      list.map(item => item.id === e.id ? { ...item, selected: !item.selected } : item)
    );
  }

  addSelected(): void {
    const selected = this.entries().filter(e => e.selected);
    if (selected.length === 0) return;

    const currentUserId = this.auth.currentUserId() ?? 0;
    const requests = selected.map(e =>
      this.friendService.addFriend({ initiatorId: currentUserId, receiverId: e.id })
    );

    let completed = 0;
    let hasError = false;

    requests.forEach(req => {
      req.subscribe({
        next: () => {
          completed++;
          if (completed === requests.length && !hasError) {
            this.entries.update(list => list.filter(e => !e.selected));
          }
        },
        error: () => {
          hasError = true;
          this.errorMessage.set('En eller flere venner kunne ikke tilføjes. Prøv igen.');
        }
      });
    });
  }
}
