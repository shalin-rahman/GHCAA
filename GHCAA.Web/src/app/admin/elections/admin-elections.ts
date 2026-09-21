import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ElectionsService } from '../../core/services/elections.service';
import { CreateElectionRequest, Election, ELECTION_PHASE_LABELS } from '../../core/models/election.models';
import { NotificationService } from '../../core/services/notification.service';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';

@Component({
    selector: 'app-admin-elections',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, SearchBarComponent],
    templateUrl: './admin-elections.html'
})
export class AdminElections {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);

    elections = signal<Election[]>([]);
    search = signal('');
    loading = signal(true);
    saving = signal(false);
    phaseLabel = ELECTION_PHASE_LABELS;
    showForm = signal(false);
    filtered = computed(() => {
        const query = this.search().trim().toLowerCase();
        return query ? this.elections().filter(election => election.title.toLowerCase().includes(query)) : this.elections();
    });
    form: CreateElectionRequest = {
        title: '', ecPeriodId: 0, nominationOpensOn: '', nominationClosesOn: '',
        pollingOpensOn: '', pollingClosesOn: '', createdBy: 0,
        description: '', positions: [{ title: '', seats: 1 }]
    };

    constructor() { this.load(); }

    load(): void {
        this.loading.set(true);
        this.electionsService.getAdminElections().subscribe({
            next: values => { this.elections.set(values); this.loading.set(false); },
            error: () => { this.loading.set(false); this.notify.error('Failed to load elections.'); }
        });
    }

    addPosition(): void {
        this.form.positions = [...(this.form.positions ?? []), { title: '', seats: 1 }];
    }
    removePosition(index: number): void {
        const positions = this.form.positions ?? [];
        if (positions.length > 1) this.form.positions = positions.filter((_, i) => i !== index);
    }

    create(): void {
        if (!this.form.title.trim() || (this.form.positions ?? []).some(position => !position.title.trim())) {
            this.notify.warning('Add a title and each election position.'); return;
        }
        this.saving.set(true);
        this.electionsService.create(this.form).subscribe({
            next: election => { this.elections.set([election, ...this.elections()]); this.showForm.set(false); this.saving.set(false); this.reset(); this.notify.success('Election created.'); },
            error: () => { this.saving.set(false); this.notify.error('Failed to create election.'); }
        });
    }

    publish(election: Election): void {
        this.electionsService.publish(election.id).subscribe({
            next: updated => this.replace(updated),
            error: () => this.notify.error('Failed to publish election.')
        });
    }

    close(election: Election): void {
        this.electionsService.close(election.id).subscribe({
            next: updated => this.replace(updated),
            error: () => this.notify.error('Failed to close election.')
        });
    }

    private replace(updated: Election): void { this.elections.set(this.elections().map(item => item.id === updated.id ? updated : item)); }
    private reset(): void {
        this.form = {
            title: '', ecPeriodId: 0, nominationOpensOn: '', nominationClosesOn: '',
            pollingOpensOn: '', pollingClosesOn: '', createdBy: 0,
            description: '', positions: [{ title: '', seats: 1 }]
        };
    }
}
