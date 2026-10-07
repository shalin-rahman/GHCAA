import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { MentorshipService } from '../../core/services/mentorship.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { MentorshipAdminRow } from '../../core/models/business.models';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { getMentorshipStatusLabel, getMentorshipStatusClass } from '../../core/constants/app.constants';

export type MentorshipTab = 'All' | 'Pending' | 'Accepted' | 'Completed' | 'Declined';

@Component({
    selector: 'app-admin-mentorship',
    standalone: true,
    imports: [CommonModule, LoadingPanelComponent, PageHeaderComponent, SearchBarComponent, ModalHeaderComponent],
    templateUrl: './admin-mentorship.html'
})
export class AdminMentorship implements OnInit {
    private mentorshipService = inject(MentorshipService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    getMentorshipStatusLabel = getMentorshipStatusLabel;
    getMentorshipStatusClass = getMentorshipStatusClass;

    readonly tabs: MentorshipTab[] = ['All', 'Pending', 'Accepted', 'Completed', 'Declined'];

    loading = signal(true);
    requests = signal<MentorshipAdminRow[]>([]);
    searchTerm = signal('');
    activeTab = signal<MentorshipTab>('All');
    selected = signal<MentorshipAdminRow | null>(null);
    closeNote = signal('');
    closing = signal(false);

    // The API may send the status as its enum name or its number, so compare on the label.
    counts = computed(() => {
        const result: Record<MentorshipTab, number> = { All: 0, Pending: 0, Accepted: 0, Completed: 0, Declined: 0 };
        for (const r of this.requests()) {
            result.All++;
            const label = getMentorshipStatusLabel(r.status) as MentorshipTab;
            if (label in result) result[label]++;
        }
        return result;
    });

    filteredRequests = computed(() => {
        const tab = this.activeTab();
        const term = this.searchTerm().toLowerCase();
        return this.requests().filter(r =>
            (tab === 'All' || getMentorshipStatusLabel(r.status) === tab) &&
            (!term ||
                r.requester?.fullName?.toLowerCase().includes(term) ||
                r.mentor?.fullName?.toLowerCase().includes(term) ||
                r.domain?.toLowerCase().includes(term))
        );
    });

    ngOnInit() {
        this.loadRequests();
    }

    canClose(r: MentorshipAdminRow): boolean {
        const label = getMentorshipStatusLabel(r.status);
        return label === 'Pending' || label === 'Accepted';
    }

    closeOutcome(r: MentorshipAdminRow): string {
        return getMentorshipStatusLabel(r.status) === 'Pending' ? 'Declined' : 'Completed';
    }

    open(r: MentorshipAdminRow) {
        this.closeNote.set('');
        this.selected.set(r);
    }

    dismiss() {
        if (this.closing()) return;
        this.selected.set(null);
    }

    async closeRequest(r: MentorshipAdminRow) {
        if (!this.canClose(r) || this.closing()) return;
        const outcome = this.closeOutcome(r);
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Close mentorship request',
            message: `Mark this request as ${outcome}? Both members get a notification.`,
            confirmLabel: 'Close request',
            danger: true
        }));
        if (!ok) return;

        this.closing.set(true);
        const note = this.closeNote().trim() || undefined;
        this.mentorshipService.adminClose(r.id, note).subscribe({
            next: () => {
                this.closing.set(false);
                this.selected.set(null);
                this.notify.success(`Request marked as ${outcome}.`);
                this.loadRequests();
            },
            error: () => {
                this.closing.set(false);
                this.notify.error('Could not close the request. It may already be closed.');
            }
        });
    }

    private loadRequests() {
        this.loading.set(true);
        this.mentorshipService.getAllForAdmin().subscribe({
            next: (data) => { this.requests.set(data); this.loading.set(false); },
            error: () => {
                this.loading.set(false);
                this.notify.error('Could not load mentorship requests.');
            }
        });
    }
}
