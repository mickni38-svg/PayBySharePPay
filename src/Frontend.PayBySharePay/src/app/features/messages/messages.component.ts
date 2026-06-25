import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { MessageService } from '../../core/services/message.service';
import { AuthService } from '../../core/services/auth.service';
import { Message } from '../../core/models/message.model';

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

  private routerSub?: Subscription;

  constructor(
    private messageService: MessageService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMessages();

    // GenindlÃÂ¦s og nulstil badge ved hvert besÃÂ¸g pÃÂ¥ /messages
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
    // Nulstil badge synkront sÃÂ¥ det forsvinder med det samme
    this.messageService.resetUnread();

    this.messageService.getByParticipant(userId).subscribe({
      next: (msgs) => {
        this.messages.set(msgs);
        this.isLoading.set(false);
        // MarkÃÂ©r alle som lÃÂ¦st i backend
        this.messageService.markAllRead(userId).subscribe();
      },
      error: () => this.isLoading.set(false)
    });
  }

  /** Returnerer URL'en fra beskeden hvis den indeholder et http-link */
  extractUrl(content: string): string | null {
    const match = content.match(/https?:\/\/\S+/);
    return match ? match[0] : null;
  }

  /** Returnerer den interne sti (/orders) hvis URL'en peger pÃÂ¥ samme origin, ellers null */
  internalPath(url: string): string | null {
    try {
      const parsed = new URL(url);
      if (parsed.origin === window.location.origin) {
        return parsed.pathname + parsed.search + parsed.hash;
      }
    } catch {}
    return null;
  }

  /** Returnerer beskeden uden URL-delen */
  textWithoutUrl(content: string): string {
    return content.replace(/https?:\/\/\S+/, '').trim();
  }
}
