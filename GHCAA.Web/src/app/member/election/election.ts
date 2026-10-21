import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { ElectionsService } from '../../core/services/elections.service';
import { BallotSeatDto, BallotViewDto, CastBallotDto, ElectionSummaryDto } from '../../core/models/election.models';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { ExportUtil } from '../../core/utils/export.util';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { API_ENDPOINTS, BALLOT_RECEIPT_KEY_PREFIX, getElectionPhaseLabel } from '../../core/constants/app.constants';
import { toDisplayDate } from '../../core/utils/date.util';

type VoteStep = 'pick' | 'review';

// Keyed by member too, so a second member on the same browser never sees it.
const receiptKey = (memberId: number, electionId: number) => `${BALLOT_RECEIPT_KEY_PREFIX}${memberId}_${electionId}`;

export interface ReceiptLine {
    position: string;
    names: string[];
}

export interface VoteReceipt {
    title: string;
    date: string;
    trackingCode: string;
    lines: ReceiptLine[];
}

@Component({
    selector: 'app-member-election',
    standalone: true,
    imports: [CommonModule, LoadingPanelComponent, PageHeaderComponent],
    templateUrl: './election.html'
})
export class MemberElection {
    private readonly elections = inject(ElectionsService);
    private readonly notify = inject(NotificationService);
    private readonly auth = inject(AuthService);
    getElectionPhaseLabel = getElectionPhaseLabel;
    pickUpTo = API_ENDPOINTS.ELECTIONS.PICK_UP_TO;

    election = signal<ElectionSummaryDto | null>(null);
    ballot = signal<BallotViewDto | null>(null);
    loading = signal(true);
    error = signal(false);
    step = signal<VoteStep>('pick');
    // Seat id to the picked nomination ids. An empty list is an abstention. A seat with no entry is undecided.
    choices = signal<Record<number, number[]>>({});
    submitting = signal(false);
    trackingCode = signal<string | null>(null);
    alreadyVoted = signal(false);
    // Built from the choices in memory when the vote succeeds. It is never stored or sent anywhere.
    receipt = signal<VoteReceipt | null>(null);
    // The server PDF (base64). It holds no choices, so it is kept on this device until the election is archived.
    receiptPdf = signal<string | null>(null);

    seats = computed<BallotSeatDto[]>(() => this.ballot()?.seats ?? []);
    allDecided = computed(() => this.seats().length > 0 && this.seats().every(seat => this.choices()[seat.seatId] !== undefined));
    reviewLines = computed<ReceiptLine[]>(() => this.linesFor(this.seats(), this.choices()));
    notEligible = computed(() => this.ballot()?.isEligible === false);
    onTheRoll = computed(() => this.ballot()?.isEligible === true);
    // Mobile shows the same words under its abstain option.
    readonly abstainHelp = 'Your ballot still counts, but no candidate on this seat gets your vote.';

