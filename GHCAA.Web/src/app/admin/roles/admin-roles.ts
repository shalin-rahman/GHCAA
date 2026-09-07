import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { AdminService } from '../../core/services/admin.service';
import { Icon } from '../../common/icon/icon';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';

@Component({
    selector: 'app-admin-roles',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, PageHeaderComponent, SearchBarComponent, Icon, LogoSpinnerComponent, ModalHeaderComponent],
    templateUrl: './admin-roles.html',
    styleUrl: './admin-roles.scss'
})
export class AdminRoles implements OnInit {
    private adminService = inject(AdminService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    private fb = inject(FormBuilder);

    users = signal<any[]>([]);
    roles = signal<any[]>([]);
    loading = signal(true);
    showCreateForm = signal(false);
    submitting = signal(false);
    searchQuery = signal('');

    filteredUsers = computed(() => {
        const q = this.searchQuery().toLowerCase().trim();
        if (!q) return this.users();
        return this.users().filter(u =>
            (u.userName || '').toLowerCase().includes(q) ||
            (u.email || '').toLowerCase().includes(q)
        );
    });

    createForm = this.fb.group({
        username: ['', [Validators.required, Validators.minLength(3)]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        role: ['Admin', Validators.required]
    });

    customRoleName = signal('');
    creatingRole = signal(false);
    showPassword = signal(false);

    togglePassword() {
        this.showPassword.set(!this.showPassword());
    }

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading.set(true);
        // Fetch all system users (identity users)
        this.adminService.getSystemUsers().subscribe({
            next: (users) => {
                this.users.set(users);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });

        // Fetch available roles
        this.adminService.getRoles().subscribe({
            next: (roles) => this.roles.set(roles),
            // 29F.2: surface failures instead of leaving the role list silently empty.
            error: () => this.notify.error('Failed to load available roles.')
        });
    }

    createAdmin() {
        if (this.createForm.invalid) {
            this.createForm.markAllAsTouched();
            this.notify.error('Please provide a valid username and password (min 6 chars).');
            return;
        }
        this.submitting.set(true);
        this.adminService.createSystemUser(this.createForm.value).subscribe({
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

    createCustomRole() {
        const role = this.customRoleName().trim();
        if (!role) {
            this.notify.error('Role name cannot be empty');
            return;
        }
        this.creatingRole.set(true);
        this.adminService.createCustomRole(role).subscribe({
            next: () => {
                this.notify.success(`Custom role '${role}' created successfully`);
                this.customRoleName.set('');
                this.creatingRole.set(false);
                this.loadData();
            },
            error: (err) => {
                this.notify.error(err.error?.message || 'Failed to create role');
                this.creatingRole.set(false);
            }
        });
    }

    assignRole(userId: number, roleName: string) {
        this.adminService.assignUserRole(userId, roleName).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} assigned`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to assign role')
        });
    }

    // 54.4: "Assign" only ever added a role — there was no single action to replace a user's one
    // existing role with another, so an admin had to manually remove the old chip and pick a new
    // one. This does remove-then-assign as one click, and is only offered when the user has
    // exactly one role (multi-role users keep the original additive Assign + per-chip remove flow,
    // since "replace which one?" isn't unambiguous there).
    updateRole(userId: number, oldRole: string, newRole: string) {
        if (oldRole === newRole) return;
        this.adminService.removeUserRole(userId, oldRole).subscribe({
            next: () => {
                this.adminService.assignUserRole(userId, newRole).subscribe({
                    next: () => {
                        this.notify.success(`Role updated to ${newRole}`);
                        this.loadData();
                    },
                    error: () => this.notify.error('Role removed but failed to assign the new one — please assign it manually.')
                });
            },
            error: () => this.notify.error('Failed to update role')
        });
    }

    removeRole(userId: number, roleName: string) {
        this.adminService.removeUserRole(userId, roleName).subscribe({
            next: () => {
                this.notify.success(`Role ${roleName} removed`);
                this.loadData();
            },
            error: () => this.notify.error('Failed to remove role')
        });
    }

    async resetPassword(user: any) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Reset password',
            message: `Reset the password for system administrator "${user.username}"? Any active session will be signed out.`,
            confirmLabel: 'Reset',
            danger: true
        }));
        if (!ok) return;

        this.adminService.resetSystemUserPassword(user.id).subscribe({
            next: (res) => {
                if (res?.resetUrl) {
                    if (navigator.clipboard) {
                        navigator.clipboard.writeText(res.resetUrl).then(() => {
                            this.notify.success('Reset link copied to clipboard. Share it with the admin.');
                        }).catch(() => {
                            prompt('Password Reset Link:', res.resetUrl);
                        });
                    } else {
                        prompt('Password Reset Link:', res.resetUrl);
                    }
                } else {
                    this.notify.success('Password reset link generated.');
                }
            },
            error: (err) => this.notify.error(err.error?.message || 'Failed to generate reset link')
        });
    }

    toggleUserActive(userId: number, isActive: boolean) {
        const action = isActive ? 'disable' : 'enable';
        this.adminService.setSystemUserActive(userId, action).subscribe({
            next: () => {
                this.notify.success(isActive ? 'User disabled' : 'User enabled');
                this.loadData();
            },
            error: (err) => this.notify.error(err.error?.message || `Failed to ${action} user`)
        });
    }

    async deleteUser(user: any) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete system administrator',
            message: `Permanently delete system administrator "${user.username}"?`,
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.adminService.deleteSystemUser(user.id).subscribe({
            next: () => {
                this.notify.success('System administrator account deleted');
                this.loadData();
            },
            error: (err) => this.notify.error(err.error?.message || 'Failed to delete account')
        });
    }
}


