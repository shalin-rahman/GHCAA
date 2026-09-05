import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrgConfigService } from '../../core/services/org-config.service';
import { SiteContentService } from '../../core/services/site-content.service';
import { SiteContent } from '../../core/models/business.models';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';
import { SITE_CONTENT } from '../../core/config/site-content.generated';
import { interpolateOrgTemplate } from '../../core/utils/org-template';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, ImgFallbackDirective],
  templateUrl: './about.html',
  styleUrl: './about.scss'
})
export class About implements OnInit {
  orgConfigService = inject(OrgConfigService);
  private siteContent = inject(SiteContentService);

  /** Empty until the CMS answers — the hardcoded story cards stay visible as the fallback. */
  blocks = signal<SiteContent[]>([]);

  /** Fallback founding-story card content, from the active institution profile pack. */
  foundingStory = SITE_CONTENT.aboutFoundingStory;
  foundingStoryHtml = computed(() =>
    interpolateOrgTemplate(this.foundingStory.paragraphHtml, this.orgConfigService.config())
  );

  ngOnInit() {
    this.siteContent.getByGroup('about').subscribe({
      next: list => this.blocks.set(list.filter(b => b.key !== 'contact-intro')),
      error: () => this.blocks.set([])
    });
  }
}

