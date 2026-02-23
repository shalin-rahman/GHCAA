import { Injectable, signal, effect } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    theme = signal<Theme>(this.getInitialTheme());

    constructor() {
        // Effect to apply theme to body class automatically
        effect(() => {
            const mode = this.theme();
            document.body.classList.remove('light-theme', 'dark-theme');
            document.body.classList.add(`${mode}-theme`);
            localStorage.setItem('ghcaa_theme', mode);
        });
    }

    toggleTheme() {
        this.theme.update(t => t === 'light' ? 'dark' : 'light');
    }

    private getInitialTheme(): Theme {
        const saved = localStorage.getItem('ghcaa_theme') as Theme;
        if (saved) return saved;

        // Check system preference
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
}
