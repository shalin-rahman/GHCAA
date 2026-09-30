import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ElectionsService } from '../../core/services/elections.service';
import {
    AdminElectionDto, AdminElectionStepResult, CreateElectionRequest, ELECTION_APPROVAL_ACTION_LABELS, ELECTION_PHASE_ORDER,
    ElectionApprovalDto, NominationViewDto
} from '../../core/models/election.models';
import { ExportUtil } from '../../core/utils/export.util';
import { ballotKeyFileName, generateBallotKeyPair, readBallotKeyFile, toPem } from '../../core/utils/ballot-key.util';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import {
    getElectionPhaseLabel, getElectionPhaseClass,
    getNominationStatusLabel, getNominationStatusClass
} from '../../core/constants/app.constants';

type AdminElectionsTab = 'elections' | 'nominations';

@Component({
    selector: 'app-admin-elections',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, SearchBarComponent, PageHeaderComponent],
    templateUrl: './admin-elections.html'
})
export class AdminElections {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);

    getElectionPhaseLabel = getElectionPhaseLabel;
    getElectionPhaseClass = getElectionPhaseClass;
    getNominationStatusLabel = getNominationStatusLabel;
    getNominationStatusClass = getNominationStatusClass;

    activeTab = signal<AdminElectionsTab>('elections');

    elections = signal<AdminElectionDto[]>([]);
    search = signal('');
    loading = signal(true);
    saving = signal(false);
    showForm = signal(false);

    publishingId = signal<number | null>(null);
    closingId = signal<number | null>(null);
    addingCandidateId = signal<number | null>(null);
    removingCandidateId = signal<number | null>(null);
    keyingId = signal<number | null>(null);
    countingId = signal<number | null>(null);
    approvals = signal<Partial<Record<number, ElectionApprovalDto[]>>>({});
    decidingApprovalId = signal<number | null>(null);
    approvalLabels = ELECTION_APPROVAL_ACTION_LABELS;

    filtered = computed(() => {
        const query = this.search().trim().toLowerCase();
        return query ? this.elections().filter(election => election.title.toLowerCase().includes(query)) : this.elections();
    });

    form: CreateElectionRequest = this.emptyForm();

    // Nominations/scrutiny tab.
    selectedElectionId = signal<number | null>(null);
    nominations = signal<NominationViewDto[]>([]);
    nominationsLoading = signal(false);
    scrutinisingId = signal<number | null>(null);

    constructor() { this.load(); }

    switchTab(tab: AdminElectionsTab): void {
        this.activeTab.set(tab);
        if (tab === 'nominations' && this.nominations().length === 0) this.loadNominations();
    }

    load(): void {
        this.loading.set(true);
        this.electionsService.getAdminElections().subscribe({
            next: values => {
                this.elections.set(values);
                this.loading.set(false);
                if (this.selectedElectionId() === null && values.length > 0) this.selectedElectionId.set(values[0].id);
            },
            error: () => { this.loading.set(false); this.notify.error('Failed to load elections.'); }
        });
    }

    private emptyForm(): CreateElectionRequest {
        return {
            title: '', ecPeriodId: 0, nominationOpensOn: '', nominationClosesOn: '',
            pollingOpensOn: '', pollingClosesOn: '',
            description: '', positions: [{ title: '', seats: 1 }]
        };
    }

    addPosition(): void {
        this.form.positions = [...(this.form.positions ?? []), { title: '', seats: 1 }];
    }

    removePosition(index: number): void {
        const positions = this.form.positions ?? [];
        if (positions.length > 1) this.form.positions = positions.filter((_, i) => i !== index);
    }

    create(): void {
        if (this.saving()) return;
        if (!this.form.title.trim() || (this.form.positions ?? []).some(position => !position.title.trim())) {
            this.notify.warning('Add a title and each election position.');
            return;
        }
        this.saving.set(true);
        this.electionsService.create(this.form).subscribe({
            next: election => {
                this.elections.set([election, ...this.elections()]);
                this.showForm.set(false);
                this.saving.set(false);
                this.form = this.emptyForm();
                this.notify.success('Election created.');
            },
            error: () => { this.saving.set(false); this.notify.error('Failed to create election.'); }
        });
    }

    publish(election: AdminElectionDto): void {
        if (this.publishingId() !== null) return;
        this.publishingId.set(election.id);
        this.electionsService.publish(election.id).subscribe({
            next: result => { this.publishingId.set(null); this.applyStep(result); },
            error: () => { this.publishingId.set(null); this.notify.error('Failed to publish election.'); }
        });
    }

    async close(election: AdminElectionDto): Promise<void> {
        if (this.closingId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Close election',
            message: `Close "${election.title}"? Voting will no longer be accepted.`,
            confirmLabel: 'Close election',
            danger: true
        }));
        if (!confirmed) return;

        this.closingId.set(election.id);
        this.electionsService.close(election.id).subscribe({
            next: result => { this.closingId.set(null); this.applyStep(result); },
            error: () => { this.closingId.set(null); this.notify.error('Failed to close election.'); }
        });
    }

    async removeCandidate(election: AdminElectionDto, candidateId: number): Promise<void> {
        if (this.removingCandidateId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Remove candidate',
            message: 'Remove this candidate from the election?',
            confirmLabel: 'Remove',
            danger: true
        }));
        if (!confirmed) return;

        this.removingCandidateId.set(candidateId);
        this.electionsService.removeCandidate(election.id, candidateId).subscribe({
            next: () => { this.removingCandidateId.set(null); this.load(); },
            error: () => { this.removingCandidateId.set(null); this.notify.error('Failed to remove candidate.'); }
        });
    }

    // Spec 023 FR-001. The key can be set until polling opens, and polling will not open without it.
    canSetBallotKey(election: AdminElectionDto): boolean {
        return ELECTION_PHASE_ORDER.indexOf(election.phase) < ELECTION_PHASE_ORDER.indexOf('Polling');
    }

    async createBallotKey(election: AdminElectionDto): Promise<void> {
        if (this.keyingId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: election.ballotKeyFingerprint ? 'Replace returning officer key' : 'Make returning officer key',
            message: 'A key file will download to this device. Only the returning officer should keep it. '
                + 'The count cannot run without it, and nobody, including the system, can recover it if it is lost.',
            confirmLabel: 'Make key'
        }));
        if (!confirmed) return;

        this.keyingId.set(election.id);
        try {
            const pair = await generateBallotKeyPair();
            // Save the private key before the server accepts the public one, so a failed download
            // never leaves an election sealed under a key nobody has.
            ExportUtil.saveFile(new Blob([toPem(pair.privateKey)], { type: 'application/x-pem-file' }), ballotKeyFileName(election.title));
            const result = await firstValueFrom(this.electionsService.setBallotKey(election.id, pair.publicKey));
            if (result.election) {
                this.replace(result.election);
                this.notify.success('Key stored. Keep the downloaded file safe until the count.');
            } else {
                // The old key stays in force until a second person approves. Keep both files until then.
                this.applyStep(result);
            }
        } catch (err) {
            // The HTTP interceptor already shows the server's reason for a failed request.
            if (!(err instanceof HttpErrorResponse)) this.notify.error('This browser could not make the key.');
        } finally {
            this.keyingId.set(null);
        }
    }

    async countWithKeyFile(election: AdminElectionDto, event: Event): Promise<void> {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        input.value = '';
        if (!file || this.countingId() !== null) return;

        this.countingId.set(election.id);
        try {
            const results = await firstValueFrom(this.electionsService.count(election.id, readBallotKeyFile(await file.text())));
            this.notify.success(`Count done. ${results.length} result rows stored.`);
        } catch (err) {
            if (!(err instanceof HttpErrorResponse)) this.notify.error('The key file could not be read.');
        } finally {
            this.countingId.set(null);
        }
    }

    private replace(updated: AdminElectionDto): void {
        this.elections.set(this.elections().map(item => item.id === updated.id ? updated : item));
    }

    private applyStep(result: AdminElectionStepResult): void {
        if (result.election) {
            this.replace(result.election);
            return;
        }
        const pending = result.pending;
        this.approvals.update(all => ({ ...all, [pending.electionId]: [...(all[pending.electionId] ?? []), pending] }));
        this.notify.info('Saved. A second person must approve this before it happens.');
    }

    // Spec 023 (37.12f). Loaded on request, since the list needs step-up.
    loadApprovals(election: AdminElectionDto): void {
        this.electionsService.getApprovals(election.id).subscribe({
            next: values => {
                this.approvals.update(all => ({ ...all, [election.id]: values }));
                if (values.length === 0) this.notify.info('Nothing is waiting for approval.');
            },
            error: () => this.notify.error('Failed to load approvals.')
        });
    }

    approve(approval: ElectionApprovalDto): void {
        if (this.decidingApprovalId() !== null) return;
        this.decidingApprovalId.set(approval.id);
        this.electionsService.approve(approval.id).subscribe({
            next: () => {
                this.decidingApprovalId.set(null);
                this.dropApproval(approval);
                this.notify.success(`${ELECTION_APPROVAL_ACTION_LABELS[approval.action]}: approved and done.`);
                this.load();
            },
            // The interceptor shows the server's reason, such as asking the requester to approve their own step.
            error: () => this.decidingApprovalId.set(null)
        });
    }

    async reject(approval: ElectionApprovalDto): Promise<void> {
        if (this.decidingApprovalId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Reject request',
            message: `Reject "${ELECTION_APPROVAL_ACTION_LABELS[approval.action]}" asked for by ${approval.requestedBy}? It will not happen.`,
            confirmLabel: 'Reject',
            danger: true
        }));
        if (!confirmed) return;

        this.decidingApprovalId.set(approval.id);
        this.electionsService.reject(approval.id, null).subscribe({
            next: () => { this.decidingApprovalId.set(null); this.dropApproval(approval); this.notify.success('Request rejected.'); },
            error: () => this.decidingApprovalId.set(null)
        });
    }

    private dropApproval(approval: ElectionApprovalDto): void {
        this.approvals.update(all => ({ ...all, [approval.electionId]: (all[approval.electionId] ?? []).filter(item => item.id !== approval.id) }));
    }

    selectElectionForScrutiny(id: number): void {
        this.selectedElectionId.set(id);
        this.loadNominations();
    }

    loadNominations(): void {
        const id = this.selectedElectionId();
        if (id === null) return;
        this.nominationsLoading.set(true);
        this.electionsService.getNominations(id).subscribe({
            next: values => { this.nominations.set(values); this.nominationsLoading.set(false); },
            error: () => { this.nominationsLoading.set(false); this.notify.error('Failed to load nominations.'); }
        });
    }

    scrutinise(nomination: NominationViewDto, accepted: boolean): void {
        if (this.scrutinisingId() !== null) return;
        this.scrutinisingId.set(nomination.id);
        this.electionsService.scrutinise(nomination.id, { accepted }).subscribe({
            next: () => { this.scrutinisingId.set(null); this.loadNominations(); },
            error: () => { this.scrutinisingId.set(null); this.notify.error('Failed to record the scrutiny decision.'); }
        });
    }
}
