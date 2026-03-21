import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LookupService } from '../../../../core/services/lookup.service';

@Component({
    selector: 'landing-cta-banner',
    standalone: true,
    imports: [RouterLink, CommonModule],
    templateUrl: './cta-banner.html',
    styleUrl: './cta-banner.scss',
})
export class LandingCtaBanner implements OnInit {
    private lookupService = inject(LookupService);
    stats = signal<any>(null);

    ngOnInit() {
        this.lookupService.getStats(true).subscribe({
            next: (data) => {
                this.stats.set({
                    totalMembers: data.totalActiveMembers || 1200,
                    eventsHosted: data.publishedEvents || 50,
                    estYear: 2025 // Association establishment year
                });
            },
            error: () => this.stats.set({ totalMembers: 1200, eventsHosted: 50, estYear: 2025 })
        });
    }
}


