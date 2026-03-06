import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS, EC_ROLES, getECPositionName } from '../../core/constants/app.constants';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-admin-governance',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-governance.html',
    styleUrl: './admin-governance.scss'
})
export class AdminGovernance implements OnInit {
    private http = inject(HttpClient);
    private adminService = inject(AdminService);
    private notify = inject(NotificationService);

    periods = signal<any[]>([]);
    selectedPeriod = signal<any>(null);
    committeeMembers = signal<any[]>([]);
    loading = signal(true);

    // Create/Edit Period
    showPeriodModal = signal(false);
    editPeriodData = signal<any>({ title: '', startDate: '', endDate: '', isActive: false });
    isSavingPeriod = signal(false);

    // Assign Member
    showAssignModal = signal(false);
    assignData = signal<any>({ memberId: null, position: 8, reason: '' });
    searchQuery = signal('');
    memberSearchResults = signal<any[]>([]);
    isSearching = signal(false);

    ecPositions = EC_ROLES.map((label, index) => ({ value: index, label }));

    ngOnInit() {
        this.loadPeriods();
    }

    loadPeriods() {
        this.loading.set(true);
        this.http.get<any[]>(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods`).subscribe({
            next: (data) => {
                this.periods.set(data);
                this.loading.set(false);
                // Auto select active period if none selected
                if (!this.selectedPeriod()) {
                    const active = data.find(p => p.isActive);
                    if (active) this.selectPeriod(active);
                }
            },
            error: () => this.loading.set(false)
        });
    }

    selectPeriod(period: any) {
        this.selectedPeriod.set(period);
        this.loadCommittee(period.id);
    }

    loadCommittee(periodId: number) {
        this.http.get<any[]>(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${periodId}/members`).subscribe({
            next: (data) => this.committeeMembers.set(data)
        });
    }

    openNewPeriod() {
        this.editPeriodData.set({ title: '', startDate: '', endDate: '', isActive: false });
        this.showPeriodModal.set(true);
    }

    savePeriod() {
        const data = this.editPeriodData();
        if (!data.title || !data.startDate) {
            this.notify.error('Title and Start Date are required');
            return;
        }

        this.isSavingPeriod.set(true);
        const api = `${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods`;

        this.http.post(api, data).subscribe({
            next: () => {
                this.notify.success('EC Period created');
                this.showPeriodModal.set(false);
                this.loadPeriods();
                this.isSavingPeriod.set(false);
            },
            error: () => {
                this.notify.error('Failed to create period');
                this.isSavingPeriod.set(false);
            }
        });
    }

    activatePeriod(id: number) {
        if (!confirm('Activating this period will deactivate all others. Continue?')) return;
        this.http.post(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${id}/activate`, {}).subscribe({
            next: () => {
                this.notify.success('Period activated');
                this.loadPeriods();
            }
        });
    }

    searchMembers(query: string) {
        if (query.length < 2) {
            this.memberSearchResults.set([]);
            return;
        }
        this.isSearching.set(true);
        this.adminService.getMembers().subscribe({
            next: (members) => {
                const lower = query.toLowerCase();
                this.memberSearchResults.set(members.filter(m =>
                    m.fullName?.toLowerCase().includes(lower) ||
                    m.membershipNumber?.toLowerCase().includes(lower)
                ).slice(0, 10));
                this.isSearching.set(false);
            }
        });
    }

    pickMember(member: any) {
        this.assignData.update(d => ({ ...d, memberId: member.id, memberName: member.fullName }));
        this.memberSearchResults.set([]);
        this.searchQuery.set(member.fullName);
    }

    assignRole() {
        const period = this.selectedPeriod();
        const data = this.assignData();
        if (!period || !data.memberId) return;

        this.http.post(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${period.id}/members`, data).subscribe({
            next: () => {
                this.notify.success('Role assigned');
                this.showAssignModal.set(false);
                this.loadCommittee(period.id);
                this.assignData.set({ memberId: null, position: 8, reason: '' });
                this.searchQuery.set('');
            },
            error: (err) => this.notify.error(err.error?.message || 'Assignment failed')
        });
    }

    removeMember(ecMemberId: number) {
        if (!confirm('Remove this member from the committee?')) return;
        this.http.delete(`${API_ENDPOINTS.ADMIN.GOVERNANCE}/members/${ecMemberId}`).subscribe({
            next: () => {
                this.notify.success('Member removed');
                this.loadCommittee(this.selectedPeriod().id);
            }
        });
    }

    getRoleName(pos: number) {
        return getECPositionName(pos);
    }
}
