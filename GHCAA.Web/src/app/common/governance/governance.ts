import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NetworkingService, MemberSummary } from '../../core/services/networking.service';
import { getECPositionName, getECPositionForPeriod } from '../../core/constants/app.constants';

@Component({
    // ... (rest of metadata)
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './governance.html',
    styleUrl: './governance.scss'
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);

    committee = signal<MemberSummary[]>([]);
    periods = signal<any[]>([]);
    selectedPeriodId = signal<number | null>(null);
    loading = signal(true);

    ngOnInit() {
        this.loadPeriods();
        this.loadCommittee();
    }

    loadPeriods() {
        this.networkService.getPeriods().subscribe(p => {
            this.periods.set(p);
            const active = p.find(x => x.isActive);
            if (active) this.selectedPeriodId.set(active.id);
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

    getMajorDisplay(degree: string | undefined, group: string | undefined, subject: string | undefined): string {
        if (!degree) return '';
        const major = degree === 'HSC' ? (group || 'None') : (subject || 'None');
        return major && major !== 'None' ? `in ${major}` : '';
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



