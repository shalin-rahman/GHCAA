import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SiteContent } from '../../core/models/business.models';
import { OrgConfigService } from '../../core/services/org-config.service';
import { SiteContentService } from '../../core/services/site-content.service';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { POLICY_PAGES } from '../../core/constants/app.constants';
import { interpolateOrgTemplate } from '../../core/utils/org-template';
import { escapeHtml } from '../../core/utils/markdown.util';

// 100.6: /privacy, /terms and /faq. The text is site content an admin edits in Site Content
// (group "policy"); the route's data.key picks the row.
@Component({
    selector: 'app-policy-page',
    standalone: true,
    imports: [LoadingPanelComponent],
    templateUrl: './policy-page.html',
    styleUrl: './policy-page.scss'
})
export class PolicyPage implements OnInit {
    private route = inject(ActivatedRoute);
    private siteContent = inject(SiteContentService);
    orgConfigService = inject(OrgConfigService);

    loading = signal(true);
    block = signal<SiteContent | null>(null);
    fallbackTitle = signal('');

    // Placeholders such as {branding.fullName} take their value from OrgConfig. Values are escaped
    // because an office address may hold characters that would otherwise read as markup.
    bodyHtml = computed(() => {
        const body = this.block()?.bodyHtml ?? '';
        const cfg = this.orgConfigService.config();
        return body.replace(/\{[\w.]+\}/g, token => {
            const value = interpolateOrgTemplate(token, cfg);
            return value === token ? token : escapeHtml(value);
        });
    });

    ngOnInit() {
        const key = this.route.snapshot.data['key'] as string;
        this.fallbackTitle.set(this.route.snapshot.data['heading'] ?? '');
        this.siteContent.getByGroup(POLICY_PAGES.Group).subscribe({
            next: list => {
                this.block.set(list.find(b => b.key === key && b.bodyHtml?.trim()) ?? null);
                this.loading.set(false);
            },
            error: () => {
                this.block.set(null);
                this.loading.set(false);
            }
        });
    }
}
