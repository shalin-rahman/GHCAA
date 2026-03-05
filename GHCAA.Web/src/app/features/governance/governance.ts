import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../core/services/networking.service';
import { getECPositionName } from '../../core/constants/governance.constants';

@Component({
    // ... (rest of metadata)
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './governance.html',
    styleUrl: './governance.scss'
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);

    committee = signal<any[]>([]);
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

    onPeriodChange(id: number) {
        this.selectedPeriodId.set(id);
        this.loadCommittee(id);
    }

    loadCommittee(periodId?: number) {
        this.loading.set(true);
        this.networkService.getCommittee(periodId ? { periodId } : {}).subscribe({
            next: (data) => {
                const boardMembers = data.filter(m => m.ecPosition < 8);
                const generalEC = data.filter(m => m.ecPosition >= 8);
                this.committee.set([...boardMembers, ...generalEC]);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getPositionName(pos: any): string {
        return getECPositionName(pos);
    }
}
