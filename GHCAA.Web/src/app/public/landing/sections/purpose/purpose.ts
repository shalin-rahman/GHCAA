import { Component, inject } from '@angular/core';
import { OrgConfigService } from '../../../../core/services/org-config.service';

@Component({
    selector: 'landing-purpose',
    standalone: true,
    templateUrl: './purpose.html',
    styleUrl: './purpose.scss',
})
export class LandingPurpose {
    orgConfigService = inject(OrgConfigService);
}

