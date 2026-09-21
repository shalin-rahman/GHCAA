import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionResult, ElectionSummary } from '../../core/models/election.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-election-results',
    standalone: true,
    imports: [CommonModule, LogoSpinnerComponent],
    templateUrl: './election-results.html'
})
export class ElectionResults {
    private readonly elections = inject(ElectionsService);
    summary = signal<ElectionSummary | null>(null);
    loading = signal(true);
    error = signal(false);

    resultsFor(positionId: number): ElectionResult[] {
        return this.summary()?.results.filter(result => result.positionId === positionId) ?? [];
    }

    constructor() {
        this.elections.getCurrent().subscribe({
            next: election => {
                if (!election) { this.loading.set(false); return; }
                this.elections.getResults(election.id).subscribe({
                    next: result => { this.summary.set(result); this.loading.set(false); },
                    error: () => { this.error.set(true); this.loading.set(false); }
                });
            },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }
}
