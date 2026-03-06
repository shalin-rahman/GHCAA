import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-admin-roles',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-roles.html',
    styleUrl: './admin-roles.scss'
})
export class AdminRoles implements OnInit {
    private http = inject(HttpClient);
    private notify = inject(NotificationService);

    users = signal<any[]>([]);
    roles = signal<any[]>([]);
    loading = signal(true);

    searchQuery = signal('');

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);
        // Fetch users (members who have a user account)
        this.http.get<any[]>('/api/admin/members').subscribe({
            next: (members) => {
                this.users.set(members.filter(m => m.username)); // Only those with login accounts
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });

        // Fetch available roles
        this.http.get<any[]>('/api/roles').subscribe({
            next: (roles) => this.roles.set(roles)
        });
    }

    assignRole(userId: number, roleName: string) {
        this.http.post('/api/roles/assign', { userId, roleName }).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} assigned`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to assign role')
        });
    }

    removeRole(userId: number, roleName: string) {
        this.http.post('/api/roles/remove', { userId, roleName }).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} removed`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to remove role')
        });
    }
}
