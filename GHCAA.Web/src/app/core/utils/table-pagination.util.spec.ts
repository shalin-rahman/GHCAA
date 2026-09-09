import { describe, expect, it } from 'vitest';
import { createTableState } from './table-pagination.util';

describe('table-pagination.util', () => {
    it('should initialize with default values', () => {
        const state = createTableState();
        expect(state.page()).toBe(1);
        expect(state.pageSize()).toBe(10);
        expect(state.search()).toBe('');
        expect(state.sortField()).toBe('');
        expect(state.sortOrder()).toBe('asc');
        expect(state.totalItems()).toBe(0);
        expect(state.totalPages()).toBe(0);
    });

    it('should compute totalPages correctly based on totalItems and pageSize', () => {
        const state = createTableState({ initialPageSize: 10 });
        state.totalItems.set(25);
        expect(state.totalPages()).toBe(3);
    });

    it('should handle nextPage and prevPage boundary limits', () => {
        const state = createTableState({ initialPage: 1, initialPageSize: 10 });
        state.totalItems.set(20);

        expect(state.totalPages()).toBe(2);

        state.nextPage();
        expect(state.page()).toBe(2);

        // Cannot navigate beyond totalPages
        state.nextPage();
        expect(state.page()).toBe(2);

        state.prevPage();
        expect(state.page()).toBe(1);

        // Cannot navigate below page 1
        state.prevPage();
        expect(state.page()).toBe(1);
    });

    it('should reset state back to initial values', () => {
        const state = createTableState({ initialPage: 1, initialPageSize: 5, initialSearch: 'init' });
        state.page.set(3);
        state.search.set('filtered');
        state.totalItems.set(50);

        state.reset();

        expect(state.page()).toBe(1);
        expect(state.pageSize()).toBe(5);
        expect(state.search()).toBe('init');
        expect(state.totalItems()).toBe(0);
    });
});
