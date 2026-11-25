import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AppDatePipe } from '../../../core/pipes/app-date.pipe';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { LoadingPanelComponent } from '../../../common/loading-panel/loading-panel';
import { API_ENDPOINTS } from '../../../core/constants/app.constants';
import { utcToLocal } from '../../../core/utils/date.util';
import { AdminElectionDto, ELECTION_PHASE_LABELS, ElectionFormPrintRowDto } from '../../../core/models/election.models';

// 37.16i. The filled forms printed for one election. An official with Count and ManageSetup can
// revoke a print; the QR check then reports it as revoked. The server checks permission and
// asks for step-up through the global interceptor.
@Component({
    selector: 'app-election-print-records',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, LoadingPanelComponent],
    templateUrl: './election-print-records.html'
})
export class ElectionPrintRecords implements OnInit {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);
    private readonly confirmDialog = inject(ConfirmDialogService);

    @Input({ required: true }) election!: AdminElectionDto;

    readonly text = API_ENDPOINTS.ADMIN_ELECTIONS;
    readonly phaseLabels = ELECTION_PHASE_LABELS;
    readonly utcToLocal = utcToLocal;

    rows = signal<ElectionFormPrintRowDto[]>([]);
    loading = signal(false);
    loadFailed = signal(false);
    revokeOpenId = signal<string | null>(null);
    revokingId = signal<string | null>(null);
    reason = '';
    readonly revokeRow = computed(() => {
        const row = this.rows().find(r => r.id === this.revokeOpenId());
        return row && this.canRevoke(row) ? row : null;
    });

    ngOnInit(): void {
        this.load();
    }

    load(): void {
        this.loading.set(true);
        this.loadFailed.set(false);
        this.electionsService.getPrintRecords(this.election.id).subscribe({
            next: rows => { this.rows.set(rows); this.loading.set(false); },
            error: () => { this.loading.set(false); this.loadFailed.set(true); }
        });
    }

    canRevoke(row: ElectionFormPrintRowDto): boolean {
        const permissions = this.election.myPermissions ?? [];
        return !row.revokedAt && permissions.includes('Count') && permissions.includes('ManageSetup');
    }

    // Same rule as the server: spaces do not count.
    reasonOk(): boolean {
        return this.reason.replace(/\s/g, '').length >= this.text.PRINT_REVOKE_REASON_MIN;
    }

    openRevoke(row: ElectionFormPrintRowDto): void {
        this.reason = '';
        this.revokeOpenId.update(id => id === row.id ? null : row.id);
    }

    async revoke(row: ElectionFormPrintRowDto): Promise<void> {
        if (!this.reasonOk() || this.revokingId() !== null) return;
        // Set before the dialog so a double click cannot open a second one.
        this.revokingId.set(row.id);
        const confirmed = await firstValueFrom(this.confirmDialog.confirm({
            title: this.text.PRINT_REVOKE_CONFIRM_TITLE,
            message: this.text.PRINT_REVOKE_CONFIRMATION(row.formCode),
            confirmLabel: this.text.PRINT_REVOKE_BUTTON,
            danger: true
        }));
        if (!confirmed) { this.revokingId.set(null); return; }
        const reason = this.reason.trim();
        this.electionsService.revokePrint(this.election.id, row.id, reason).subscribe({
            next: () => {
                this.revokingId.set(null);
                this.revokeOpenId.set(null);
                this.reason = '';
                this.rows.update(list => list.map(r => r.id === row.id ? { ...r, revokedAt: new Date().toISOString(), revokeReason: reason } : r));
                this.notify.success(this.text.PRINT_REVOKED);
            },
            // On a 409 someone else revoked it first, and the reload shows their reason.
            error: () => { this.revokingId.set(null); this.load(); }
        });
    }
}
