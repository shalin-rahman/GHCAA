import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../../../core/services/networking.service';
import { getECPositionName, getCurrentECPosition } from '../../../../core/constants/app.constants';

@Component({
    selector: 'landing-ec-preview',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './ec-preview.html',
    styleUrl: './ec-preview.scss',
})
export class LandingEcPreview implements OnInit {
    private networking = inject(NetworkingService);
    committee = signal<any[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        this.networking.getCommittee({}, true).subscribe({
            next: (data) => {
                // Filter out any members that have 'None' position or no position at all
                const validCommittee = (data || []).filter((m: any) => {
                    const p = getCurrentECPosition(m.ecHistory);
                    return p !== null && p !== undefined && p !== 0 && p !== '0' && p !== 'None';
                });
                this.committee.set(validCommittee);
            },
            error: () => {
                this.committee.set([]);
                this.isVisible.set(false);
            }
        });
    }

    getRoleName(member: any) {
        return getECPositionName(getCurrentECPosition(member.ecHistory));
    }
}


