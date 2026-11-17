import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PolicyPage } from './policy-page';
import { SiteContentService } from '../../core/services/site-content.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { POLICY_PAGES } from '../../core/constants/app.constants';

// FR-7 (spec 029, TODO 100.6): /privacy, /terms and /faq render the admin-editable policy content.
describe('PolicyPage', () => {
    let siteContent: { getByGroup: ReturnType<typeof vi.fn> };
    const config = {
        branding: { fullName: 'Test Alumni Association', shortName: 'TAA' },
        contact: { supportEmail: 'help@example.org', registeredOffice: 'Hall <A> & B' }
    };

    function create(key = POLICY_PAGES.Privacy) {
        TestBed.configureTestingModule({
            imports: [PolicyPage],
            providers: [
                { provide: SiteContentService, useValue: siteContent },
                { provide: OrgConfigService, useValue: { config: () => config } },
                { provide: ActivatedRoute, useValue: { snapshot: { data: { key, heading: 'Privacy Policy' } } } }
            ]
        });
        const fixture = TestBed.createComponent(PolicyPage);
        fixture.detectChanges();
        return fixture;
    }

    beforeEach(() => {
        siteContent = { getByGroup: vi.fn() };
    });

    it('shows the row for the route key and fills placeholders from OrgConfig, escaped', () => {
        siteContent.getByGroup.mockReturnValue(of([
            { id: 1, key: POLICY_PAGES.Terms, group: 'policy', title: 'Terms of Use', bodyHtml: '<p>Terms</p>', isActive: true },
            { id: 2, key: POLICY_PAGES.Privacy, group: 'policy', title: 'Privacy Policy',
              bodyHtml: '<p>{branding.fullName} at {contact.registeredOffice}, {unknown.field}</p>', isActive: true }
        ]));
        const fixture = create();
        const el: HTMLElement = fixture.nativeElement;

        expect(siteContent.getByGroup).toHaveBeenCalledWith(POLICY_PAGES.Group);
        expect(el.querySelector('h1')?.textContent).toContain('Privacy Policy');
        expect(fixture.componentInstance.bodyHtml())
            .toBe('<p>Test Alumni Association at Hall &lt;A&gt; &amp; B, {unknown.field}</p>');
        expect(el.querySelector('.doc-prose')?.textContent).toContain('Test Alumni Association at Hall <A> & B');
    });

    it('says the page is not published when the row is missing or empty', () => {
        siteContent.getByGroup.mockReturnValue(of([
            { id: 2, key: POLICY_PAGES.Privacy, group: 'policy', title: 'Privacy Policy', bodyHtml: '  ', isActive: true }
        ]));
        const el: HTMLElement = create().nativeElement;

        expect(el.textContent).toContain('Not published yet');
        expect(el.querySelector('h1')?.textContent).toContain('Privacy Policy');
    });

    it('falls back to the not-published state when the request fails', () => {
        siteContent.getByGroup.mockReturnValue(throwError(() => new Error('offline')));
        const fixture = create(POLICY_PAGES.Faq);

        expect(fixture.componentInstance.loading()).toBe(false);
        expect((fixture.nativeElement as HTMLElement).textContent).toContain('Not published yet');
    });
});
