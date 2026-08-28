import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AuthService } from './auth.service';

export interface AppNotification {
    id: number;
    title: string;
    message: string;
    type: string;
    targetUrl?: string;
    createdAt: string;
    isRead: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private http = inject(HttpClient);
    private auth = inject(AuthService);
    private apiUrl = API_ENDPOINTS.NOTIFICATIONS.BASE;

    notifications = signal<AppNotification[]>([]);
    unreadCount = signal<number>(0);

    constructor() {
        // GET /api/notifications is [Authorize] — calling it unconditionally for a guest (this
        // service is constructed from the header/nav, present on every page) 401s and, before
        // this gate, fed the interceptor's refresh→logout cascade exactly like the /auth/me
        // restore did. authChecked() is already true on construction whenever a cached session
        // was found, so the logged-in fast path still loads immediately.
        this.auth.whenAuthenticated(() => this.loadNotifications());
    }

    loadNotifications() {
        // 29F.2: surface HTTP failures instead of failing silently
        this.http.get<AppNotification[]>(this.apiUrl).subscribe({
            next: data => {
                this.notifications.set(data);
                this.unreadCount.set(data.filter(n => !n.isRead).length);
            },
            error: err => console.error('Failed to load notifications', err)
        });
    }

    markAsRead(id: number) {
        this.http.post(`${this.apiUrl}/${id}/read`, {}).subscribe({
            next: () => {
                this.loadNotifications();
            },
            error: err => console.error('Failed to mark notification as read', err)
        });
    }

    markAllAsRead() {
        this.http.post(API_ENDPOINTS.NOTIFICATIONS.READ_ALL, {}).subscribe({
            next: () => {
                this.loadNotifications();
            },
            error: err => console.error('Failed to mark all notifications as read', err)
        });
    }
}
