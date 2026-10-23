import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { API_ENDPOINTS } from '../constants/app.constants';

export type StepUpMethod = 'email' | 'app';

export interface AuthenticatorStatus {
    enrolled: boolean;
    pending: boolean;
    recoveryCodesLeft: number;
    // True when the user has no email on file, so setup asks for the password instead of a step-up.
    setupNeedsPassword?: boolean;
}

export interface AuthenticatorSetup {
    secret: string;
    otpAuthUri: string;
    qrCodeDataUri: string;
}

export interface RecoveryCodes {
    codes: string[];
}

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
    // 95.6: an enrolled user starts on the app code and sends no email, but can switch to an
    // emailed code if the phone is not at hand. canSwitch is false when there is no email to send to.
    readonly method = signal<StepUpMethod>('email');
    readonly canSwitch = signal(false);

    private outcome$: Subject<boolean> | null = null;

    /** Opens the challenge (or joins the one already open). Emits true once verified. */
    challenge(): Observable<boolean> {
        if (this.outcome$) return this.outcome$.asObservable();

        const outcome$ = new Subject<boolean>();
        this.outcome$ = outcome$;
        this.errorMessage.set(null);
        this.infoMessage.set(null);
        this.method.set('email');
        this.canSwitch.set(false);
        this.isOpen.set(true);
        this.sending.set(true);

        // If the status call fails, fall back to email, which the API accepts for every user.
        this.authenticatorStatus().pipe(
            catchError(() => of(null))
        ).subscribe(status => {
            if (this.outcome$ !== outcome$) return;
            if (status?.enrolled) {
                this.sending.set(false);
                this.method.set('app');
                this.canSwitch.set(!status.setupNeedsPassword);
            } else {
                this.requestCode();
            }
        });

        return outcome$.asObservable();
    }

    /** Switches between an app code and an emailed code. Moving to email sends a code. */
    useMethod(method: StepUpMethod): void {
        if (!this.canSwitch() || method === this.method()) return;
        this.method.set(method);
        this.errorMessage.set(null);
        this.infoMessage.set(null);
        if (method === 'email') this.requestCode();
    }

    /** Sends (or re-sends) the verification code to the admin's registered email. */
    requestCode(): void {
        this.sending.set(true);
        this.errorMessage.set(null);

        this.http.post<{ sentTo?: string }>(API_ENDPOINTS.AUTH.STEP_UP_REQUEST, {}, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        }).pipe(
            catchError(err => {
                this.errorMessage.set(err?.error?.detail || err?.error?.message || 'Could not send a verification code.');
                return of(null);
            })
        ).subscribe(res => {
            this.sending.set(false);
            if (res === null) return;
            this.infoMessage.set(res?.sentTo
                ? `A verification code has been sent to ${res.sentTo}.`
                : 'A verification code has been sent to your registered email.');
        });
    }

    /** Verifies the submitted code; on success the API re-issues the token with the step-up claim. */
    submitCode(code: string): void {
        this.verifying.set(true);
        this.errorMessage.set(null);

        this.http.post(API_ENDPOINTS.AUTH.STEP_UP_VERIFY, { code, method: this.method() }, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        }).pipe(
            map(() => true),
            catchError(err => {
                this.errorMessage.set(err?.error?.detail || err?.error?.message || 'That code is invalid or has expired.');
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

    authenticatorStatus(): Observable<AuthenticatorStatus> {
        return this.http.get<AuthenticatorStatus>(API_ENDPOINTS.AUTH.AUTHENTICATOR, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        });
    }

    // Setup, new recovery codes and removal are step-up gated on the API, so the interceptor
    // raises the challenge first when the token has no recent verification.
    setupAuthenticator(): Observable<AuthenticatorSetup> {
        return this.http.post<AuthenticatorSetup>(API_ENDPOINTS.AUTH.AUTHENTICATOR_SETUP, {});
    }

    // For a user with no email, who cannot pass an email step-up. The API refuses it for anyone else.
    setupAuthenticatorWithPassword(password: string): Observable<AuthenticatorSetup> {
        return this.http.post<AuthenticatorSetup>(API_ENDPOINTS.AUTH.AUTHENTICATOR_SETUP_WITH_PASSWORD, { password }, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        });
    }

    confirmAuthenticator(code: string): Observable<RecoveryCodes> {
        return this.http.post<RecoveryCodes>(API_ENDPOINTS.AUTH.AUTHENTICATOR_CONFIRM, { code }, {
            headers: { 'X-Skip-Error-Notify': 'true' }
        });
    }

    regenerateRecoveryCodes(): Observable<RecoveryCodes> {
        return this.http.post<RecoveryCodes>(API_ENDPOINTS.AUTH.AUTHENTICATOR_RECOVERY_CODES, {});
    }

    removeAuthenticator(): Observable<void> {
        return this.http.delete<void>(API_ENDPOINTS.AUTH.AUTHENTICATOR);
    }

    private close(verified: boolean): void {
        this.isOpen.set(false);
        this.method.set('email');
        this.canSwitch.set(false);
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
