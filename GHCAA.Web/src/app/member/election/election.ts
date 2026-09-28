import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { ElectionsService } from '../../core/services/elections.service';
import { CastBallotDto, ElectionSummaryDto, NominationViewDto } from '../../core/models/election.models';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { getElectionPhaseLabel } from '../../core/constants/app.constants';

export interface SeatBallot {
    seatId: number;
    nominations: NominationViewDto[];
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
    getElectionPhaseLabel = getElectionPhaseLabel;

    election = signal<ElectionSummaryDto | null>(null);
    loading = signal(true);
    error = signal(false);
    // Seat id to the chosen nomination. A seat left out of the map is cast blank.
    choices = signal<Record<number, number>>({});
    submitting = signal(false);
    trackingCode = signal<string | null>(null);
    alreadyVoted = signal(false);

    private nominations = signal<NominationViewDto[]>([]);

    seatBallots = computed<SeatBallot[]>(() => {
        const bySeat = new Map<number, NominationViewDto[]>();
        for (const nomination of this.nominations()) {
            if (nomination.status !== 'Accepted') continue;
            const list = bySeat.get(nomination.electionSeatId) ?? [];
            list.push(nomination);
            bySeat.set(nomination.electionSeatId, list);
        }
        return Array.from(bySeat.entries())
            .sort(([a], [b]) => a - b)
            .map(([seatId, nominations]) => ({ seatId, nominations }));
    });

    constructor() {
        this.elections.getCurrent().subscribe({
            next: election => {
                this.election.set(election);
                if (!election) { this.loading.set(false); return; }
                this.elections.getNominations(election.id).subscribe({
                    next: nominations => { this.nominations.set(nominations); this.loading.set(false); },
                    error: () => { this.error.set(true); this.loading.set(false); }
                });
            },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }

    get closed(): boolean {
        return this.election()?.phase !== 'Polling' || this.trackingCode() !== null || this.alreadyVoted();
    }

    choose(seatId: number, nominationId: number): void {
        if (this.closed) return;
        const current = this.choices();
        const next = { ...current };
        if (current[seatId] === nominationId) delete next[seatId];
        else next[seatId] = nominationId;
        this.choices.set(next);
    }

    isChosen(seatId: number, nominationId: number): boolean {
        return this.choices()[seatId] === nominationId;
    }

    submit(): void {
        const election = this.election();
        if (!election || this.closed || this.submitting()) return;

        const choices = this.choices();
        const ballot: CastBallotDto = {
            seats: this.seatBallots().map(seat => ({
                electionSeatId: seat.seatId,
                nominationIds: choices[seat.seatId] !== undefined ? [choices[seat.seatId]] : []
            }))
        };
        this.submitting.set(true);
        this.elections.castBallot(election.id, ballot).subscribe({
            next: result => {
                this.submitting.set(false);
                this.trackingCode.set(result.trackingCode);
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
}
