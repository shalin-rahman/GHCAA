import { Injectable, inject } from '@angular/core';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { OrgConfigService } from '../services/org-config.service';
import { interpolateOrgTemplate } from '../utils/org-template';

/**
 * Route `title` values are templates (e.g. `'Job Hub | {branding.shortName}'`) rather
 * than plain strings, so the tab title carries the active institution's name instead
 * of a hardcoded one. Angular's default TitleStrategy just calls Title.setTitle with
 * the raw string; this subclass interpolates it against OrgConfigService first.
 */
@Injectable({ providedIn: 'root' })
export class BrandingTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);
  private readonly orgConfig = inject(OrgConfigService);

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const template = this.buildTitle(snapshot);
    if (template === undefined) return;
    this.title.setTitle(interpolateOrgTemplate(template, this.orgConfig.config()));
  }
}
