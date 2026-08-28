import { Injectable, signal, effect, inject, afterNextRender } from '@angular/core';
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

        // Deferred for consistency with AuthService's same-shape NG0200 guard: this is a root
        // service whose constructor mutates a signal (`activeSpecialTheme`) that templates read
        // directly (e.g. public-layout.html), and it can be constructed mid-render.
        afterNextRender(() => this.loadActiveSpecialTheme());
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

        // Default to dark for a fresh visitor. Both themes are fully styled — the landing's
        // section backgrounds and text now use the semantic --section-bg / --text-* tokens in
        // styles.scss, which flip between :root (light) and body.dark-theme (dark). Dark is the
        // brand's native "Obsidian & Gold" look, so it's the first impression; the toggle then
        // switches to a properly-contrasted light palette and persists the choice.
        return 'dark';
    }
}
