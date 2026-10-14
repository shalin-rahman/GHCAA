import { describe, it, expect } from 'vitest';
import { appendById } from './paging.util';

describe('NFR-P1: appendById', () => {
    it('keeps the order and adds the new rows', () => {
        expect(appendById([{ id: 1 }], [{ id: 2 }, { id: 3 }]).map(i => i.id)).toEqual([1, 2, 3]);
    });

    it('drops a row the list already holds', () => {
        expect(appendById([{ id: 1 }, { id: 2 }], [{ id: 2 }, { id: 3 }]).map(i => i.id)).toEqual([1, 2, 3]);
    });

    it('handles an empty list or an empty page', () => {
        expect(appendById([], [{ id: 1 }])).toEqual([{ id: 1 }]);
        expect(appendById([{ id: 1 }], [])).toEqual([{ id: 1 }]);
    });
});
