import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ElectionsService } from '../../core/services/elections.service';
import {
    AdminElectionDto, AdminElectionStepResult, CreateElectionRequest, ELECTION_APPROVAL_ACTION_LABELS, ELECTION_PHASE_ORDER,
    ELECTION_TOO_FEW_APPROVERS_WARNING, electionReadinessLabel, ElectionApprovalDto, ElectionPermissionName, ElectionPhase, ElectionReadinessDto, ElectionReadinessItemDto,
    ElectionTieRule, NominationViewDto
} from '../../core/models/election.models';
import { ExportUtil } from '../../core/utils/export.util';
import { ballotKeyFileName, generateBallotKeyPair, readBallotKeyFile, toPem } from '../../core/utils/ballot-key.util';
import { NotificationService } from '../../core/services/notification.service';
import { confirmRejectApproval } from '../../core/utils/election-approval.util';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { AuthService } from '../../core/services/auth.service';
import { AdminService } from '../../core/services/admin.service';
import { formatPeriodRange } from '../../core/utils/date.util';
import { ElectionOfficials } from './election-officials/election-officials';
import { ElectionRulesUnlock } from './election-rules-unlock/election-rules-unlock';
import { ElectionFormRecords } from './election-form-records/election-form-records';
import { NominationRecords } from './nomination-records/nomination-records';
import {
    getElectionPhaseLabel, getElectionPhaseClass,
    getNominationStatusLabel, getNominationStatusClass, SUPER_ADMIN_ROLE, ADMIN_ROLE, API_ENDPOINTS, ELECTION_APPOINTMENT_REASON_MAX
} from '../../core/constants/app.constants';

type AdminElectionsTab = 'elections' | 'nominations';

