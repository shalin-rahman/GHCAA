import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionPersonasService } from '../../core/services/election-personas.service';
import { NotificationService } from '../../core/services/notification.service';
import { confirmRejectApproval } from '../../core/utils/election-approval.util';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import {
    AdminElectionDto, ELECTION_APPROVAL_ACTION_LABELS, ELECTION_PHASE_ORDER, ELECTION_READINESS_LABELS, ElectionApprovalDto,
    ElectionAppointmentDto, ElectionAuditEntryDto, ElectionBoardDto, ElectionPermissionName, ElectionPersonaDto,
    ElectionReadinessDto, ElectionReadinessItemDto, NominationViewDto
} from '../../core/models/election.models';
import { getElectionPhaseClass, getElectionPhaseLabel } from '../../core/constants/app.constants';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

interface PersonaWarning {
    persona: string;
    text: string;
}

// Spec 023 (37.12h). One election at a time. A card shows only when the caller's permissions on it allow.
@Component({
    selector: 'app-officials-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink, LoadingPanelComponent, PageHeaderComponent],
    templateUrl: './officials-dashboard.html'
})
export class OfficialsDashboard {
    private readonly elections = inject(ElectionsService);
    private readonly personas = inject(ElectionPersonasService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);

    getElectionPhaseLabel = getElectionPhaseLabel;
    getElectionPhaseClass = getElectionPhaseClass;
    approvalLabels = ELECTION_APPROVAL_ACTION_LABELS;

    list = signal<AdminElectionDto[]>([]);
    loading = signal(true);
    error = signal(false);
    selectedId = signal<number | null>(null);
    selected = computed(() => this.list().find(e => e.id === this.selectedId()) ?? null);

    board = signal<ElectionBoardDto | null>(null);
    nominations = signal<NominationViewDto[]>([]);
    audit = signal<ElectionAuditEntryDto[]>([]);
    warnings = signal<PersonaWarning[]>([]);
    approvals = signal<ElectionApprovalDto[] | null>(null);
    decidingId = signal<number | null>(null);
    keyDestroyedSaved = signal(false);
    readiness = signal<ElectionReadinessDto | null>(null);
    attestingKey = signal<string | null>(null);

    waitingNominations = computed(() => this.nominations().filter(n => n.status === 'Submitted' || n.status === 'UnderScrutiny').length);
    // The board sends no turnout while polling is open, so the card follows what the server returns.
    turnout = computed(() => this.board()?.turnout ?? null);
    showKeyCard = computed(() => this.can('SetBallotKey') || this.can('ViewDashboard'));
    showDestroyKey = computed(() =>
        this.selected()?.phase === 'Declared' && this.can('SetBallotKey') && !this.keyDestroyedSaved());

    // 37.12w. The list matters only until polling opens.
    showReadiness = computed(() => {
        const phase = this.selected()?.phase;
        return !!phase && this.can('ViewDashboard') && ELECTION_PHASE_ORDER.indexOf(phase) < ELECTION_PHASE_ORDER.indexOf('Polling');
    });

    constructor() {
        this.load();
    }

    can(permission: ElectionPermissionName): boolean {
        return this.selected()?.myPermissions?.includes(permission) ?? false;
    }

