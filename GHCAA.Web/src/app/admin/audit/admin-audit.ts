import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-admin-audit',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, SearchBarComponent],
    templateUrl: './admin-audit.html',
    styleUrl: './admin-audit.scss'
})
export class AdminAudit implements OnInit {
    private http = inject(HttpClient);

    logs = signal<any[]>([]);
    loading = signal(true);
    searchQuery = signal('');

    filteredLogs = computed(() => {
        const q = this.searchQuery().toLowerCase().trim();
        if (!q) return this.logs();
        return this.logs().filter(log =>
            (log.action || '').toLowerCase().includes(q) ||
            (log.details || '').toLowerCase().includes(q) ||
            (log.performedBy || '').toLowerCase().includes(q) ||
            (log.ipAddress || '').toLowerCase().includes(q)
        );
    });

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
