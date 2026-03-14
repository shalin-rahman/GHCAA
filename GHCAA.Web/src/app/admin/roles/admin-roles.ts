import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-admin-roles',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './admin-roles.html',
    styleUrl: './admin-roles.scss'
})
export class AdminRoles implements OnInit {
    private http = inject(HttpClient);
    private notify = inject(NotificationService);
    private fb = inject(FormBuilder);

    users = signal<any[]>([]);
    roles = signal<any[]>([]);
    loading = signal(true);
    showCreateForm = signal(false);
    submitting = signal(false);

    createForm = this.fb.group({
        username: ['', [Validators.required, Validators.minLength(3)]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        role: ['Admin', Validators.required]
    });

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);
        // Fetch all system users (identity users)
        this.http.get<any[]>('/api/roles/users').subscribe({
            next: (users) => {
                this.users.set(users);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });

        // Fetch available roles
        this.http.get<any[]>('/api/roles').subscribe({
            next: (roles) => this.roles.set(roles)
        });
    }

    createAdmin() {
        if (this.createForm.invalid) return;
        this.submitting.set(true);
        this.http.post('/api/roles/users', this.createForm.value).subscribe({
            next: () => {
                this.notify.success('System user created successfully');
                this.showCreateForm.set(false);
                this.createForm.reset({ role: 'Admin' });
                this.submitting.set(false);
                this.loadData();
            },
            error: (err) => {
                this.notify.error(err.error?.message || 'Failed to create user');
                this.submitting.set(false);
            }
        });
    }

    assignRole(userId: number, roleName: string) {
        this.http.post('/api/roles/assign', null, { params: { userId, roleName } }).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} assigned`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to assign role')
        });
    }

    removeRole(userId: number, roleName: string) {
        this.http.post('/api/roles/remove', null, { params: { userId, roleName } }).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} removed`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to remove role')
        });
    }
}


