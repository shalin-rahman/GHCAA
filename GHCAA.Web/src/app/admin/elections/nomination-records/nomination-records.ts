import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { API_ENDPOINTS } from '../../../core/constants/app.constants';
import { localInputToUtc, utcToLocal, utcToLocalInput } from '../../../core/utils/date.util';
import {
    AdminElectionDto, ElectionPermissionName, NominationViewDto, SCRUTINY_CHECK_ITEMS, SCRUTINY_CHECK_LABELS, ScrutinyCheckDto, ScrutinyCheckItem
} from '../../../core/models/election.models';

// 37.14m. The consent time (ER-10) and the scrutiny sheet (ER-17) for one nomination.
// The server refuses a tick outside scrutiny and a consent time once polling has opened.
@Component({
    selector: 'app-nomination-records',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './nomination-records.html'
})
export class NominationRecords implements OnInit {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);

    @Input({ required: true }) election!: AdminElectionDto;
    @Input({ required: true }) nomination!: NominationViewDto;

    readonly text = API_ENDPOINTS.ADMIN_ELECTIONS;
    readonly items = SCRUTINY_CHECK_ITEMS;
    readonly labels = SCRUTINY_CHECK_LABELS;

    consentAt = signal<string | null>(null);
    consentInput = '';
    savingConsent = signal(false);

    checks = signal<ScrutinyCheckDto[]>([]);
    busyItem = signal<ScrutinyCheckItem | null>(null);

    ngOnInit(): void {
        this.consentAt.set(this.nomination.consentAt ?? null);
        this.consentInput = utcToLocalInput(this.nomination.consentAt);
        if (this.can('DecideNominations')) {
            this.electionsService.getScrutinyChecks(this.election.id, this.nomination.id).subscribe({
                next: checks => this.checks.set(checks)
            });
        }
    }

    can(permission: ElectionPermissionName): boolean {
        return this.election.myPermissions?.includes(permission) ?? false;
    }

    // A withdrawn or rejected candidate is off the ballot, so their consent time no longer matters.
    canEditConsent(): boolean {
        return this.can('ManageSetup') && this.nomination.status !== 'Withdrawn' && this.nomination.status !== 'Rejected';
    }

    consentShown(): Date | null {
        return utcToLocal(this.consentAt());
    }

    saveConsent(): void {
        const consentAt = localInputToUtc(this.consentInput);
        if (!consentAt || this.savingConsent()) return;
        this.savingConsent.set(true);
        this.electionsService.recordConsent(this.election.id, this.nomination.id, consentAt).subscribe({
            next: () => {
                this.savingConsent.set(false);
                this.consentAt.set(consentAt);
                this.notify.success(this.text.CONSENT_SAVED);
            },
            error: () => this.savingConsent.set(false)
        });
    }

    isChecked(item: ScrutinyCheckItem): boolean {
        return this.checks().find(check => check.item === item)?.isChecked ?? false;
    }

    // Ticks can only change while the election is in scrutiny.
    canTick(): boolean {
        return this.election.phase === 'Scrutiny' && this.busyItem() === null;
    }

    setCheck(item: ScrutinyCheckItem, isChecked: boolean): void {
        if (!this.canTick()) return;
        this.busyItem.set(item);
        this.electionsService.setScrutinyCheck(this.election.id, this.nomination.id, item, isChecked).subscribe({
            next: checks => { this.checks.set(checks); this.busyItem.set(null); },
            error: () => this.busyItem.set(null)
        });
    }
}
