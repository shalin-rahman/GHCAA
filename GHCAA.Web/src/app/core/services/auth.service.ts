import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { LoginDto, TokenResponseDto, User } from '../models/auth.models';

import { API_ENDPOINTS } from '../constants/app.constants';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);

    // User State Signal
    private _currentUser = signal<User | null>(this.getUserFromStorage());
    public currentUser = computed(() => this._currentUser());
    public isAuthenticated = computed(() => !!this._currentUser());

    constructor() { }

    login(credentials: LoginDto): Observable<User> {
        return this.http.post<TokenResponseDto>(API_ENDPOINTS.AUTH.LOGIN, credentials).pipe(
            map(response => {
                const user: User = {
                    username: response.username,
                    memberId: response.memberId,
                    token: response.token,
                    role: response.role ?? 'Member'
                };
                this.setSession(user);
                return user;
            })
        );
    }

    logout() {
        this._currentUser.set(null);
        localStorage.removeItem('user_session');
        this.router.navigate(['/login']);
    }

    private setSession(user: User) {
        this._currentUser.set(user);
        localStorage.setItem('user_session', JSON.stringify(user));
    }

    private getUserFromStorage(): User | null {
        const stored = localStorage.getItem('user_session');
        return stored ? JSON.parse(stored) : null;
    }

    getToken(): string | null {
        return this._currentUser()?.token || null;
    }
}
