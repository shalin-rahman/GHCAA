import { CommonModule } from '@angular/common';
import { Component, Input, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { SEARCH_DEBOUNCE_MS } from '../../../core/constants/app.constants';
import { ElectionPhase, ElectionSeatViewDto, NOMINATION_STATUS_LABELS, NominationViewDto } from '../../../core/models/election.models';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ElectionsService } from '../../../core/services/elections.service';
import { NetworkingService } from '../../../core/services/networking.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Debounced, debounce } from '../../../core/utils/debounce.util';

export interface MemberHit {
    id: number;
    fullName: string;
    membershipNumber: string | null;
}

type PickRole = 'candidate' | 'seconder';

// FR-37.1b. A member files a nomination as its proposer, and a candidate withdraws their own.
@Component({
    selector: 'app-election-nomination',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './election-nomination.html'
})
export class ElectionNomination implements OnInit, OnDestroy {
    private readonly elections = inject(ElectionsService);
    private readonly networking = inject(NetworkingService);
    private readonly notify = inject(NotificationService);
    private readonly auth = inject(AuthService);
    private readonly confirmDialog = inject(ConfirmDialogService);
    readonly statusLabels = NOMINATION_STATUS_LABELS;
    readonly statementMax = 4000;
    readonly roles: PickRole[] = ['candidate', 'seconder'];

    @Input({ required: true }) electionId!: number;
    @Input({ required: true }) phase!: ElectionPhase;

    seats = signal<ElectionSeatViewDto[]>([]);
    nominations = signal<NominationViewDto[]>([]);
    seatId = signal<number | null>(null);
    statement = signal('');
    picked = signal<Record<PickRole, MemberHit | null>>({ candidate: null, seconder: null });
    queries = signal<Record<PickRole, string>>({ candidate: '', seconder: '' });
    hits = signal<Record<PickRole, MemberHit[]>>({ candidate: [], seconder: [] });
    submitting = signal(false);
    withdrawingId = signal<number | null>(null);

    private readonly memberId = computed(() => this.auth.currentUser()?.memberId ?? null);
    // The public list carries no names, so each row is named by its seat.
    mine = computed(() => this.nominations().filter(n => n.candidateMemberId === this.memberId()));
    canSubmit = computed(() => !this.submitting() && this.seatId() !== null && !!this.picked().candidate
        && !!this.picked().seconder && this.statement().trim().length > 0);

    private readonly searches: Record<PickRole, Debounced<[string]>> = {
        candidate: debounce((query: string) => this.runSearch('candidate', query), SEARCH_DEBOUNCE_MS),
        seconder: debounce((query: string) => this.runSearch('seconder', query), SEARCH_DEBOUNCE_MS)
    };

    ngOnInit(): void {
        this.elections.getSeats(this.electionId).subscribe({ next: seats => this.seats.set(seats), error: () => this.seats.set([]) });
        this.loadNominations();
    }

    ngOnDestroy(): void {
        this.searches.candidate.cancel();
        this.searches.seconder.cancel();
    }

    seatTitle(seatId: number): string {
        return this.seats().find(s => s.seatId === seatId)?.positionTitle ?? `Seat ${seatId}`;
    }

    search(role: PickRole, query: string): void {
        this.queries.set({ ...this.queries(), [role]: query });
        this.picked.set({ ...this.picked(), [role]: null });
        if (query.trim().length < 2) {
            this.searches[role].cancel();
            this.hits.set({ ...this.hits(), [role]: [] });
            return;
        }
        this.searches[role](query.trim());
    }

    pick(role: PickRole, member: MemberHit): void {
        this.picked.set({ ...this.picked(), [role]: member });
        this.queries.set({ ...this.queries(), [role]: member.fullName });
        this.hits.set({ ...this.hits(), [role]: [] });
    }

    submit(): void {
        const seatId = this.seatId();
        const { candidate, seconder } = this.picked();
        const memberId = this.memberId();
        if (!this.canSubmit() || seatId === null || !candidate || !seconder || !memberId) return;
        this.submitting.set(true);
        // The server sets the proposer to the signed-in member whatever is sent.
        this.elections.nominate(this.electionId, {
            electionSeatId: seatId, candidateMemberId: candidate.id, proposerMemberId: memberId,
            seconderMemberId: seconder.id, statement: this.statement().trim()
        }).subscribe({
            next: () => {
                this.submitting.set(false);
                this.reset();
                this.loadNominations();
                this.notify.success('Nomination filed. The election commission will check it at scrutiny.');
            },
            // The HTTP interceptor shows the rule the nomination broke.
            error: () => this.submitting.set(false)
        });
    }

    async withdraw(nomination: NominationViewDto): Promise<void> {
        if (this.withdrawingId() !== null) return;
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Withdraw nomination',
            message: `Withdraw your nomination for ${this.seatTitle(nomination.electionSeatId)}? You cannot undo this.`,
            confirmLabel: 'Withdraw',
            danger: true
        }));
        if (!confirmed) return;
        this.withdrawingId.set(nomination.id);
        this.elections.withdrawNomination(nomination.id).subscribe({
            next: () => {
                this.withdrawingId.set(null);
                this.nominations.set(this.nominations().map(n => n.id === nomination.id ? { ...n, status: 'Withdrawn' } : n));
                this.notify.success('Your nomination was withdrawn.');
            },
            error: () => this.withdrawingId.set(null)
        });
    }

    private loadNominations(): void {
        this.elections.getNominations(this.electionId).subscribe({ next: list => this.nominations.set(list), error: () => this.nominations.set([]) });
    }

    private runSearch(role: PickRole, query: string): void {
        this.networking.searchMembers({ query, page: 1, pageSize: 10 }).subscribe({
            next: response => {
                // A slower reply for an older query must not replace the newer list.
                if (this.queries()[role].trim() !== query) return;
                this.hits.set({
                    ...this.hits(),
                    [role]: (response?.items ?? []).map(m => ({ id: m.id, fullName: m.fullName, membershipNumber: m.membershipNumber ?? null }))
                });
            },
            error: () => this.hits.set({ ...this.hits(), [role]: [] })
        });
    }

    private reset(): void {
        this.seatId.set(null);
        this.statement.set('');
        this.picked.set({ candidate: null, seconder: null });
        this.queries.set({ candidate: '', seconder: '' });
        this.hits.set({ candidate: [], seconder: [] });
    }
}
