/**
 * Architected & Developed by: md habibur rahman shalin (shalin.rahman@gmail.com)
 * Version: 2.0.5 - Professional Suite
 */
import { Component, inject } from '@angular/core';
import { RouterOutlet, Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { ToastComponent } from './common/toast/toast';
import { StepUpDialog } from './common/step-up-dialog/step-up-dialog';
import { ConfirmDialog } from './common/confirm-dialog/confirm-dialog';
import { OrgConfigService } from './core/services/org-config.service';
import { interpolateOrgTemplate } from './core/utils/org-template';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, StepUpDialog, ConfirmDialog],
  templateUrl: './app.html'
})
export class App {
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private meta = inject(Meta);
  private orgConfig = inject(OrgConfigService);

  private static readonly DEFAULT_DESCRIPTION_TEMPLATE =
    'Official portal of the {branding.memberNickname}s. Reconnecting {branding.institutionName} members worldwide through heritage, networking, and advancement.';

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      window.scrollTo({ top: 0, behavior: 'instant' });
      this.updateMetaDescription();
    });
  }

  // Angular Router's `title` route property sets <title> natively; there's no equivalent for
  // <meta name="description">, so each route that wants a unique one carries it in `data.description`
  // and this reads it off the deepest activated route on every navigation. Routes without one keep
  // the site-wide default from index.html rather than being left blank.
  private updateMetaDescription(): void {
    let route = this.activatedRoute;
    while (route.firstChild) route = route.firstChild;
    const template = route.snapshot.data['description'] ?? App.DEFAULT_DESCRIPTION_TEMPLATE;
    const description = interpolateOrgTemplate(template, this.orgConfig.config());
    this.meta.updateTag({ name: 'description', content: description });
  }
}
