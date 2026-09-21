import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ElectionsService } from '../../core/services/elections.service';
import { Election, ElectionBallot, ElectionCandidate } from '../../core/models/election.models';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-member-election',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent],
    templateUrl: './election.html'
})
export class MemberElection {
    private readonly elections = inject(ElectionsService);
    private readonly notify = inject(NotificationService);

    election = signal<Election | null>(null);
    loading = signal(true);
    submitting = signal(false);
    submitted = signal(false);
    error = signal(false);
    selected = signal<Record<number, number[]>>({});

    constructor() {
        this.elections.getCurrent().subscribe({
            next: value => { this.election.set(value); this.loading.set(false); },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }

    candidates(positionId: number): ElectionCandidate[] {
        return this.election()?.candidates.filter(candidate => candidate.positionId === positionId) ?? [];
    }

    isSelected(positionId: number, candidateId: number): boolean {
        return this.selected()[positionId]?.includes(candidateId) ?? false;
    }

    choose(positionId: number, candidateId: number, seats: number): void {
        const current = this.selected()[positionId] ?? [];
        const next = current.includes(candidateId)
            ? current.filter(id => id !== candidateId)
            : seats === 1 ? [candidateId] : current.length < seats ? [...current, candidateId] : current;
        this.selected.set({ ...this.selected(), [positionId]: next });
    }

    submit(): void {
        const election = this.election();
        if (!election || this.submitting()) return;
        if (election.positions.some(position => (this.selected()[position.id]?.length ?? 0) !== position.seats)) {
            this.notify.warning('Select one candidate for each available seat.');
            return;
        }
        const ballot: ElectionBallot = {
            electionId: election.id,
            selections: election.positions.map(position => ({
                positionId: position.id,
                candidateIds: this.selected()[position.id] ?? []
            }))
        };
        this.submitting.set(true);
        this.elections.submitBallot(election.id, ballot).subscribe({
            next: () => { this.submitting.set(false); this.submitted.set(true); this.notify.success('Your ballot was submitted.'); },
            error: () => { this.submitting.set(false); this.notify.error('The ballot could not be submitted.'); }
        });
    }
}
