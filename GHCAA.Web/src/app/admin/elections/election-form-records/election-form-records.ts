import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ElectionsService } from '../../../core/services/elections.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingPanelComponent } from '../../../common/loading-panel/loading-panel';
import { API_ENDPOINTS } from '../../../core/constants/app.constants';
import { localInputToUtc, utcToLocal, utcToLocalInput } from '../../../core/utils/date.util';
import {
    AdminElectionDto, ElectionPermissionName, ElectionRecountDto, PublicResultCandidateDto, UpdateElectionFormDetailsRequest
} from '../../../core/models/election.models';

interface FormDetailsForm {
    finalListOn: string;
    campaignOpensOn: string;
    campaignClosesOn: string;
    silenceStartsAt: string;
    venue: string;
    noticeReference: string;
    recountWindowEndsOn: string;
}

interface RecountForm {
    electionSeatId: number | null;
    applicantName: string;
    applicantMemberId: number | null;
    grounds: string;
    explanation: string;
    receivedAt: string;
}

// 37.14m. The fields the printed election forms read that the setup screen does not ask for,
// and the recount requests (ER-11, ER-28). The server checks phase and permission; the buttons
// here only hide what the caller cannot do.
@Component({
    selector: 'app-election-form-records',
    standalone: true,
    imports: [CommonModule, FormsModule, LoadingPanelComponent],
    templateUrl: './election-form-records.html'
})
export class ElectionFormRecords implements OnInit {
    private readonly electionsService = inject(ElectionsService);
    private readonly notify = inject(NotificationService);

    @Input({ required: true }) election!: AdminElectionDto;
    changed = output<AdminElectionDto>();

    readonly text = API_ENDPOINTS.ADMIN_ELECTIONS;
    readonly utcToLocal = utcToLocal;

    details: FormDetailsForm = this.emptyDetails();
    savingDetails = signal(false);

    recounts = signal<ElectionRecountDto[]>([]);
    loadingRecounts = signal(false);
    recountForm: RecountForm = this.emptyRecount();
    savingRecount = signal(false);
    busyRecountId = signal<number | null>(null);
    refuseReasons: Record<number, string> = {};

    // Figures are typed against the declared result rows, so the nomination list always matches the seat.
    figuresOpenId = signal<number | null>(null);
    figureRows = signal<PublicResultCandidateDto[]>([]);
    figureVotes: Record<number, number | null> = {};

    ngOnInit(): void {
        this.details = this.toForm(this.election);
        if (this.can('ViewDashboard')) this.loadRecounts();
    }

    can(permission: ElectionPermissionName): boolean {
        return this.election.myPermissions?.includes(permission) ?? false;
    }

    saveDetails(): void {
        if (this.savingDetails()) return;
        const request: UpdateElectionFormDetailsRequest = {
            finalListOn: this.details.finalListOn || null,
            campaignOpensOn: this.details.campaignOpensOn || null,
            campaignClosesOn: this.details.campaignClosesOn || null,
            silenceStartsAt: localInputToUtc(this.details.silenceStartsAt),
            venue: this.details.venue.trim() || null,
            noticeReference: this.details.noticeReference.trim() || null,
            recountWindowEndsOn: localInputToUtc(this.details.recountWindowEndsOn)
        };
        this.savingDetails.set(true);
        this.electionsService.updateFormDetails(this.election.id, request).subscribe({
            next: updated => {
                this.savingDetails.set(false);
                this.notify.success(this.text.FORM_DETAILS_SAVED);
                this.changed.emit(updated);
            },
            error: () => this.savingDetails.set(false)
        });
    }

    loadRecounts(): void {
        this.loadingRecounts.set(true);
        this.electionsService.getRecounts(this.election.id).subscribe({
            next: values => { this.recounts.set(values); this.loadingRecounts.set(false); },
            error: () => this.loadingRecounts.set(false)
        });
    }

    canRecord(): boolean {
        const f = this.recountForm;
        return f.electionSeatId !== null && !!f.applicantName.trim() && !!f.grounds.trim() && !!f.receivedAt;
    }

