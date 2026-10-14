import { describe, it, expect } from 'vitest';
import { appendById, fetchAllPages } from './paging.util';

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

describe('NFR-P1: fetchAllPages', () => {
    const pages = [[{ id: 1 }, { id: 2 }], [{ id: 2 }, { id: 3 }], [{ id: 4 }]];
    const fetchFrom = (source: { id: number }[][], totalPages: number) => {
        const asked: number[] = [];
        const fetchPage = async (page: number) => {
            asked.push(page);
            return { items: source[page - 1] ?? [], totalPages };
        };
        return { asked, fetchPage };
    };

    it('reads every page and drops a row repeated across pages', async () => {
        const { asked, fetchPage } = fetchFrom(pages, 3);
        expect((await fetchAllPages(fetchPage)).map(i => i.id)).toEqual([1, 2, 3, 4]);
        expect(asked).toEqual([1, 2, 3]);
    });

    it('stops at an empty page even if totalPages says more', async () => {
        const { asked, fetchPage } = fetchFrom([[{ id: 1 }]], 5);
        expect((await fetchAllPages(fetchPage)).map(i => i.id)).toEqual([1]);
        expect(asked).toEqual([1, 2]);
    });

    it('reads one page when the response has no totalPages or items', async () => {
        expect(await fetchAllPages(async () => ({}))).toEqual([]);
    });

    it('passes a failed page on to the caller', async () => {
        await expect(fetchAllPages(async () => { throw new Error('down'); })).rejects.toThrow('down');
    });
});
