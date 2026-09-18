import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, DevTrackerItem } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';

interface DevTrackerGroup {
    workPackageNumber: number;
    workPackageTitle: string;
    items: DevTrackerItem[];
}

@Component({
    selector: 'app-admin-dev-tracker',
    standalone: true,
    imports: [CommonModule, FormsModule, PageHeaderComponent, LoadingPanelComponent],
    templateUrl: './admin-dev-tracker.html',
    styleUrl: './admin-dev-tracker.scss'
})
export class AdminDevTracker implements OnInit {
    private adminService = inject(AdminService);
    private notify = inject(NotificationService);

    items = signal<DevTrackerItem[]>([]);
    loading = signal(true);
    priorityFilter = signal('all');

    // 82.115: groups the flat list by work package for display, in the order the API
    // already returns them (WP number, then priority).
    groups = signal<DevTrackerGroup[]>([]);

    ngOnInit() {
        this.loadItems();
    }

    onFilterChange() {
        this.loadItems();
    }

    loadItems() {
        this.loading.set(true);
        this.adminService.getDevTrackerItems(this.priorityFilter()).subscribe({
            next: (items) => {
                this.items.set(items);
                this.groups.set(this.groupByWorkPackage(items));
                this.loading.set(false);
            },
            error: (err) => {
                this.items.set([]);
                this.groups.set([]);
                this.loading.set(false);
                this.notify.error(err.error?.detail || 'Could not load the developer tracker.');
            }
        });
    }

    // Reuses the .status-badge state modifiers instead of adding new colors for TODO/PARTIAL.
    getStatusClass(status: string): string {
        return status === 'PARTIAL' ? 'pending' : 'terminated';
    }

    private groupByWorkPackage(items: DevTrackerItem[]): DevTrackerGroup[] {
        const groups: DevTrackerGroup[] = [];
        for (const item of items) {
            let group = groups.find(g => g.workPackageNumber === item.workPackageNumber);
            if (!group) {
                group = { workPackageNumber: item.workPackageNumber, workPackageTitle: item.workPackageTitle, items: [] };
                groups.push(group);
            }
            group.items.push(item);
        }
        return groups;
    }
}
