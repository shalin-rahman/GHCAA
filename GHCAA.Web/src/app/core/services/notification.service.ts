import { Injectable, signal } from '@angular/core';

export interface Toast {
    id: number;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    toasts = signal<Toast[]>([]);
    private counter = 0;

    show(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info') {
        const id = this.counter++;
        const toast: Toast = { id, message, type };
        this.toasts.set([...this.toasts(), toast]);

        // Auto-remove after 5 seconds
        setTimeout(() => this.remove(id), 5000);
    }

    success(msg: string) { this.show(msg, 'success'); }
    error(msg: string) { this.show(msg, 'error'); }
    info(msg: string) { this.show(msg, 'info'); }

    remove(id: number) {
        this.toasts.set(this.toasts().filter(t => t.id !== id));
    }
}
