import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { ConstitutionService, Constitution } from '../../core/services/constitution.service';
import { getECPositionName, getECPositionForPeriod } from '../../core/constants/app.constants';
import { LogoSpinnerComponent } from '../logo-spinner/logo-spinner';
import { ImgFallbackDirective } from '../directives/img-fallback.directive';

@Component({
    // ... (rest of metadata)
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, LogoSpinnerComponent, ImgFallbackDirective],
    templateUrl: './governance.html',
    styleUrl: './governance.scss'
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);
    private constitutionService = inject(ConstitutionService);

    committee = signal<MemberSummary[]>([]);
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

    onPeriodChange(id: any) {
        const numId = Number(id);
        this.selectedPeriodId.set(numId);
        this.loadCommittee(numId);
    }

    loadCommittee(periodId?: number) {
        const boardPositions = [
            'President', 'VicePresident', 'GeneralSecretary', 'OfficeSecretary', 
            'JointSecretary1', 'JointSecretary2', 'Treasurer', 'MediaCulturalAndSportsSecretary',
            'OrganizationalSecretary', 'InformationAndTechnologySecretary', 'LawSecretary'
        ];

        const targetPeriodId = periodId || this.selectedPeriodId();

        this.loading.set(true);
        this.networkService.getCommittee(targetPeriodId ? { periodId: targetPeriodId } : {}).subscribe({
            next: (data) => {
                const boardMembers = data.filter(m => {
                    const pos = getECPositionForPeriod(m.ecHistory, targetPeriodId);
                    return boardPositions.includes(pos);
                });
                const generalEC = data.filter(m => {
                    const pos = getECPositionForPeriod(m.ecHistory, targetPeriodId);
                    return !boardPositions.includes(pos) && pos !== 'None' && pos !== 0;
                });
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
        const pos = getECPositionForPeriod(member.ecHistory, this.selectedPeriodId());
        return getECPositionName(pos);
    }

    isBoardMember(member: any): boolean {
        const boardPositions = [
            'President', 'VicePresident', 'GeneralSecretary', 'OfficeSecretary', 
            'JointSecretary1', 'JointSecretary2', 'Treasurer', 'MediaCulturalAndSportsSecretary',
            'OrganizationalSecretary', 'InformationAndTechnologySecretary', 'LawSecretary'
        ];
        const pos = getECPositionForPeriod(member.ecHistory, this.selectedPeriodId());
        return boardPositions.includes(pos);
    }
}



