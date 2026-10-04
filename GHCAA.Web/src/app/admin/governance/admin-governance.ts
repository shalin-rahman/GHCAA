import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { LOOKUP_GROUPS, SEARCH_DEBOUNCE_MS, getECPositionName, getMembershipTypeLabel, getCategoryLabel, VACANCY_REASON_OPTIONS, VACANCY_REASON_OTHER, VACANCY_NOTE_MAX_LENGTH, getVacancyReasonLabel } from '../../core/constants/app.constants';
import { LookupService, LookupOption } from '../../core/services/lookup.service';
import { debounce } from '../../core/utils/debounce.util';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { firstValueFrom } from 'rxjs';
import { AdminService } from '../../core/services/admin.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { NotifyToggleComponent } from '../../common/notify-toggle/notify-toggle.component';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';

@Component({
    selector: 'app-admin-governance',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, NotifyToggleComponent, ImgFallbackDirective, ModalHeaderComponent],
    templateUrl: './admin-governance.html',
    styleUrl: './admin-governance.scss'
})
export class AdminGovernance implements OnInit {
    private adminService = inject(AdminService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    private lookupService = inject(LookupService);

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
    assignData = signal<any>(this.emptyAssignData());
    searchQuery = signal('');
    memberSearchResults = signal<any[]>([]);
    isSearching = signal(false);

    // End a term (95.3). The dialog asks why, and keeps the 82.52 notify toggle.
    removingMemberId = signal<number | null>(null);
    removeNotifyMember = signal(false);
    removeReason = signal('');
    removeNote = signal('');
    removingMember = computed(() => this.committeeMembers().find(m => m.id === this.removingMemberId()));

    // Every seat in the period, so empty ones show why they are empty.
    seats = signal<any[]>([]);
    vacantSeats = computed(() => this.seats().filter(s => !s.holder));

    vacancyReasons = VACANCY_REASON_OPTIONS;
    vacancyReasonOther = VACANCY_REASON_OTHER;
    vacancyNoteMaxLength = VACANCY_NOTE_MAX_LENGTH;
    getVacancyReasonLabel = getVacancyReasonLabel;

    committeeSearch = signal('');
    filteredMembers = computed(() => {
        const q = this.committeeSearch().toLowerCase();
        return this.committeeMembers().filter(m =>
            m.member?.fullName?.toLowerCase().includes(q) ||
            m.member?.membershipNumber?.toLowerCase().includes(q) ||
            this.getRoleName(m.position).toLowerCase().includes(q)
        );
    });

    // 82.42: sourced from the ECPosition lookup group instead of a local index-based copy of EC_ROLES.
    ecPositions: LookupOption[] = [];

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
        this.lookupService.getOptions(LOOKUP_GROUPS.ECPosition).subscribe(opts => this.ecPositions = opts);
    }

    loadPeriods() {
        this.loading.set(true);
        this.adminService.getGovernancePeriods().subscribe({
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
        this.adminService.getCommitteeMembers(periodId).subscribe({
            next: (data) => this.committeeMembers.set(data),
            // 29D.8: without this the committee list silently stayed empty on failure,
            // indistinguishable from a genuinely empty committee.
            error: () => this.notify.error('Failed to load committee members.')
        });
        this.adminService.getCommitteeSeats(periodId).subscribe({
            next: (data) => this.seats.set(data),
            error: () => this.seats.set([])
        });
    }

    openNewPeriod() {
        this.editPeriodData.set({ id: null, title: '', startDate: '', endDate: '', isActive: false });
        this.showPeriodModal.set(true);
    }

    formatDateToDMY(d: any) {
        return toDisplayDate(d);
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

        // Format payload to ISO wire dates; empty dates are sent as null, avoiding ASP.NET 400 JSON conversion errors
        const payload = { ...data, startDate: toWireDate(data.startDate), endDate: toWireDate(data.endDate) };
        const request = isEdit
            ? this.adminService.updateGovernancePeriod(data.id, payload)
            : this.adminService.createGovernancePeriod(payload);

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

    async activatePeriod(id: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Activate period',
            message: 'Activating this period will deactivate all others. Continue?',
            confirmLabel: 'Activate'
        }));
        if (!ok) return;

        this.adminService.activateGovernancePeriod(id).subscribe({
            next: () => {
                this.notify.success('Period activated');
                this.loadPeriods();
            },
            error: () => this.notify.error('Failed to activate period.')
        });
    }

    private debouncedMemberSearch = debounce((query: string) => this.runMemberSearch(query), SEARCH_DEBOUNCE_MS);

    searchMembers(query: string) {
        if (query.length < 2) {
            this.memberSearchResults.set([]);
            return;
        }
        this.debouncedMemberSearch(query);
    }

    private runMemberSearch(query: string) {
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
        // The API refuses to seat a second holder, so ask for the outgoing holder's reason here.
        const holder = this.holderOf(data.position);
        if (holder && !this.reasonIsComplete(data.endCurrentHolderReason, data.endCurrentHolderNote)) {
            this.notify.error(`Say why ${holder.member?.fullName || 'the current holder'}'s term ended.`);
            return;
        }

        const payload = {
            ...data,
            endCurrentHolderReason: holder ? data.endCurrentHolderReason : null,
            endCurrentHolderNote: holder ? (data.endCurrentHolderNote?.trim() || null) : null
        };
        this.adminService.assignCommitteeRole(period.id, payload).subscribe({
            next: () => {
                this.notify.success('Role assigned');
                this.showAssignModal.set(false);
                this.loadCommittee(period.id);
                this.assignData.set(this.emptyAssignData());
                this.searchQuery.set('');
            },
            error: (err) => this.notify.error(err.error?.detail || err.error?.message || 'Assignment failed')
        });
    }

    // Read on every change detection because the position select writes into assignData() in place.
    holderOf(position: number | string | null | undefined) {
        if (position === null || position === undefined || position === '') return undefined;
        const name = getECPositionName(position);
        return this.committeeMembers().find(m => getECPositionName(m.position) === name);
    }

    removeMember(ecMemberId: number) {
        this.removeNotifyMember.set(false);
        this.removeReason.set('');
        this.removeNote.set('');
        this.removingMemberId.set(ecMemberId);
    }

    canEndTerm(): boolean {
        return this.reasonIsComplete(this.removeReason(), this.removeNote());
    }

    confirmRemoveMember() {
        const ecMemberId = this.removingMemberId();
        if (ecMemberId == null || !this.canEndTerm()) return;
        this.adminService.endCommitteeTerm(ecMemberId, {
            reason: this.removeReason(),
            note: this.removeNote().trim() || null,
            notifyMember: this.removeNotifyMember()
        }).subscribe({
            next: () => {
                this.notify.success('Term ended');
                this.removingMemberId.set(null);
                this.loadCommittee(this.selectedPeriod().id);
            },
            error: (err) => this.notify.error(err.error?.detail || 'Failed to end the term.')
        });
    }

    private reasonIsComplete(reason: string | null | undefined, note: string | null | undefined): boolean {
        if (!reason) return false;
        return reason !== VACANCY_REASON_OTHER || !!note?.trim();
    }

    private emptyAssignData() {
        return { memberId: null, position: '', reason: '', notifyMember: false, endCurrentHolderReason: '', endCurrentHolderNote: '' };
    }

    getRoleName(pos: number | string) {
        return getECPositionName(pos);
    }
}

