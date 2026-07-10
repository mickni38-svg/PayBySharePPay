import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { MessageService } from '../../core/services/message.service';
import { AuthService } from '../../core/services/auth.service';
import { Message } from '../../core/models/message.model';

export type MessageFilter = 'alle' | 'bestillinger' | 'beskeder';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, EmptyStateComponent, RouterLink],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  messages = signal<Message[]>([]);
  isLoading = signal(true);
  activeFilter = signal<MessageFilter>('alle');

  filteredMessages = computed(() => {
    const f = this.activeFilter();
    const all = this.messages();
    if (f === 'alle') return all;
    if (f === 'bestillinger') return all.filter(m => this.getCategory(m.content) === 'bestilling');
    if (f === 'beskeder') return all.filter(m => this.getCategory(m.content) === 'besked');
    return all;
  });

  setFilter(f: MessageFilter): void {
    this.activeFilter.set(f);
  }

  /** Bestemmer kategori ud fra beskedindhold */
  getCategory(content: string): 'bestilling' | 'besked' | 'system' {
    const lower = content.toLowerCase();
    if (lower.includes('bestil') || lower.includes('oprettet') || lower.includes('ordre') || lower.includes('reservation') || lower.includes('betaling') || lower.includes('godkend') || lower.includes('captured') || lower.includes('reserveret')) return 'bestilling';
    if (lower.includes('skrev') || lower.includes('skriver') || lower.includes('besked') || lower.includes('tilføjet') || lower.includes('invit')) return 'besked';
    return 'system';
  }

  private routerSub?: Subscription;

  constructor(
    private messageService: MessageService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMessages();

    // Genindlæs og nulstil badge ved hvert besøg på /messages
    this.routerSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      const nav = e as NavigationEnd;
      if (nav.urlAfterRedirects.startsWith('/messages')) {
        this.loadMessages();
      }
    });
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  private loadMessages(): void {
    const userId = this.auth.currentUserId();
    if (userId == null) { this.isLoading.set(false); return; }

    this.isLoading.set(true);

    this.messageService.getByParticipant(userId).subscribe({
      next: (msgs) => {
        this.messages.set(msgs);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  /** Kaldes ved klik på et card — markerer beskeden som læst og opdaterer badge */
  onCardClick(msg: Message): void {
    if (msg.isRead) return;

    this.messageService.markRead(msg.id).subscribe({
      next: () => {
        // Opdatér lokalt så UI skifter med det samme
        this.messages.update(list =>
          list.map(m => m.id === msg.id ? { ...m, isRead: true } : m)
        );
        // Sænk badge-tæller
        const current = this.messageService.unreadCount();
        if (current > 0) this.messageService.unreadCount.set(current - 1);
      }
    });
  }

  /** Returnerer URL'en fra beskeden hvis den indeholder et http-link */
  extractUrl(content: string): string | null {
    const match = content.match(/https?:\/\/\S+/);
    return match ? match[0] : null;
  }

  /** Returnerer den interne sti (/orders) hvis URL'en peger på samme origin, ellers null */
  internalPath(url: string): string | null {
    try {
      const parsed = new URL(url);
      if (parsed.origin === window.location.origin) {
        return parsed.pathname + parsed.search + parsed.hash;
      }
    } catch {}
    return null;
  }

  /** Returnerer beskeden uden URL-delen og uden "Bestil din mad her:"-label */
  textWithoutUrl(content: string): string {
    return content
      .replace(/https?:\/\/\S+/, '')
      .replace(/bestil din mad her\s*:/i, '')
      .trim();
  }
}
