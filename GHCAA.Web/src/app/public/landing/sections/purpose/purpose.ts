import { Component, inject } from '@angular/core';
import { OrgConfigService } from '../../../../core/services/org-config.service';
import { ImgFallbackDirective } from '../../../../common/directives/img-fallback.directive';

@Component({
    selector: 'landing-purpose',
    standalone: true,
    imports: [ImgFallbackDirective],
    templateUrl: './purpose.html',
    styleUrl: './purpose.scss',
})
export class LandingPurpose {
    orgConfigService = inject(OrgConfigService);
}

