import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LookupService } from '../../../../core/services/lookup.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'landing-banner',
    standalone: true,
    imports: [RouterLink, CommonModule],
    templateUrl: './banner.html',
    styleUrl: './banner.scss',
})
export class LandingBanner implements OnInit {
    private lookupService = inject(LookupService);
    stats = signal<any>(null);

    ngOnInit() {
        this.lookupService.getStats(true).subscribe({
            next: (data) => this.stats.set(data),
            error: () => this.stats.set({ totalActiveMembers: 0, publishedEvents: 0, alumniChapters: 12 })
        });
    }
}


