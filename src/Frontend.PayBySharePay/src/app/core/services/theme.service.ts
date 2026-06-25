import { Injectable, signal } from '@angular/core';

export type Theme = 'default' | 'dark' | 'pink';

const THEME_KEY = 'sbys_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly current = signal<Theme>('default');

  init(): void {
    const stored = localStorage.getItem(THEME_KEY) as Theme | null;
    this.apply(stored ?? 'default');
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
