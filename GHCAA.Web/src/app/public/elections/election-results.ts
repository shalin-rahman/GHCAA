import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ElectionsService } from '../../core/services/elections.service';
import { PublicElectionResultsDto } from '../../core/models/election.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { toDisplayDate } from '../../core/utils/date.util';

// Rendering thousands of codes at once makes the page slow. The filter box narrows the list.
const TRACKING_CODE_PAGE = 200;

@Component({
    selector: 'app-election-results',
    standalone: true,
    imports: [CommonModule, LogoSpinnerComponent, SearchBarComponent],
    templateUrl: './election-results.html'
})
export class ElectionResults {
    private readonly elections = inject(ElectionsService);
    readonly toDisplayDate = toDisplayDate;
    readonly codePage = TRACKING_CODE_PAGE;

    results = signal<PublicElectionResultsDto | null>(null);
    loading = signal(true);
    error = signal(false);
    notDeclared = signal(false);

    trackingCodes = signal<string[] | null>(null);
    codesNotPublished = signal(false);
    codesError = signal(false);
    codeFilter = signal('');

    matchingCodes = computed(() => {
        const query = this.codeFilter().trim().toLowerCase();
        const codes = this.trackingCodes() ?? [];
        return query ? codes.filter(code => code.toLowerCase().includes(query)) : codes;
    });
    shownCodes = computed(() => this.matchingCodes().slice(0, TRACKING_CODE_PAGE));

    constructor() {
        // /current has no election once one is declared, so the results come from /latest-declared.
        this.elections.getLatestDeclared().subscribe({
            next: election => {
                if (!election) { this.loading.set(false); return; }
                this.loadResults(election.id);
                this.loadTrackingCodes(election.id);
            },
            error: (err: HttpErrorResponse) => {
                if (err.status === 404) this.notDeclared.set(true);
                else this.error.set(true);
                this.loading.set(false);
            }
        });
    }

    private loadResults(id: number): void {
        this.elections.getPublicResults(id).subscribe({
            next: result => { this.results.set(result); this.loading.set(false); },
            error: (err: HttpErrorResponse) => {
                if (err.status === 404) this.notDeclared.set(true);
                else this.error.set(true);
                this.loading.set(false);
            }
        });
    }

    private loadTrackingCodes(id: number): void {
        this.elections.getTrackingCodes(id).subscribe({
            next: codes => this.trackingCodes.set(codes),
            error: (err: HttpErrorResponse) => {
                if (err.status === 404) this.codesNotPublished.set(true);
                else this.codesError.set(true);
            }
        });
    }
}
