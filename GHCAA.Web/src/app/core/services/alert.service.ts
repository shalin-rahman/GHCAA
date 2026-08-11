import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { API_ENDPOINTS } from '../constants/app.constants';

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
    private apiUrl = API_ENDPOINTS.NOTIFICATIONS.BASE;

    notifications = signal<AppNotification[]>([]);
    unreadCount = signal<number>(0);

    constructor() {
        this.loadNotifications();
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
