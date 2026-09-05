import { Component, inject } from '@angular/core';
import { OrgConfigService } from '../../../../core/services/org-config.service';
import { ImgFallbackDirective } from '../../../../common/directives/img-fallback.directive';
import { SITE_CONTENT } from '../../../../core/config/site-content.generated';

@Component({
    selector: 'landing-purpose',
    standalone: true,
    imports: [ImgFallbackDirective],
    templateUrl: './purpose.html',
    styleUrl: './purpose.scss',
})
export class LandingPurpose {
    orgConfigService = inject(OrgConfigService);
    historyNote = SITE_CONTENT.purposeHistoryNote;
}

