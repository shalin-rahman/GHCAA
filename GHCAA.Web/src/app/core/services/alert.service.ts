import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface AppNotification {
    id: number;
    title: string;
    message: string;
    type: string;
    targetUrl?: string;
    createdAt: string;
    isRead: boolean;
}

@Injectable({ providedIn: 'root' })
export class AlertService {
    private http = inject(HttpClient);
    notifications = signal<AppNotification[]>([]);
    unreadCount = signal(0);

    loadNotifications() {
        this.http.get<AppNotification[]>('/api/notifications').subscribe(data => {
            this.notifications.set(data);
            this.unreadCount.set(data.filter(n => !n.isRead).length);
        });
    }

    markAsRead(id: number) {
        this.http.post(`/api/notifications/${id}/read`, {}).subscribe(() => {
            this.loadNotifications();
        });
    }

    markAllAsRead() {
        this.http.post('/api/notifications/read-all', {}).subscribe(() => {
            this.loadNotifications();
        });
    }
}
