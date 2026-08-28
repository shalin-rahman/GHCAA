import { Injectable, signal, computed, inject, afterNextRender, effect } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, catchError, of, switchMap } from 'rxjs';
import { LoginDto, TokenResponseDto, User } from '../models/auth.models';
import { API_ENDPOINTS, ROUTES } from '../constants/app.constants';

// Non-sensitive display fields stored in sessionStorage (no token).
type SessionUser = Omit<User, 'token'> & { token?: never };

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);

    private _currentUser = signal<User | null>(this.getUserFromStorage());
    public currentUser = computed(() => this._currentUser());
    public isAuthenticated = computed(() => !!this._currentUser());

    // 30.26: true once the async session-restore below has settled (immediately true if a
    // cached user was already found in storage, so no consumer waits unnecessarily). Lets
    // callers that need to distinguish "confirmed guest" from "auth still resolving" (e.g. a
    // deep-linked page that redirects guests away) avoid mis-classifying a real member as a
    // guest during the brief window before /auth/me responds on a fresh page load.
    private _authChecked = signal<boolean>(!!this._currentUser());
    public authChecked = computed(() => this._authChecked());

    private inactivityTimer: any;
    private readonly TIMEOUT_MS = 10 * 60 * 1000;

    // Centralizes the "run this once the deferred /auth/me restore has settled AND the user turns
    // out to be logged in" pattern — every consumer of member-only data on a page that doesn't
    // itself go through authGuard (gallery "My Albums", saved payment methods, "my event
    // registrations") needs this same gate, or a fresh page load races the restore and the data
    // silently never loads. Call from a component's constructor/field initializer (a live
    // injection context) exactly like the callers below do with a plain effect() today —
    // this just avoids re-deriving the same 3-line effect at every call site.
    whenAuthenticated(callback: () => void): void {
        effect(() => {
            if (this._authChecked() && this._currentUser()) {
                callback();
            }
        });
    }

    constructor() {
        this.initActivityTracking();
        // 24.39: Restore auth state from httpOnly cookie via /auth/me on page load.
        // NG0200 guard: AuthService is providedIn:'root', so it can be constructed mid-render
        // (e.g. a header component injecting it during its own construction). If the /auth/me
        // response happened to resolve inside that same change-detection flush, the resulting
        // signal writes below would land mid-check and throw ExpressionChangedAfterItHasBeenCheckedError.
        // afterNextRender defers the call (and its signal mutations) until after the first render
        // pass has fully settled, so it can never race the initial CD cycle.
        if (typeof window !== 'undefined' && !this._currentUser()) {
            afterNextRender(() => {
                // A guest visitor gets a routine 401 here — it must never surface as an error
                // toast (X-Skip-Error-Notify) or feed the interceptor's refresh/logout cascade
                // (the interceptor separately excludes /api/auth/me from that by URL).
                this.http.get<any>(API_ENDPOINTS.AUTH.ME, {
                    withCredentials: true,
                    headers: new HttpHeaders().set('X-Skip-Error-Notify', 'true')
                })
                    .pipe(catchError(err => {
                        // A 401 here just means "not logged in" — the expected answer for every
                        // guest on every page load, not a failure worth logging. Anything else
                        // (network error, 500, timeout) is a genuine problem worth keeping.
                        if (err?.status !== 401) {
                            console.error('Failed to restore session from /auth/me', err);
                        }
                        return of(null);
                    }))
                    .subscribe(me => {
                        if (me) {
                            const user: User = {
                                username: me.username,
                                memberId: me.memberId,
                                role: me.role ?? 'Member',
                                token: '',      // token is httpOnly — not accessible to JS
                                fullName: me.fullName ?? me.username,
                                email: me.email,
                                mobileNo: me.mobileNo,
                                mustChangePassword: me.mustChangePassword ?? false
                            };
                            this._currentUser.set(user);
                            sessionStorage.setItem('user_session', JSON.stringify(this.toSessionUser(user)));
                            this.resetTimer();
                        }
                        this._authChecked.set(true);
                    });
            });
        }
    }

    private initActivityTracking() {
        if (typeof window !== 'undefined') {
            ['mousemove', 'keydown', 'click', 'scroll'].forEach(e =>
                window.addEventListener(e, () => this.resetTimer(), { passive: true })
            );
            this.resetTimer();
        }
    }

    private resetTimer() {
        if (this.inactivityTimer) clearTimeout(this.inactivityTimer);
        if (!this.isAuthenticated()) return;
        this.inactivityTimer = setTimeout(() => {
            if (this.isAuthenticated()) {
                this._currentUser.set(null);
                sessionStorage.removeItem('user_session');
                this.router.navigate([ROUTES.LOGIN], { queryParams: { expired: true } });
            }
        }, this.TIMEOUT_MS);
    }

    login(credentials: LoginDto): Observable<User> {
        return this.http.post<TokenResponseDto>(API_ENDPOINTS.AUTH.LOGIN, credentials, { withCredentials: true }).pipe(
            map(response => this.mapAndSetUser(response))
        );
    }

    getSocialProviders(): Observable<any[]> {
        return this.http.get<any[]>(API_ENDPOINTS.AUTH.PROVIDERS);
    }

    googleLogin(idToken: string): Observable<User> {
        return this.http.post<TokenResponseDto>(API_ENDPOINTS.AUTH.GOOGLE, { idToken }, { withCredentials: true }).pipe(
            map(response => this.mapAndSetUser(response))
        );
    }

    facebookLogin(accessToken: string): Observable<User> {
        return this.http.post<TokenResponseDto>(API_ENDPOINTS.AUTH.FACEBOOK, { accessToken }, { withCredentials: true }).pipe(
            map(response => this.mapAndSetUser(response))
        );
    }

    // 24.44: Called by the interceptor on 401 to issue a new access token cookie.
    refresh(): Observable<boolean> {
        return this.http.post<any>(API_ENDPOINTS.AUTH.REFRESH, {}, { withCredentials: true }).pipe(
            map(() => true),
            catchError(() => of(false))
        );
    }

    logout() {
        this.http.post(API_ENDPOINTS.AUTH.LOGOUT, {}, { withCredentials: true })
            .pipe(catchError(err => {
                console.error('Logout request to server failed; clearing local session anyway', err);
                return of(null);
            }))
            .subscribe(() => {
                this._currentUser.set(null);
                sessionStorage.removeItem('user_session');
                localStorage.removeItem('user_session');
                this.router.navigate([ROUTES.LOGIN]);
            });
    }

    // token is in the httpOnly cookie — Angular doesn't read it directly.
    // Return empty string so Bearer header injection is harmless when token missing.
    getToken(): string | null {
        return this._currentUser()?.token || null;
    }

    // 29A.1: Clear the forced-password-change flag after a successful change so authGuard
    // stops redirecting to /portal/change-password and the user can reach the portal.
    clearMustChangePassword(): void {
        const current = this._currentUser();
        if (!current || !current.mustChangePassword) return;
        const updated: User = { ...current, mustChangePassword: false };
        this._currentUser.set(updated);
        sessionStorage.setItem('user_session', JSON.stringify(this.toSessionUser(updated)));
    }

    private mapAndSetUser(response: TokenResponseDto): User {
        const user: User = {
            username: response.username,
            memberId: response.memberId,
            // 24.39: Keep the token in-memory for the session only (NOT stored in localStorage).
            token: response.token,
            role: response.role ?? 'Member',
            fullName: response.fullName,
            email: response.email,
            mobileNo: response.mobileNo,
            mustChangePassword: response.mustChangePassword
        };
        this._currentUser.set(user);
        // Only persist non-sensitive display fields.
        sessionStorage.setItem('user_session', JSON.stringify(this.toSessionUser(user)));
        this.resetTimer();
        return user;
    }

    private toSessionUser(u: User): SessionUser {
        const { token: _t, ...rest } = u as any;
        return rest;
    }

    private getUserFromStorage(): User | null {
        if (typeof window === 'undefined') return null;
        // 24.39: Prefer sessionStorage (no token); fall back to legacy localStorage and migrate.
        const raw = sessionStorage.getItem('user_session') || localStorage.getItem('user_session');
        if (!raw) return null;

        try {
            const parsed: User = JSON.parse(raw);
            // If legacy localStorage had a token, clear it.
            if (localStorage.getItem('user_session')) {
                sessionStorage.setItem('user_session', JSON.stringify(this.toSessionUser(parsed)));
                localStorage.removeItem('user_session');
            }
            // Session user has no token — auth state is restored via /auth/me constructor call.
            return parsed;
        } catch {
            sessionStorage.removeItem('user_session');
            localStorage.removeItem('user_session');
            return null;
        }
    }
}
