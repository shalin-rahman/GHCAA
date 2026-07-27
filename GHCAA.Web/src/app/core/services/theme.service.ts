import { Injectable, signal, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../constants/app.constants';
import { tap } from 'rxjs';

export type Theme = 'light' | 'dark';

export interface SpecialDayTheme {
    id: number;
    title: string;
    startDate: string;
    endDate: string;
    backgroundColor: string;
    textColor: string;
    announcementText: string;
    animationStyle: string; // 'Fade' | '3D' | 'Typewriter' | 'Scroll' | 'None'
    imageUrl: string;
    sidebarColor: string;
    enableGradientFading: boolean;
    isActive?: boolean;
    isEnabled: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    private http = inject(HttpClient);
    theme = signal<Theme>(this.getInitialTheme());
    activeSpecialTheme = signal<SpecialDayTheme | null>(null);

    constructor() {
        // Effect to apply theme to body class automatically
        effect(() => {
            const mode = this.theme();
            document.body.classList.remove('light-theme', 'dark-theme');
            document.body.classList.add(`${mode}-theme`);
            localStorage.setItem('ghcaa_theme', mode);
        });

        // Initial load
        this.loadActiveSpecialTheme();
    }

    loadActiveSpecialTheme() {
        this.http.get<SpecialDayTheme>(`${API_ENDPOINTS.THEMES}/active`).subscribe({
            next: (theme) => this.activeSpecialTheme.set(theme),
            error: () => this.activeSpecialTheme.set(null)
        });
    }

    toggleTheme() {
        this.theme.update(t => t === 'light' ? 'dark' : 'light');
    }

    private getInitialTheme(): Theme {
        const saved = localStorage.getItem('ghcaa_theme') as Theme;
        if (saved) return saved;

        // Default to dark. The UI is authored dark-first: the landing sections hardcode
        // #000/#050505 backgrounds and there is no `body.light-theme` palette, so `--bg-color`
        // falls back to :root white in light mode and produces mismatched black bands. Until a
        // real light palette exists, dark is the only fully-styled theme, so a fresh visitor
        // (no saved preference, non-dark OS) must land on dark rather than the broken light mode.
        return 'dark';
    }
}
