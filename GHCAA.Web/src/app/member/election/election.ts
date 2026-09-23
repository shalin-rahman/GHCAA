import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ElectionsService } from '../../core/services/elections.service';
import { CastVoteDto, ElectionSummaryDto, NominationViewDto } from '../../core/models/election.models';
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
    votedSeatIds = signal<number[]>([]);
    votingSeatId = signal<number | null>(null);

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

    hasVoted(seatId: number): boolean {
        return this.votedSeatIds().includes(seatId);
    }

    vote(seatId: number, nominationId: number): void {
        const election = this.election();
        if (!election || this.votingSeatId() !== null || this.hasVoted(seatId)) return;

        const dto: CastVoteDto = { electionSeatId: seatId, nominationId, serialNumber: undefined };
        this.votingSeatId.set(seatId);
        this.elections.castVote(election.id, dto).subscribe({
            next: () => {
                this.votingSeatId.set(null);
                this.votedSeatIds.set([...this.votedSeatIds(), seatId]);
                this.notify.success('Your vote was recorded.');
            },
            error: () => {
                this.votingSeatId.set(null);
                this.notify.error('The vote could not be recorded.');
            }
        });
    }
}