    recordRecount(): void {
        if (!this.canRecord() || this.savingRecount()) return;
        const f = this.recountForm;
        this.savingRecount.set(true);
        this.electionsService.recordRecount(this.election.id, {
            electionSeatId: f.electionSeatId!,
            applicantName: f.applicantName.trim(),
            applicantMemberId: f.applicantMemberId || null,
            grounds: f.grounds.trim(),
            explanation: f.explanation.trim() || null,
            receivedAt: localInputToUtc(f.receivedAt)!
        }).subscribe({
            next: recount => {
                this.savingRecount.set(false);
                this.recounts.update(list => [...list, recount]);
                this.recountForm = this.emptyRecount();
                this.notify.success(this.text.RECOUNT_RECORDED);
            },
            error: () => this.savingRecount.set(false)
        });
    }

    decide(recount: ElectionRecountDto, allowed: boolean): void {
        if (this.busyRecountId() !== null) return;
        const reason = (this.refuseReasons[recount.id] ?? '').trim();
        if (!allowed && !reason) {
            this.notify.error(this.text.RECOUNT_REFUSE_REASON_REQUIRED);
            return;
        }
        this.busyRecountId.set(recount.id);
        this.electionsService.decideRecount(this.election.id, recount.id, { allowed, reason: reason || null }).subscribe({
            next: () => {
                this.busyRecountId.set(null);
                this.notify.success(this.text.RECOUNT_DECIDED);
                this.loadRecounts();
            },
            error: () => this.busyRecountId.set(null)
        });
    }

    canEnterFigures(recount: ElectionRecountDto): boolean {
        return this.election.phase === 'Declared' && recount.allowed === true && recount.figures.length === 0 && this.can('Count');
    }

    openFigures(recount: ElectionRecountDto): void {
        this.figuresOpenId.set(recount.id);
        this.figureRows.set([]);
        this.figureVotes = {};
        this.electionsService.getPublicResults(this.election.id).subscribe({
            next: results => {
                const rows = results.seats.find(seat => seat.seatId === recount.electionSeatId)?.candidates ?? [];
                if (rows.length === 0) {
                    this.figuresOpenId.set(null);
                    this.notify.error(this.text.RECOUNT_NO_RESULT);
                    return;
                }
                this.figureRows.set(rows);
                this.figureVotes = Object.fromEntries(rows.map(row => [row.nominationId, row.voteCount]));
            },
            error: () => this.figuresOpenId.set(null)
        });
    }

    figuresReady(): boolean {
        return this.figureRows().length > 0
            && this.figureRows().every(row => Number.isInteger(this.figureVotes[row.nominationId]) && this.figureVotes[row.nominationId]! >= 0);
    }

    saveFigures(recount: ElectionRecountDto): void {
        if (!this.figuresReady() || this.busyRecountId() !== null) return;
        const figures = this.figureRows().map(row => ({ nominationId: row.nominationId, voteCount: this.figureVotes[row.nominationId]! }));
        this.busyRecountId.set(recount.id);
        this.electionsService.recordRecountFigures(this.election.id, recount.id, figures).subscribe({
            next: () => {
                this.busyRecountId.set(null);
                this.figuresOpenId.set(null);
                this.notify.success(this.text.RECOUNT_FIGURES_SAVED);
                this.loadRecounts();
            },
            error: () => this.busyRecountId.set(null)
        });
    }

    private toForm(election: AdminElectionDto): FormDetailsForm {
        const d = election.formDetails;
        return {
            finalListOn: d?.finalListOn?.slice(0, 10) ?? '',
            campaignOpensOn: d?.campaignOpensOn?.slice(0, 10) ?? '',
            campaignClosesOn: d?.campaignClosesOn?.slice(0, 10) ?? '',
            silenceStartsAt: utcToLocalInput(d?.silenceStartsAt),
            venue: d?.venue ?? '',
            noticeReference: d?.noticeReference ?? '',
            recountWindowEndsOn: utcToLocalInput(d?.recountWindowEndsOn)
        };
    }

    private emptyDetails(): FormDetailsForm {
        return { finalListOn: '', campaignOpensOn: '', campaignClosesOn: '', silenceStartsAt: '', venue: '', noticeReference: '', recountWindowEndsOn: '' };
    }

    private emptyRecount(): RecountForm {
        return { electionSeatId: null, applicantName: '', applicantMemberId: null, grounds: '', explanation: '', receivedAt: '' };
    }
}
