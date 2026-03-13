import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../../../core/services/networking.service';
import { getECPositionName } from '../../../../core/constants/app.constants';

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

    ngOnInit() {
        this.networking.getCommittee().subscribe({
            next: (data) => {
                // Filter out any members that have 'None' position (0) or no position at all
                const validCommittee = (data || []).filter((m: any) => {
                    const p = m.ecPosition ?? m.ECPosition ?? m.position ?? m.Position;
                    return p !== null && p !== undefined && p !== 0 && p !== '0' && p !== 'None';
                });
                this.committee.set(validCommittee);
            },
            error: () => this.committee.set([]) // Silently fail - no committee set up yet
        });
    }

    getRoleName(pos: number | string) {
        return getECPositionName(pos);
    }
}
