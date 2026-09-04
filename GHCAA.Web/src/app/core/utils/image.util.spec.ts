import { describe, it, expect } from 'vitest';
import { safeImageUrl } from './image.util';

describe('safeImageUrl', () => {
    it.each([
        ['passes through an absolute upload path', '/uploads/members/1/photo.jpg', undefined, '/uploads/members/1/photo.jpg'],
        ['passes through a full http(s) URL', 'https://cdn.example.com/a.jpg', undefined, 'https://cdn.example.com/a.jpg'],
        ['falls back for bad seed/import data like "..."', '...', undefined, '/assets/logo.png'],
        ['falls back for null', null, undefined, '/assets/logo.png'],
        ['falls back for undefined', undefined, undefined, '/assets/logo.png'],
        ['falls back for empty string', '', undefined, '/assets/logo.png'],
        ['honors a custom fallback', 'bad-value', '/assets/placeholder.jpg', '/assets/placeholder.jpg']
    ] as const)('%s', (_label, input, fallback, expected) => {
        expect(safeImageUrl(input as any, fallback)).toBe(expected);
    });
});
