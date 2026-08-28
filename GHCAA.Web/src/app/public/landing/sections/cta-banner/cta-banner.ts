import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OrgConfigService } from '../../../../core/services/org-config.service';

@Component({
    selector: 'landing-cta-banner',
    standalone: true,
    imports: [RouterLink, CommonModule],
    templateUrl: './cta-banner.html',
    styleUrl: './cta-banner.scss',
})
export class LandingCtaBanner {
    orgConfigService = inject(OrgConfigService);
}