@Component({
    selector: 'app-admin-elections',
    standalone: true,
    imports: [CommonModule, FormsModule, LoadingPanelComponent, SearchBarComponent, PageHeaderComponent, ElectionOfficials, ElectionRulesUnlock, ElectionFormRecords, NominationRecords],
    templateUrl: './admin-elections.html'
})
export class AdminElections {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);
    private readonly auth = inject(AuthService);
    private readonly adminService = inject(AdminService);
    // Set on /officials/elections/:id, where the page shows that one election.
    private readonly routeElectionId = Number(inject(ActivatedRoute, { optional: true })?.snapshot.paramMap.get('id')) || null;

    readonly text = API_ENDPOINTS.ADMIN_ELECTIONS;
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
    advancingId = signal<number | null>(null);
    addingCandidateId = signal<number | null>(null);
    removingCandidateId = signal<number | null>(null);
    keyingId = signal<number | null>(null);
    countingId = signal<number | null>(null);
    freezingId = signal<number | null>(null);
    unfreezeOpenId = signal<number | null>(null);
    unfreezeReason = '';
    readonly reasonMax = ELECTION_APPOINTMENT_REASON_MAX;
    approvals = signal<Partial<Record<number, ElectionApprovalDto[]>>>({});
    readiness = signal<Partial<Record<number, ElectionReadinessDto>>>({});
    decidingApprovalId = signal<number | null>(null);
    approvalLabels = ELECTION_APPROVAL_ACTION_LABELS;
    officialsOpenId = signal<number | null>(null);
    formRecordsOpenId = signal<number | null>(null);
    destroyingKeyId = signal<number | null>(null);
    readonly isSuperAdmin = computed(() => this.auth.hasRole(SUPER_ADMIN_ROLE));
    // Creating an election and listing governance periods are admin-only on the server.
    readonly isAdmin = computed(() => this.auth.hasRole(SUPER_ADMIN_ROLE) || this.auth.hasRole(ADMIN_ROLE));

    filtered = computed(() => {
        const query = this.search().trim().toLowerCase();
        const all = this.routeElectionId === null ? this.elections() : this.elections().filter(election => election.id === this.routeElectionId);
        return query ? all.filter(election => election.title.toLowerCase().includes(query)) : all;
    });

    form: CreateElectionRequest = this.emptyForm();
    // Chosen in the form's period select. It stays null until the admin picks one.
    selectedPeriodId: number | null = null;
    periods = signal<{ id: number; label: string }[]>([]);

    // Nominations/scrutiny tab.
    selectedElectionId = signal<number | null>(null);
    nominations = signal<NominationViewDto[]>([]);
    nominationsLoading = signal(false);
    scrutinisingId = signal<number | null>(null);
    scrutinyReasons = signal<Partial<Record<number, string>>>({});

    constructor() {
        this.load();
        if (this.isAdmin()) this.loadPeriods();
    }

    private loadPeriods(): void {
        this.adminService.getGovernancePeriods().subscribe({
            next: periods => this.periods.set(periods.map(p => ({ id: p.id, label: `${p.title} (${formatPeriodRange(p)})` }))),
            error: () => this.periods.set([])
        });
    }

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
                this.loadReadiness(values);
                if (this.selectedElectionId() === null && values.length > 0) this.selectedElectionId.set(values[0].id);
            },
            error: () => { this.loading.set(false); this.notify.error('Failed to load elections.'); }
        });
    }

    tieRuleHelp(rule: ElectionTieRule | undefined): string {
        return this.text.TIE_RULES.find(option => option.value === rule)?.description ?? '';
    }

    private emptyForm(): CreateElectionRequest {
        return {
            title: '', ecPeriodId: 0, nominationOpensOn: '', nominationClosesOn: '',
            pollingOpensOn: '', pollingClosesOn: '',
            tieRule: 'DrawingLots', scrutinyOn: null, withdrawalClosesOn: null,
            description: '', positions: [{ title: '', seats: 1 }], isTest: false
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
        if (this.selectedPeriodId === null) {
            this.notify.warning('Choose the governance period this election is for.');
            return;
        }
        const { nominationOpensOn, nominationClosesOn, pollingOpensOn, pollingClosesOn } = this.form;
        if (![nominationOpensOn, nominationClosesOn, pollingOpensOn, pollingClosesOn].every(Boolean)) {
            this.notify.warning(this.text.DATES_MISSING);
            return;
        }
        this.saving.set(true);
        // An emptied date input gives '', which the server would reject as a date.
        const request: CreateElectionRequest = {
            ...this.form,
            ecPeriodId: this.selectedPeriodId,
            scrutinyOn: this.form.scrutinyOn || null,
            withdrawalClosesOn: this.form.withdrawalClosesOn || null
        };
        this.electionsService.create(request).subscribe({
            next: election => {
                this.elections.set([election, ...this.elections()]);
                this.showForm.set(false);
                this.saving.set(false);
                this.form = this.emptyForm();
                this.selectedPeriodId = null;
                this.notify.success('Election created.');
            },
            // The HTTP interceptor already shows the server's 400 detail as a toast.
            error: () => this.saving.set(false)
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

    // FR-37.1a. Publish and close have their own buttons, and counting ends with declare.
    nextPhase(election: AdminElectionDto): ElectionPhase | null {
        if (election.phase === 'Announced' || election.phase === 'Polling' || election.phase === 'Counting') return null;
        const index = ELECTION_PHASE_ORDER.indexOf(election.phase);
        return index >= 0 && index < ELECTION_PHASE_ORDER.length - 1 ? ELECTION_PHASE_ORDER[index + 1] : null;
    }

    async advance(election: AdminElectionDto): Promise<void> {
        const next = this.nextPhase(election);
        if (next === null || this.advancingId() !== null) return;
        const warning = next === 'Polling' && election.tooFewApprovers ? ELECTION_TOO_FEW_APPROVERS_WARNING : '';
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Advance phase',
            message: `Move "${election.title}" from ${getElectionPhaseLabel(election.phase)} to ${getElectionPhaseLabel(next)}? ${warning}`.trim(),
            confirmLabel: 'Advance',
            danger: warning !== ''
        }));
        if (!confirmed) return;

        this.advancingId.set(election.id);
        this.electionsService.setPhase(election.id, next).subscribe({
            next: pending => {
                this.advancingId.set(null);
                if (pending) {
                    this.addApproval(pending);
                    this.notify.info('Saved. A second person must approve this before it happens.');
                    return;
                }
                this.replace({ ...election, phase: next });
                this.notify.success(`Moved to ${getElectionPhaseLabel(next)}.`);
            },
            // The HTTP interceptor already shows the server's reason.
            error: () => this.advancingId.set(null)
        });
    }

    canEditRoll(election: AdminElectionDto): boolean {
        return (election.phase === 'Announced' || election.phase === 'Nomination') && this.can(election, 'ManageVoterRoll');
    }

    freezeRoll(election: AdminElectionDto): void {
        if (this.freezingId() !== null) return;
        this.freezingId.set(election.id);
        this.electionsService.freezeVoterRoll(election.id).subscribe({
            next: result => { this.freezingId.set(null); this.notify.success(`${result.count} voters frozen onto the roll.`); this.load(); },
            error: () => { this.freezingId.set(null); this.notify.error('Failed to freeze the voter roll.'); }
        });
    }

    openUnfreeze(election: AdminElectionDto): void {
        this.unfreezeReason = '';
        this.unfreezeOpenId.set(election.id);
    }

    unfreezeRoll(election: AdminElectionDto): void {
        const reason = this.unfreezeReason.trim();
        if (!reason) {
            this.notify.error('Give a reason for unfreezing the voter roll.');
            return;
        }
        if (this.freezingId() !== null) return;
        this.freezingId.set(election.id);
        this.electionsService.unfreezeVoterRoll(election.id, reason).subscribe({
            next: () => { this.freezingId.set(null); this.unfreezeOpenId.set(null); this.notify.success('Voter roll unfrozen.'); this.load(); },
            error: err => { this.freezingId.set(null); this.notify.error(err?.error?.detail ?? 'Failed to unfreeze the voter roll.'); }
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

    // The server sends MyPermissions on every reply. Without it, show nothing.
    can(election: AdminElectionDto, permission: ElectionPermissionName): boolean {
        return election.myPermissions?.includes(permission) ?? false;
    }

    readonly selectedElection = computed(() => this.elections().find(item => item.id === this.selectedElectionId()) ?? null);

    canOnSelected(permission: ElectionPermissionName): boolean {
        const election = this.selectedElection();
        return election ? this.can(election, permission) : false;
    }

    // FR-050. The box records that the key file was destroyed. It cannot be unticked once saved.
    keyDestroyed(election: AdminElectionDto, checked: boolean): void {
        if (!checked || this.destroyingKeyId() !== null) return;
        this.destroyingKeyId.set(election.id);
        this.electionsService.keyDestroyed(election.id).subscribe({
            next: () => { this.destroyingKeyId.set(null); this.notify.success('Recorded in the audit log.'); },
            error: (err: HttpErrorResponse) => {
                this.destroyingKeyId.set(null);
                if (err.status === 409) this.notify.info('The key was already recorded as destroyed.');
            }
        });
    }

    // Spec 023 FR-001. The key can be set until polling opens, and polling will not open without it.
    canSetBallotKey(election: AdminElectionDto): boolean {
        return ELECTION_PHASE_ORDER.indexOf(election.phase) < ELECTION_PHASE_ORDER.indexOf('Polling');
    }

    // 37.12w. Each row before polling shows the whole readiness list, the approver warning among it.
    // Officials record the attested items on their dashboard, so this list is read only.
    private loadReadiness(values: AdminElectionDto[]): void {
        this.readiness.set({});
        for (const election of values.filter(item => this.canSetBallotKey(item) && this.can(item, 'ViewDashboard'))) {
            this.electionsService.getReadiness(election.id).subscribe({
                next: result => this.readiness.update(all => ({ ...all, [election.id]: result })),
                error: () => undefined
            });
        }
    }

    readinessLabel(item: ElectionReadinessItemDto): string {
        return electionReadinessLabel(item);
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
            const run = await firstValueFrom(this.electionsService.runCount(election.id, readBallotKeyFile(await file.text())));
            if (run.results) {
                this.dropCountApprovals(election.id);
                this.notify.success(`Count done. ${run.results.length} result rows stored.`);
            } else {
                this.addApproval(run.pending);
                this.notify.info('Saved. A second person must approve the count. Then count again with the same key file.');
            }
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
        this.addApproval(result.pending);
        this.notify.info('Saved. A second person must approve this before it happens.');
    }

    toggleOfficials(election: AdminElectionDto): void {
        this.officialsOpenId.update(id => id === election.id ? null : election.id);
    }

    toggleFormRecords(election: AdminElectionDto): void {
        this.formRecordsOpenId.update(id => id === election.id ? null : election.id);
    }

    onFormRecordsChanged(updated: AdminElectionDto): void {
        this.replace(updated);
    }

    // The officials panel has already shown its own toast for an emergency revoke.
    onOfficialPending(pending: ElectionApprovalDto): void {
        this.addApproval(pending);
    }

    private addApproval(pending: ElectionApprovalDto): void {
        this.approvals.update(all => ({ ...all, [pending.electionId]: [...(all[pending.electionId] ?? []), pending] }));
    }

    private dropCountApprovals(electionId: number): void {
        this.approvals.update(all => ({ ...all, [electionId]: (all[electionId] ?? []).filter(item => item.action !== 'Count') }));
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
                // 37.13h. An approved count stays listed until the requester runs it with the key file.
                if (approval.action === 'Count') {
                    this.approvals.update(all => ({ ...all, [approval.electionId]: (all[approval.electionId] ?? [])
                        .map(item => item.id === approval.id ? { ...item, approvedAt: new Date().toISOString() } : item) }));
                    this.notify.success('Count approved. Anyone but you can now count with the key file.');
                    return;
                }
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
        if (!await confirmRejectApproval(this.confirmDialog, approval)) return;

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
        this.electionsService.getAdminNominations(id).subscribe({
            next: values => { this.nominations.set(values); this.nominationsLoading.set(false); },
            error: () => { this.nominationsLoading.set(false); this.notify.error('Failed to load nominations.'); }
        });
    }

    // The server only takes a decision while the election is in Scrutiny.
    canDecide(nomination: NominationViewDto): boolean {
        return (nomination.status === 'Submitted' || nomination.status === 'UnderScrutiny')
            && this.selectedElection()?.phase === 'Scrutiny' && this.canOnSelected('DecideNominations');
    }

    setScrutinyReason(nominationId: number, reason: string): void {
        this.scrutinyReasons.set({ ...this.scrutinyReasons(), [nominationId]: reason });
    }

    reasonFor(nomination: NominationViewDto): string {
        return (this.scrutinyReasons()[nomination.id] ?? '').trim();
    }

    // A rejection must say why, so Reject waits for a reason. Accept sends one only if given.
    scrutinise(nomination: NominationViewDto, accepted: boolean): void {
        const reason = this.reasonFor(nomination);
        if (this.scrutinisingId() !== null || (!accepted && !reason)) return;
        this.scrutinisingId.set(nomination.id);
        this.electionsService.scrutinise(nomination.id, reason ? { accepted, reason } : { accepted }).subscribe({
            next: () => { this.scrutinisingId.set(null); this.loadNominations(); },
            error: () => { this.scrutinisingId.set(null); this.notify.error('Failed to record the scrutiny decision.'); }
        });
    }
}
