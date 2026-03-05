import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../../../core/services/networking.service';

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
            next: (data) => this.committee.set(data),
            error: () => this.committee.set([]) // Silently fail - no committee set up yet
        });
    }
}
