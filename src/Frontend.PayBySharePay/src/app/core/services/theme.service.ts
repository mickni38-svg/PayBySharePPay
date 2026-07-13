import { Injectable, signal } from '@angular/core';

export type Theme = 'default' | 'dark';

const THEME_KEY = 'sbys_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly current = signal<Theme>('dark');

  constructor() {
    // Pas temaet til ved opstart – læs localStorage, ellers brug 'dark'
    const stored = localStorage.getItem(THEME_KEY);
    const valid: Theme[] = ['default', 'dark'];
    this.apply(valid.includes(stored as Theme) ? (stored as Theme) : 'dark');
  }

  init(): void {
    // Beholdes for bagud-kompatibilitet – constructor håndterer nu initialiseringen
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
