import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrgConfigService } from '../../core/services/org-config.service';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, ImgFallbackDirective],
  templateUrl: './about.html',
  styleUrl: './about.scss'
})
export class About {
  orgConfigService = inject(OrgConfigService);
}

