import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ActivityService } from '../../core/services/activity.service';
import { ActivityItem } from '../../core/models/activity.model';

@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activity.component.html',
  styleUrl: './activity.component.scss'
})
export class ActivityComponent implements OnInit {
  loading = signal(true);
  items = signal<ActivityItem[]>([]);
  error = signal(false);

  constructor(
    private auth: AuthService,
    private activityService: ActivityService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const userId = this.auth.currentUserId();
    if (!userId) { this.loading.set(false); return; }

    this.activityService.getActivityFeed(userId).subscribe({
      next: (feed) => {
        this.items.set(feed.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/home']);
  }

  /** Gruppér aktiviteter i 'I dag', 'I går', 'Tidligere' */
  get grouped(): { label: string; items: ActivityItem[] }[] {
    const now = new Date();
    const todayStr = now.toDateString();
    const yesterday = new Date(now);
    yesterday.setDate(now.getDate() - 1);
    const yesterdayStr = yesterday.toDateString();

    const today: ActivityItem[] = [];
    const igar: ActivityItem[] = [];
    const earlier: ActivityItem[] = [];

    for (const item of this.items()) {
      const d = new Date(item.createdAt).toDateString();
      if (d === todayStr) today.push(item);
      else if (d === yesterdayStr) igar.push(item);
      else earlier.push(item);
    }

    const result: { label: string; items: ActivityItem[] }[] = [];
    if (today.length)   result.push({ label: 'I dag',    items: today });
    if (igar.length)    result.push({ label: 'I går',    items: igar });
    if (earlier.length) result.push({ label: 'Tidligere', items: earlier });
    return result;
  }
}
