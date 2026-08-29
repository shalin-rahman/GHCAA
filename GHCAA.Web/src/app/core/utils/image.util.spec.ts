import { describe, it, expect } from 'vitest';
import { safeImageUrl } from './image.util';

describe('safeImageUrl', () => {
    it('passes through an absolute upload path', () => {
        expect(safeImageUrl('/uploads/members/1/photo.jpg')).toBe('/uploads/members/1/photo.jpg');
    });

    it('passes through a full http(s) URL', () => {
        expect(safeImageUrl('https://cdn.example.com/a.jpg')).toBe('https://cdn.example.com/a.jpg');
    });

    it('falls back for bad seed/import data like "..."', () => {
        expect(safeImageUrl('...')).toBe('/assets/logo.png');
    });

    it('falls back for null/undefined/empty', () => {
        expect(safeImageUrl(null)).toBe('/assets/logo.png');
        expect(safeImageUrl(undefined)).toBe('/assets/logo.png');
        expect(safeImageUrl('')).toBe('/assets/logo.png');
    });

    it('honors a custom fallback', () => {
        expect(safeImageUrl('bad-value', '/assets/placeholder.jpg')).toBe('/assets/placeholder.jpg');
    });
});
