import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('uses minimal as the default theme when no preference is stored', () => {
    const service = new ThemeService();

    expect(service.current()).toBe('minimal');
    expect(document.documentElement.getAttribute('data-theme')).toBe('minimal');
  });

  it('preserves a valid stored theme preference', () => {
    localStorage.setItem('sbys_theme', 'charcoal');

    const service = new ThemeService();

    expect(service.current()).toBe('charcoal');
    expect(document.documentElement.getAttribute('data-theme')).toBe('charcoal');
  });
});
