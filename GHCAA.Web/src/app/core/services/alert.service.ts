import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api.endpoints';

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
        this.http.get<AppNotification[]>(this.apiUrl).subscribe(data => {
            this.notifications.set(data);
            this.unreadCount.set(data.filter(n => !n.isRead).length);
        });
    }

    markAsRead(id: number) {
        this.http.post(`${this.apiUrl}/${id}/read`, {}).subscribe(() => {
            this.loadNotifications();
        });
    }

    markAllAsRead() {
        this.http.post(API_ENDPOINTS.NOTIFICATIONS.READ_ALL, {}).subscribe(() => {
            this.loadNotifications();
        });
    }
}