    constructor() {
        this.elections.getCurrent().subscribe({
            next: election => {
                this.election.set(election);
                if (!election) { this.loading.set(false); return; }
                this.elections.getBallot(election.id).subscribe({
                    next: ballot => {
                        this.ballot.set(ballot);
                        this.alreadyVoted.set(ballot.hasVoted);
                        this.restoreReceiptPdf(election.id, ballot.phase);
                        this.loading.set(false);
                    },
                    error: () => { this.error.set(true); this.loading.set(false); }
                });
            },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }

    get closed(): boolean {
        return this.ballot()?.phase !== 'Polling' || this.trackingCode() !== null || this.alreadyVoted() || this.notEligible();
    }

    get fingerprint(): string | null {
        return this.ballot()?.ballotKeyFingerprint ?? this.election()?.ballotKeyFingerprint ?? null;
    }

    toggle(seat: BallotSeatDto, nominationId: number): void {
        if (this.closed) return;
        const picked = this.choices()[seat.seatId] ?? [];
        let next: number[];
        if (picked.includes(nominationId)) next = picked.filter(id => id !== nominationId);
        else if (seat.places === 1) next = [nominationId];
        else if (picked.length < seat.places) next = [...picked, nominationId];
        else return;
        const choices = { ...this.choices() };
        if (next.length === 0) delete choices[seat.seatId];
        else choices[seat.seatId] = next;
        this.choices.set(choices);
    }

    abstain(seat: BallotSeatDto): void {
        if (this.closed) return;
        this.choices.set({ ...this.choices(), [seat.seatId]: [] });
    }

    isPicked(seatId: number, nominationId: number): boolean {
        return (this.choices()[seatId] ?? []).includes(nominationId);
    }

    isAbstained(seatId: number): boolean {
        const picked = this.choices()[seatId];
        return picked !== undefined && picked.length === 0;
    }

    atLimit(seat: BallotSeatDto, nominationId: number): boolean {
        const picked = this.choices()[seat.seatId] ?? [];
        return seat.places > 1 && picked.length >= seat.places && !picked.includes(nominationId);
    }

    review(): void {
        if (this.closed || !this.allDecided()) return;
        this.step.set('review');
    }

    backToPaper(): void {
        if (!this.submitting()) this.step.set('pick');
    }

    confirm(): void {
        const ballot = this.ballot();
        if (!ballot || this.closed || this.submitting() || !this.allDecided()) return;

        const choices = this.choices();
        const request: CastBallotDto = {
            seats: this.seats().map(seat => ({ electionSeatId: seat.seatId, nominationIds: choices[seat.seatId] }))
        };
        this.submitting.set(true);
        this.elections.castBallot(ballot.electionId, request).subscribe({
            next: result => {
                this.submitting.set(false);
                this.trackingCode.set(result.trackingCode);
                this.receipt.set({
                    title: ballot.title,
                    date: toDisplayDate(new Date()),
                    trackingCode: result.trackingCode,
                    lines: this.linesFor(this.seats(), choices)
                });
                this.choices.set({});
                if (result.receiptPdfBase64) {
                    this.receiptPdf.set(result.receiptPdfBase64);
                    this.storeReceiptPdf(ballot.electionId, result.receiptPdfBase64);
                }
                this.notify.success('Your ballot was recorded.');
            },
            error: (err: HttpErrorResponse) => {
                this.submitting.set(false);
                if (err.status === 409) {
                    this.alreadyVoted.set(true);
                    this.notify.error('You have already voted in this election.');
                } else {
                    this.notify.error('The ballot could not be recorded.');
                }
            }
        });
    }

    printReceipt(): void {
        window.print();
    }

    downloadReceipt(): void {
        const pdf = this.receiptPdf();
        const electionId = this.ballot()?.electionId;
        if (!pdf || electionId === undefined) return;
        let bytes: Uint8Array<ArrayBuffer>;
        try {
            bytes = Uint8Array.from(atob(pdf), c => c.charCodeAt(0));
        } catch {
            // A damaged stored copy. Drop it so the button goes away.
            const memberId = this.auth.currentUser()?.memberId;
            if (memberId) try { localStorage.removeItem(receiptKey(memberId, electionId)); } catch { /* storage blocked */ }
            this.receiptPdf.set(null);
            this.notify.error('The saved receipt could not be read.');
            return;
        }
        ExportUtil.saveFile(new Blob([bytes], { type: 'application/pdf' }), `ballot-receipt-${electionId}.pdf`);
    }

    private storeReceiptPdf(electionId: number, pdf: string): void {
        const memberId = this.auth.currentUser()?.memberId;
        if (!memberId) return;
        try { localStorage.setItem(receiptKey(memberId, electionId), pdf); } catch { /* storage full or blocked */ }
    }

    // Drops receipts for other elections and for this one once it is archived.
    private restoreReceiptPdf(electionId: number, phase: string): void {
        const memberId = this.auth.currentUser()?.memberId;
        if (!memberId) return;
        const keep = phase === 'Archived' ? null : receiptKey(memberId, electionId);
        try {
            for (let i = localStorage.length - 1; i >= 0; i--) {
                const key = localStorage.key(i);
                if (key?.startsWith(`${BALLOT_RECEIPT_KEY_PREFIX}${memberId}_`) && key !== keep) localStorage.removeItem(key);
            }
            if (keep) this.receiptPdf.set(localStorage.getItem(keep));
        } catch { /* storage blocked */ }
    }

    private linesFor(seats: BallotSeatDto[], choices: Record<number, number[]>): ReceiptLine[] {
        return seats.map(seat => ({
            position: seat.positionTitle,
            names: (choices[seat.seatId] ?? [])
                .map(id => seat.candidates.find(candidate => candidate.nominationId === id)?.name)
                .filter((name): name is string => !!name)
        }));
    }
}
