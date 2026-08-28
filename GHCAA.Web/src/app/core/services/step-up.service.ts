import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { API_ENDPOINTS } from '../constants/app.constants';

/**
 * 7.13: Drives the admin step-up (2FA) challenge.
 *
 * The API answers destructive/financial admin actions with 403 + Code STEP_UP_REQUIRED when the
 * caller's token carries no recent OTP verification. The interceptor calls `challenge()`, which
 * opens the dialog and emits once the admin has verified (or cancelled), so the original request
 * can be retried on the freshly re-issued token.
 *
 * One challenge at a time: concurrent gated requests all wait on the same dialog rather than
 * stacking several on top of each other.
 */
@Injectable({ providedIn: 'root' })
export class StepUpService {
    private http = inject(HttpClient);

    readonly isOpen = signal(false);
    readonly sending = signal(false);
    readonly verifying = signal(false);
    readonly errorMessage = signal<string | null>(null);
    readonly infoMessage = signal<string | null>(null);

    private outcome$: Subject<boolean> | null = null;

    /** Opens the challenge (or joins the one already open). Emits true once verified. */
    challenge(): Observable<boolean> {
        if (this.outcome$) return this.outcome$.asObservable();

        this.outcome$ = new Subject<boolean>();
        this.errorMessage.set(null);
        this.infoMessage.set(null);
        this.isOpen.set(true);
        this.requestCode();

        return this.outcome$.asObservable();
    }

    /** Sends (or re-sends) the verification code to the admin's registered email. */
    requestCode(): void {
        this.sending.set(true);
        this.errorMessage.set(null);

        this.http.post(API_ENDPOINTS.AUTH.STEP_UP_REQUEST, {}, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        }).pipe(
            catchError(err => {
                this.errorMessage.set(err?.error?.message || 'Could not send a verification code.');
                return of(null);
            })
        ).subscribe(res => {
            this.sending.set(false);
            if (res !== null) this.infoMessage.set('A verification code has been sent to your registered email.');
        });
    }

    /** Verifies the submitted code; on success the API re-issues the token with the step-up claim. */
    submitCode(code: string): void {
        this.verifying.set(true);
        this.errorMessage.set(null);

        this.http.post(API_ENDPOINTS.AUTH.STEP_UP_VERIFY, { code }, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        }).pipe(
            map(() => true),
            catchError(err => {
                this.errorMessage.set(err?.error?.message || 'That code is invalid or has expired.');
                return of(false);
            }),
            tap(() => this.verifying.set(false))
        ).subscribe(ok => {
            if (ok) this.close(true);
        });
    }

    /** Admin dismissed the dialog — the gated request fails with its original error. */
    cancel(): void {
        this.close(false);
    }

    private close(verified: boolean): void {
        this.isOpen.set(false);
        this.sending.set(false);
        this.verifying.set(false);
        this.infoMessage.set(null);
        this.errorMessage.set(null);

        const pending = this.outcome$;
        this.outcome$ = null;
        pending?.next(verified);
        pending?.complete();
    }
}
