import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { ConstitutionService, Constitution } from '../../core/services/constitution.service';
import { forkJoin, of, catchError } from 'rxjs';
import { getPositionLabel, getECRecordForPeriod } from '../../core/constants/app.constants';
import { EcPositionService, EcPosition } from '../../core/services/ec-position.service';
import { formatPeriodRange } from '../../core/utils/date.util';
import { LoadingPanelComponent } from '../loading-panel/loading-panel';
import { PageHeaderComponent } from '../page-header/page-header.component';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';
import { LazyImgDirective } from '../directives/lazy-img.directive';

@Component({
    // ... (rest of metadata)
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, LoadingPanelComponent, PageHeaderComponent, ImgFallbackDirective, LazyImgDirective],
    templateUrl: './governance.html',
    styleUrl: './governance.scss'
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);
    private constitutionService = inject(ConstitutionService);
    private positionService = inject(EcPositionService);

    committee = signal<MemberSummary[]>([]);
    // 95.2: the board is whichever positions an admin flags ShowOnBoard.
    positions = signal<EcPosition[]>([]);
    periods = signal<any[]>([]);
    selectedPeriodId = signal<number | null>(null);
    loading = signal(true);

    /* --- 36.9: amendment ratification ------------------------------------ */

    /** The active version members ratify. Null while loading, or when none is published. */
    constitution = signal<Constitution | null>(null);
    /** Optional remarks submitted alongside the vote; the endpoint stores them verbatim. */
    voteComments = signal('');
    /** The choice being submitted, so the pressed button can show its own busy state. */
    pendingChoice = signal<boolean | null>(null);
    voteOutcome = signal<'none' | 'recorded' | 'rejected'>('none');
    voteMessage = signal('');

    ngOnInit() {
        this.loadPeriods();
        this.loadCommittee();

        // 404 here is the documented "no active version" state, so it stays silent and the
        // ratification card simply does not render.
        this.constitutionService.getCurrent().subscribe({
            next: c => this.constitution.set(c),
            error: () => this.constitution.set(null)
        });
    }

    /**
     * Casts the member's ratification vote. One vote per member per version is enforced by a
     * unique index server-side, and non-voting membership tiers are rejected there too, so a
     * refusal is reported in place rather than pre-empted in the UI.
     */
    castVote(isFor: boolean) {
        const version = this.constitution();
        if (!version || this.pendingChoice() !== null || this.voteOutcome() === 'recorded') return;

        this.pendingChoice.set(isFor);
        const comments = this.voteComments().trim();

        this.constitutionService.vote(version.id, isFor, comments || undefined).subscribe({
            next: res => {
                this.pendingChoice.set(null);
                this.voteOutcome.set('recorded');
                this.voteMessage.set(res?.message || 'Your vote has been recorded.');
            },
            error: () => {
                this.pendingChoice.set(null);
                this.voteOutcome.set('rejected');
                this.voteMessage.set(
                    'Your vote could not be recorded. You may have already voted on this version, ' +
                    'or your membership tier does not carry voting rights.'
                );
            }
        });
    }

    loadPeriods() {
        // 29F.2: surface HTTP failures instead of failing silently
        this.networkService.getPeriods().subscribe({
            next: p => {
                this.periods.set(p);
                const active = p.find(x => x.isActive);
                if (active) this.selectedPeriodId.set(active.id);
            },
            error: err => console.error('Failed to load governance periods', err)
        });
    }

    periodLabel(p: { title: string; startDate: string | Date; endDate?: string | Date; isActive: boolean }): string {
        return `${p.title} (${formatPeriodRange(p)})`;
    }

    onPeriodChange(id: any) {
        const numId = Number(id);
        this.selectedPeriodId.set(numId);
        this.loadCommittee(numId);
    }

    loadCommittee(periodId?: number) {
        const targetPeriodId = periodId || this.selectedPeriodId();

        this.loading.set(true);
        // Without the positions list nobody is shown as board, but the committee still loads.
        forkJoin({
            positions: this.positionService.getActive().pipe(catchError(() => of([] as EcPosition[]))),
            data: this.networkService.getCommittee(targetPeriodId ? { periodId: targetPeriodId } : {})
        }).subscribe({
            next: ({ positions, data }) => {
                this.positions.set(positions);
                const held = data.filter(m => getECRecordForPeriod(m.ecHistory, targetPeriodId));
                const boardMembers = held.filter(m => this.isBoardRecord(getECRecordForPeriod(m.ecHistory, targetPeriodId)));
                const generalEC = held.filter(m => !this.isBoardRecord(getECRecordForPeriod(m.ecHistory, targetPeriodId)));
                this.committee.set([...boardMembers, ...generalEC] as MemberSummary[]);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getMajorDisplay(degree: string | undefined, subject: string | undefined): string {
        return subject && subject !== 'None' ? `in ${subject}` : '';
    }

    getPositionName(member: any): string {
        return getPositionLabel(getECRecordForPeriod(member.ecHistory, this.selectedPeriodId()));
    }

    isBoardMember(member: any): boolean {
        return this.isBoardRecord(getECRecordForPeriod(member.ecHistory, this.selectedPeriodId()));
    }

    // Matches on id, or on the key for a record from an older API that sends no id.
    private isBoardRecord(record: any): boolean {
        if (!record) return false;
        const position = this.positions().find(p => record.positionId ? p.id === record.positionId : p.key === record.position);
        return !!position?.showOnBoard;
    }
}


