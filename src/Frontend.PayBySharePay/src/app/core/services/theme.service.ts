import { Injectable, signal } from '@angular/core';

export type Theme = 'default' | 'color' | 'minimal' | 'charcoal';

const THEME_KEY = 'sbys_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly current = signal<Theme>('color');

  constructor() {
    const stored = localStorage.getItem(THEME_KEY);
    const valid: Theme[] = ['default', 'color', 'minimal', 'charcoal'];
    this.apply(valid.includes(stored as Theme) ? (stored as Theme) : 'color');
  }

  init(): void {}

  setTheme(theme: Theme): void {
    this.apply(theme);
    localStorage.setItem(THEME_KEY, theme);
  }

  private apply(theme: Theme): void {
    this.current.set(theme);
    document.documentElement.setAttribute('data-theme', theme);
  }
}