    load(): void {
        this.loading.set(true);
        this.elections.getAdminElections().subscribe({
            next: values => {
                this.list.set(values);
                this.error.set(false);
                this.loading.set(false);
                if (values.length > 0) this.select(values[0].id);
            },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }

    select(id: number): void {
        this.selectedId.set(id);
        this.board.set(null);
        this.nominations.set([]);
        this.audit.set([]);
        this.warnings.set([]);
        this.approvals.set(null);
        this.keyDestroyedSaved.set(false);
        this.readiness.set(null);
        if (this.showReadiness()) this.loadReadiness();
        this.elections.getBoard(id).subscribe({ next: b => this.board.set(b), error: () => this.board.set(null) });
        if (this.can('DecideNominations')) {
            this.elections.getNominations(id).subscribe({ next: n => this.nominations.set(n), error: () => this.nominations.set([]) });
        }
        if (this.can('ViewAudit')) {
            this.elections.getAudit(id).subscribe({ next: a => this.audit.set(a.slice(-5).reverse()), error: () => this.audit.set([]) });
        }
        if (this.can('ViewDashboard')) {
            this.elections.getAppointments(id).subscribe({
                next: appointments => this.personas.listActive().subscribe({
                    next: personas => this.warnings.set(this.buildWarnings(appointments, personas)),
                    error: () => this.warnings.set([])
                }),
                error: () => this.warnings.set([])
            });
        }
    }

    private buildWarnings(appointments: ElectionAppointmentDto[], personas: ElectionPersonaDto[]): PersonaWarning[] {
        const result: PersonaWarning[] = [];
        for (const persona of personas) {
            const count = appointments.filter(a => a.personaId === persona.id && a.isLive).length;
            if (count < persona.minCount) {
                result.push({ persona: persona.name, text: `${count} of at least ${persona.minCount}` });
            } else if (persona.maxCount !== null && count > persona.maxCount) {
                result.push({ persona: persona.name, text: `${count}, more than ${persona.maxCount} allowed` });
            }
        }
        return result;
    }

    loadReadiness(): void {
        const id = this.selectedId();
        if (id === null) return;
        this.elections.getReadiness(id).subscribe({ next: r => this.readiness.set(r), error: () => this.readiness.set(null) });
    }

    readinessLabel(item: ElectionReadinessItemDto): string {
        return ELECTION_READINESS_LABELS[item.key] ?? item.key;
    }

    canAttest(item: ElectionReadinessItemDto): boolean {
        return item.attestation && !item.met && this.can('ChangePhase');
    }

    attest(item: ElectionReadinessItemDto): void {
        const id = this.selectedId();
        if (id === null || this.attestingKey() !== null) return;
        this.attestingKey.set(item.key);
        this.elections.attestReadiness(id, item.key).subscribe({
            next: () => {
                this.attestingKey.set(null);
                this.notify.success('Recorded in the audit log.');
                this.loadReadiness();
            },
            // 409 means someone else recorded it first, so the reload shows it ticked.
            error: (err: { status?: number }) => {
                this.attestingKey.set(null);
                if (err.status === 409) this.loadReadiness();
            }
        });
    }

    // Loaded on request, since the list needs a step-up.
    loadApprovals(): void {
        const id = this.selectedId();
        if (id === null) return;
        this.elections.getApprovals(id).subscribe({
            next: values => this.approvals.set(values),
            error: () => this.notify.error('The approvals could not be loaded.')
        });
    }

    canApprove(approval: ElectionApprovalDto): boolean {
        return this.can('Approve') && !approval.requestedByMe && !approval.approvedAt;
    }

    approve(approval: ElectionApprovalDto): void {
        if (this.decidingId() !== null) return;
        this.decidingId.set(approval.id);
        this.elections.approve(approval.id).subscribe({
            next: () => {
                this.decidingId.set(null);
                this.approvals.update(all => (all ?? []).filter(item => item.id !== approval.id));
                this.notify.success(`${ELECTION_APPROVAL_ACTION_LABELS[approval.action]}: approved.`);
            },
            // The global interceptor shows the server's reason.
            error: () => this.decidingId.set(null)
        });
    }

    async reject(approval: ElectionApprovalDto): Promise<void> {
        if (this.decidingId() !== null) return;
        if (!await confirmRejectApproval(this.confirmDialog, approval)) return;

        this.decidingId.set(approval.id);
        this.elections.reject(approval.id, null).subscribe({
            next: () => {
                this.decidingId.set(null);
                this.approvals.update(all => (all ?? []).filter(item => item.id !== approval.id));
                this.notify.success('Request rejected.');
            },
            error: () => this.decidingId.set(null)
        });
    }

    markKeyDestroyed(box: HTMLInputElement): void {
        const id = this.selectedId();
        if (!box.checked || id === null) return;
        this.elections.keyDestroyed(id).subscribe({
            next: () => { this.keyDestroyedSaved.set(true); this.notify.success('Recorded in the audit log.'); },
            // 409 means it was already recorded. Any other failure leaves the box ticked, so clear it.
            error: (err: { status?: number }) => {
                if (err.status === 409) this.keyDestroyedSaved.set(true);
                else box.checked = false;
            }
        });
    }
}
