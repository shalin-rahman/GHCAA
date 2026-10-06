import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ELECTION_RULES_UNLOCK } from '../../../core/constants/app.constants';
import { ElectionRulesUnlockDto } from '../../../core/models/election.models';

// 37.13z. SuperAdmin box for the short unlock of frozen election rules (FR-039). The page only
// renders it for a SuperAdmin; the API refuses everyone else anyway. It loads on request because
// every unlock call needs step-up, and the page should not ask for a code just to be opened.
@Component({
    selector: 'app-election-rules-unlock',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './election-rules-unlock.html'
})
export class ElectionRulesUnlock {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);

    readonly limits = ELECTION_RULES_UNLOCK;

    unlock = signal<ElectionRulesUnlockDto | null>(null);
    loaded = signal(false);
    busy = signal(false);
    reason = '';
    minutes: number = ELECTION_RULES_UNLOCK.DEFAULT_MINUTES;

    load(): void {
        this.electionsService.getRulesUnlock().subscribe({
            next: value => { this.unlock.set(value ?? null); this.loaded.set(true); },
            error: () => this.loaded.set(true)
        });
    }

    // Whole minutes left, rounded up, so a nearly-expired unlock reads 1 rather than 0.
    minutesLeft(unlock: ElectionRulesUnlockDto, now: number = Date.now()): number {
        return Math.max(0, Math.ceil((new Date(unlock.expiresAt).getTime() - now) / 60000));
    }

    // Same checks as the API, so the common mistakes are caught before the step-up dialog.
    validate(): string | null {
        const length = this.reason.trim().length;
        if (length < this.limits.REASON_MIN) return `Give a reason of at least ${this.limits.REASON_MIN} characters.`;
        if (length > this.limits.REASON_MAX) return `The reason can be at most ${this.limits.REASON_MAX} characters.`;
        if (!Number.isInteger(this.minutes) || this.minutes < 1 || this.minutes > this.limits.MAX_MINUTES)
            return `An unlock lasts 1 to ${this.limits.MAX_MINUTES} minutes.`;
        return null;
    }

    async open(): Promise<void> {
        if (this.busy()) return;
        const problem = this.validate();
        if (problem) {
            this.notify.warning(problem);
            return;
        }
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Unlock election rules',
            message: `Frozen election rules can be changed for ${this.minutes} minutes. Opening, closing and every change are logged.`,
            confirmLabel: 'Unlock',
            danger: true
        }));
        if (!confirmed) return;

        this.busy.set(true);
        this.electionsService.openRulesUnlock(this.reason.trim(), this.minutes).subscribe({
            next: value => {
                this.busy.set(false);
                this.unlock.set(value);
                this.reason = '';
                this.minutes = this.limits.DEFAULT_MINUTES;
                this.notify.success('Rules unlocked.');
            },
            error: () => this.busy.set(false)
        });
    }

    close(): void {
        if (this.busy()) return;
        this.busy.set(true);
        this.electionsService.closeRulesUnlock().subscribe({
            next: () => { this.busy.set(false); this.unlock.set(null); this.notify.success('Rules locked again.'); },
            // A 409 means it already expired or someone else closed it, so refresh either way.
            error: () => { this.busy.set(false); this.load(); }
        });
    }
}
