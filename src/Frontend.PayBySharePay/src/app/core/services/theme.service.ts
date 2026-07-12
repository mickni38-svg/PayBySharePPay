import { Injectable, signal } from '@angular/core';

export type Theme = 'default' | 'dark';

const THEME_KEY = 'sbys_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly current = signal<Theme>('default');

  init(): void {
    const stored = localStorage.getItem(THEME_KEY);
    // Guard: hvis et gammelt 'pink'-tema er gemt, nulstil til default
    const valid: Theme[] = ['default', 'dark'];
    this.apply(valid.includes(stored as Theme) ? (stored as Theme) : 'default');
  }

  setTheme(theme: Theme): void {
    this.apply(theme);
    localStorage.setItem(THEME_KEY, theme);
  }

  private apply(theme: Theme): void {
    this.current.set(theme);
    document.documentElement.setAttribute('data-theme', theme);
  }
}
