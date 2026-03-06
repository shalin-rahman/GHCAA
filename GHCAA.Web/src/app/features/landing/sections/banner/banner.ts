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
        this.lookupService.getStats().subscribe({
            next: (data) => this.stats.set(data),
            error: () => this.stats.set({ totalMembers: '500+', batches: 68, countries: 10 })
        });
    }
}
