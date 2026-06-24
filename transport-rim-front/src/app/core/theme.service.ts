import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'transport-rim-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly isDark = signal(this.getInitialTheme());

  constructor() {
    this.applyTheme(this.isDark());
  }

  toggle(): void {
    this.setDark(!this.isDark());
  }

  private setDark(value: boolean): void {
    this.isDark.set(value);
    this.applyTheme(value);
    localStorage.setItem(STORAGE_KEY, value ? 'dark' : 'light');
  }

  private applyTheme(dark: boolean): void {
    document.documentElement.classList.toggle('dark', dark);
  }

  private getInitialTheme(): boolean {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      return stored === 'dark';
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}
