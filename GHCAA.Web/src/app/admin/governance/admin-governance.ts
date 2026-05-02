import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS, EC_ROLES, getECPositionName, getMembershipTypeLabel, getCategoryLabel } from '../../core/constants/app.constants';
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

    committeeSearch = signal('');
    filteredMembers = computed(() => {
        const q = this.committeeSearch().toLowerCase();
        return this.committeeMembers().filter(m =>
            m.member?.fullName?.toLowerCase().includes(q) ||
            m.member?.membershipNumber?.toLowerCase().includes(q) ||
            this.getRoleName(m.position).toLowerCase().includes(q)
        );
    });

    ecPositions = EC_ROLES.map((label, index) => ({ value: index, label }));

    getImageUrl(path: string | null | undefined): string {
        if (!path) return '';
        if (path.startsWith('http')) return path;
        const cleanPath = path.startsWith('/') ? path : '/' + path;
        return cleanPath.replace(/^\/\//, '/');
    }

    getMembershipTypeLabel = getMembershipTypeLabel;
    getCategoryLabel = getCategoryLabel;

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
        this.editPeriodData.set({ id: null, title: '', startDate: '', endDate: '', isActive: false });
        this.showPeriodModal.set(true);
    }

    formatDateToDMY(d: any) {
        if (!d) return '';
        const date = new Date(d);
        if (isNaN(date.getTime())) return d;
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}-${month}-${year}`;
    }

    editPeriod(period: any) {
        this.editPeriodData.set({
            id: period.id,
            title: period.title,
            startDate: this.formatDateToDMY(period.startDate),
            endDate: this.formatDateToDMY(period.endDate),
            isActive: period.isActive
        });
        this.showPeriodModal.set(true);
    }

    savePeriod() {
        const data = this.editPeriodData();
        if (!data.title || !data.startDate) {
            this.notify.error('Title and Start Date are required');
            return;
        }

        this.isSavingPeriod.set(true);
        const isEdit = !!data.id;
        const api = isEdit ? `${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods/${data.id}` : `${API_ENDPOINTS.ADMIN.GOVERNANCE}/periods`;

        // Format payload to ensure empty dates are sent as null, avoiding ASP.NET 400 JSON conversion errors
        const payload = { ...data, endDate: data.endDate ? data.endDate : null };
        const request = isEdit ? this.http.put(api, payload) : this.http.post(api, payload);
        
        request.subscribe({
            next: () => {
                this.notify.success(isEdit ? 'EC Period updated' : 'EC Period created');
                this.showPeriodModal.set(false);
                this.loadPeriods();
                this.isSavingPeriod.set(false);
            },
            error: () => {
                this.notify.error(isEdit ? 'Failed to update period' : 'Failed to create period');
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
        this.adminService.getMembers(1, 50, query).subscribe({
            next: (response: any) => {
                const searchTerms = query.toLowerCase().split(' ').filter(p => p.trim() !== '');
                const memberArray = response.items || response || [];
                
                this.memberSearchResults.set(memberArray.filter((m: any) => {
                    const fullName = (m.fullName || '').toLowerCase();
                    const memberId = (m.membershipNumber || '').toLowerCase();
                    const mobile = (m.mobileNo || '').toLowerCase();

                    // Every space-separated search term must naturally appear in the person's info
                    return searchTerms.every(term => 
                        fullName.includes(term) || 
                        memberId.includes(term) || 
                        mobile.includes(term)
                    );
                }));
                
                this.isSearching.set(false);
            },
            error: () => this.isSearching.set(false)
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
        if (!period) return;
        if (!data.memberId) {
            this.notify.error('Please search and select a member first.');
            return;
        }

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


