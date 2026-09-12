import { ChangeDetectionStrategy, Component, inject, signal, OnInit, computed } from '@angular/core';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';

@Component({
    selector: 'app-admin-audit',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, PageHeaderComponent, SearchBarComponent, LoadingPanelComponent],
    templateUrl: './admin-audit.html',
    styleUrl: './admin-audit.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminAudit implements OnInit {
    private adminService = inject(AdminService);

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
        this.adminService.getGlobalActivityLog().subscribe({
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
