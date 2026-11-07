import { Injectable, signal, effect, inject, afterNextRender } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../constants/app.constants';
import {
    DEFAULT_THEME,
    getNextTheme,
    isTheme,
    THEME_CONFIG,
    THEME_STORAGE_KEY,
    type Theme
} from '../constants/theme.config';
import { tap } from 'rxjs';

export type { Theme } from '../constants/theme.config';

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
            document.body.classList.remove(...Object.values(THEME_CONFIG).map(config => config.bodyClass));
            document.body.classList.add(THEME_CONFIG[mode].bodyClass);
            localStorage.setItem(THEME_STORAGE_KEY, mode);
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
        this.theme.update(getNextTheme);
    }

    private getInitialTheme(): Theme {
        const saved = localStorage.getItem(THEME_STORAGE_KEY);
        if (isTheme(saved)) return saved;

        return DEFAULT_THEME;
    }
}
