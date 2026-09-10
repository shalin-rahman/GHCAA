import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { PaginationComponent } from '../../common/pagination/pagination.component';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';

@Component({
    selector: 'app-admin-error-logs',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, SearchBarComponent, PaginationComponent, LoadingPanelComponent],
    templateUrl: './admin-error-logs.html',
    styleUrl: './admin-error-logs.scss'
})
export class AdminErrorLogs implements OnInit {
    private adminService = inject(AdminService);

    logs = signal<any[]>([]);
    loading = signal(true);
    searchQuery = signal('');
    levelFilter = signal('all');
    fromDate = signal('');
    toDate = signal('');
    expandedId = signal<number | null>(null);

    currentPage = signal(1);
    pageSize = signal(20);
    totalPages = signal(1);
    totalItems = signal(0);

    ngOnInit() {
        this.loadLogs();
    }

    onFilterChange() {
        this.currentPage.set(1);
        this.loadLogs();
    }

    loadLogs() {
        this.loading.set(true);
        this.adminService.getErrorLogs(
            this.currentPage(),
            this.pageSize(),
            this.levelFilter(),
            this.searchQuery(),
            this.fromDate() ? this.toWireDate(this.fromDate()) : undefined,
            this.toDate() ? this.toWireDate(this.toDate(), true) : undefined
        ).subscribe({
            next: (res: any) => {
                this.logs.set(res.items || []);
                this.totalPages.set(res.totalPages || 1);
                this.totalItems.set(res.totalItems || 0);
                this.loading.set(false);
            },
            error: () => {
                this.logs.set([]);
                this.loading.set(false);
            }
        });
    }

    changePage(page: number) {
        this.currentPage.set(page);
        this.loadLogs();
    }

    toggleExpand(id: number) {
        this.expandedId.update(current => current === id ? null : id);
    }

    // Reuses the existing .status-badge state modifiers (styles.scss) instead of adding new
    // error/warning colors: 'terminated' is the badge system's red, 'pending' its amber.
    getLevelClass(level: string): string {
        return level === 'Error' ? 'terminated' : 'pending';
    }

    private toWireDate(dateInput: string, endOfDay = false): string | undefined {
        const d = new Date(dateInput);
        if (isNaN(d.getTime())) return undefined;
        if (endOfDay) d.setHours(23, 59, 59, 999);
        return d.toISOString();
    }
}
