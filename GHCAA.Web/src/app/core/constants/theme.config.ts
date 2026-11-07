export const THEME_CONFIG = {
    light: {
        bodyClass: 'light-theme',
        toggleLabel: 'Switch to dark mode',
        toggleIcon: 'theme-dark'
    },
    dark: {
        bodyClass: 'dark-theme',
        toggleLabel: 'Switch to light mode',
        toggleIcon: 'theme-light'
    }
} as const;

export type Theme = keyof typeof THEME_CONFIG;

// Dark is the brand's native Obsidian and Gold first impression.
export const DEFAULT_THEME: Theme = 'dark';
export const THEME_STORAGE_KEY = 'ghcaa_theme';

const themeOrder = Object.keys(THEME_CONFIG) as Theme[];

export function isTheme(value: string | null): value is Theme {
    return value !== null && Object.prototype.hasOwnProperty.call(THEME_CONFIG, value);
}

export function getNextTheme(current: Theme): Theme {
    const currentIndex = themeOrder.indexOf(current);
    const next = themeOrder[(currentIndex + 1) % themeOrder.length];
    if (!next) throw new Error(`Theme "${current}" is missing from themeOrder.`);
    return next;
}
