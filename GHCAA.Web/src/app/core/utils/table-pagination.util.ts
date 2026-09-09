import { signal, computed, WritableSignal, Signal } from '@angular/core';

export interface TableStateOptions {
    initialPage?: number;
    initialPageSize?: number;
    initialSearch?: string;
    initialSortField?: string;
    initialSortOrder?: 'asc' | 'desc';
}

export interface TableState {
    page: WritableSignal<number>;
    pageSize: WritableSignal<number>;
    search: WritableSignal<string>;
    sortField: WritableSignal<string>;
    sortOrder: WritableSignal<'asc' | 'desc'>;
    totalItems: WritableSignal<number>;
    totalPages: Signal<number>;
    nextPage: () => void;
    prevPage: () => void;
    reset: () => void;
}

export function createTableState(options: TableStateOptions = {}): TableState {
    const defaultPage = options.initialPage ?? 1;
    const defaultPageSize = options.initialPageSize ?? 10;
    const defaultSearch = options.initialSearch ?? '';
    const defaultSortField = options.initialSortField ?? '';
    const defaultSortOrder = options.initialSortOrder ?? 'asc';

    const page = signal<number>(defaultPage);
    const pageSize = signal<number>(defaultPageSize);
    const search = signal<string>(defaultSearch);
    const sortField = signal<string>(defaultSortField);
    const sortOrder = signal<'asc' | 'desc'>(defaultSortOrder);
    const totalItems = signal<number>(0);

    const totalPages = computed(() => {
        const size = pageSize();
        return size > 0 ? Math.ceil(totalItems() / size) : 0;
    });

    const nextPage = () => {
        if (page() < totalPages()) {
            page.update(p => p + 1);
        }
    };

    const prevPage = () => {
        if (page() > 1) {
            page.update(p => p - 1);
        }
    };

    const reset = () => {
        page.set(defaultPage);
        pageSize.set(defaultPageSize);
        search.set(defaultSearch);
        sortField.set(defaultSortField);
        sortOrder.set(defaultSortOrder);
        totalItems.set(0);
    };

    return {
        page,
        pageSize,
        search,
        sortField,
        sortOrder,
        totalItems,
        totalPages,
        nextPage,
        prevPage,
        reset
    };
}
