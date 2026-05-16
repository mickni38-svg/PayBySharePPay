import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { MessageService } from '../../core/services/message.service';
import { AuthService } from '../../core/services/auth.service';
import { Message } from '../../core/models/message.model';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, EmptyStateComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit {
  messages = signal<Message[]>([]);
  isLoading = signal(true);

  constructor(
    private messageService: MessageService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const userId = this.auth.currentUserId();
    if (userId == null) { this.isLoading.set(false); return; }

    this.messageService.getByParticipant(userId).subscribe({
      next: (msgs) => {
        this.messages.set(msgs);
        this.isLoading.set(false);
        // Markér alle som læst
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

  /** Returnerer beskeden uden URL-delen */
  textWithoutUrl(content: string): string {
    return content.replace(/https?:\/\/\S+/, '').trim();
  }
}
