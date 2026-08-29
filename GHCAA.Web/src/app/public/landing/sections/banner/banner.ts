import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LookupService } from '../../../../core/services/lookup.service';
import { CommonModule } from '@angular/common';
import { LogoSpinnerComponent } from '../../../../common/logo-spinner/logo-spinner';
import { OrgConfigService } from '../../../../core/services/org-config.service';

@Component({
    selector: 'landing-banner',
    standalone: true,
    imports: [RouterLink, CommonModule, LogoSpinnerComponent],
    templateUrl: './banner.html',
    styleUrl: './banner.scss',
})
export class LandingBanner implements OnInit {
    private lookupService = inject(LookupService);
    orgConfigService = inject(OrgConfigService);
    stats = signal<any>(null);

    ngOnInit() {
        this.lookupService.getStats(true).subscribe({
            next: (data) => this.stats.set(data),
            // On failure show nothing rather than invented numbers; the template hides
            // any stat that has no real value.
            error: () => this.stats.set({ totalActiveMembers: 0, publishedEvents: 0, currentECMembers: 0 })
        });
    }
}


