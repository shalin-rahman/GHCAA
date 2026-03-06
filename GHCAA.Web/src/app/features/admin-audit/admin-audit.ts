import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-admin-audit',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './admin-audit.html',
    styleUrl: './admin-audit.scss'
})
export class AdminAudit implements OnInit {
    private http = inject(HttpClient);

    logs = signal<any[]>([]);
    loading = signal(true);

    ngOnInit() {
        this.loadLogs();
    }

    loadLogs() {
        this.loading.set(true);
        this.http.get<any[]>('/api/activity/admin/global').subscribe({
            next: (data) => {
                this.logs.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getActionIcon(action: string): string {
        const map: Record<string, string> = {
            'Login': '🔑',
            'Update': '📝',
            'Create': '🆕',
            'Delete': '🗑️',
            'Archive': '🗄️',
            'Approve': '✅',
            'Email': '✉️',
            'Payment': '💰'
        };
        return map[action] || '📜';
    }
}
