export interface Debounced<Args extends unknown[]> {
    (...args: Args): void;
    /** Clears a pending call without running it — call from ngOnDestroy so a component teardown doesn't fire late. */
    cancel(): void;
}

/**
 * Returns a debounced wrapper around `fn`: a call resets the pending timer instead of
 * queuing another invocation, so only the last call within `ms` actually runs.
 *
 * Each call site owns its own debounced instance (build one per component field), since the
 * timer handle lives in the closure.
 */
export function debounce<Args extends unknown[]>(fn: (...args: Args) => void, ms: number): Debounced<Args> {
    let timer: ReturnType<typeof setTimeout> | null = null;
    const debounced = ((...args: Args) => {
        if (timer) clearTimeout(timer);
        timer = setTimeout(() => fn(...args), ms);
    }) as Debounced<Args>;
    debounced.cancel = () => {
        if (timer) clearTimeout(timer);
        timer = null;
    };
    return debounced;
}
