import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ElectionsService } from '../../core/services/elections.service';
import { ElectionResultDto } from '../../core/models/election.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-election-results',
    standalone: true,
    imports: [CommonModule, LogoSpinnerComponent],
    templateUrl: './election-results.html'
})
export class ElectionResults {
    private readonly elections = inject(ElectionsService);
    results = signal<ElectionResultDto[] | null>(null);
    loading = signal(true);
    error = signal(false);
    forbidden = signal(false);

    constructor() {
        this.elections.getCurrent().subscribe({
            next: election => {
                if (!election) { this.loading.set(false); return; }
                // /count is admin-only — an anonymous or non-admin caller sees the 401/403 branch below,
                // not a results table, until an admin has actually run the count.
                this.elections.count(election.id).subscribe({
                    next: result => { this.results.set(result); this.loading.set(false); },
                    error: (err: HttpErrorResponse) => {
                        if (err.status === 401 || err.status === 403) this.forbidden.set(true);
                        else this.error.set(true);
                        this.loading.set(false);
                    }
                });
            },
            error: () => { this.error.set(true); this.loading.set(false); }
        });
    }
}
