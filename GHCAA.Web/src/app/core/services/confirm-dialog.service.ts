import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export interface ConfirmDialogOptions {
    title?: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    /** Styles the confirm button as destructive (red) — set for delete/remove actions. */
    danger?: boolean;
}

/**
 * 82.45: replaces the native `window.confirm()` used across 22 admin delete/danger actions
 * with the app's own glass-UI dialog (same host pattern as StepUpService — one dialog mounted
 * once at the app root, opened from anywhere via `confirm()`).
 */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
    readonly isOpen = signal(false);
    readonly title = signal('Confirm');
    readonly message = signal('');
    readonly confirmLabel = signal('Confirm');
    readonly cancelLabel = signal('Cancel');
    readonly danger = signal(false);

    private outcome$: Subject<boolean> | null = null;

    /** Opens the dialog and emits true (confirmed) or false (cancelled/dismissed). */
    confirm(options: ConfirmDialogOptions): Observable<boolean> {
        // A second call while one is open resolves the first as cancelled — only one question
        // is on screen at a time, matching window.confirm()'s own blocking behaviour.
        this.outcome$?.next(false);
        this.outcome$?.complete();

        this.title.set(options.title ?? 'Confirm');
        this.message.set(options.message);
        this.confirmLabel.set(options.confirmLabel ?? 'Confirm');
        this.cancelLabel.set(options.cancelLabel ?? 'Cancel');
        this.danger.set(options.danger ?? false);
        this.isOpen.set(true);

        this.outcome$ = new Subject<boolean>();
        return this.outcome$.asObservable();
    }

    resolve(confirmed: boolean): void {
        this.isOpen.set(false);
        const pending = this.outcome$;
        this.outcome$ = null;
        pending?.next(confirmed);
        pending?.complete();
    }
}
