import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../core/services/networking.service';

@Component({
    selector: 'app-governance',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './governance.html',
    styleUrl: './governance.scss'
})
export class Governance implements OnInit {
    private networkService = inject(NetworkingService);

    committee = signal<any[]>([]);
    loading = signal(true);

    ngOnInit() {
        this.networkService.getCommittee().subscribe({
            next: (data) => {
                this.committee.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getPositionName(pos: any): string {
        const roles = [
            'President',
            'General Secretary',
            'Vice President',
            'Treasurer',
            'Organizational Secretary',
            'Joint Secretary',
            'Information & Tech Secretary',
            'Media & Sports Secretary',
            'Law Secretary',
            'Executive Member'
        ];
        if (typeof pos === 'number') return roles[pos] || 'Executive Member';
        return pos;
    }
}
