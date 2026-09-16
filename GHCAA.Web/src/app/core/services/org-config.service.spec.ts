import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { OrgConfigService } from './org-config.service';
import { ThemeService } from './theme.service';
import { API_ENDPOINTS } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { ORG_CONFIG_FALLBACK } from '../config/org-config-fallback.generated';

// Highest fan-in Angular service in the app (18 consumers: layouts, guards, admin/public
// pages, app.config.ts) — no spec existed before this, despite being the config-driven
// framework backbone the rest of the app relies on.
describe('OrgConfigService', () => {
    let service: OrgConfigService;
    let httpMock: HttpTestingController;

    const minimalConfig = {
        orgId: 'ghcaa',
        schemaVersion: 1,
        branding: { primaryColor: '#111111', accentColor: '#222222' },
        features: { enableEvents: true, enableJobHub: false }
    } as unknown as OrgConfig;

    // Stubbed rather than the real ThemeService: the real one issues its own HTTP call
    // from an afterNextRender guard, which would need its own httpMock.expectOne() in every
    // test that ticks the ApplicationRef to flush OrgConfigService's branding effect.
    const themeServiceStub = { theme: () => 'dark' as 'light' | 'dark' };

    beforeEach(() => {
        themeServiceStub.theme = () => 'dark';
        TestBed.configureTestingModule({
            providers: [
                OrgConfigService,
                provideHttpClient(),
                provideHttpClientTesting(),
                { provide: ThemeService, useValue: themeServiceStub }
            ]
        });
        service = TestBed.inject(OrgConfigService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    it('starts with no config until loadConfig resolves', () => {
        expect(service.config()).toBeNull();
        expect(service.dateFormat()).toBe('dd-MM-yyyy');
    });

    it('normalizes unsupported and missing date formats to the safe default', async () => {
        const promise = service.loadConfig();
        httpMock.expectOne(API_ENDPOINTS.CONFIG).flush({
            ...minimalConfig,
            localization: { locales: {}, dateFormat: 'MM/dd/yyyy' }
        });
        await promise;
        expect(service.dateFormat()).toBe('MM/dd/yyyy');

        service.config.set({
            ...minimalConfig,
            localization: { locales: {}, dateFormat: 'yyyy-MM-dd' as never }
        });
        expect(service.dateFormat()).toBe('dd-MM-yyyy');
    });

    it('loadConfig fetches from the config endpoint and stores the result', async () => {
        const promise = service.loadConfig();

        const req = httpMock.expectOne(API_ENDPOINTS.CONFIG);
        expect(req.request.method).toBe('GET');
        req.flush(minimalConfig);

        await promise;
        expect(service.config()).toEqual(minimalConfig);
    });

    it('loadConfig carries the documents registry through untouched', async () => {
        const withDocuments = {
            ...minimalConfig,
            documents: [{ label: 'Bylaws', url: '/assets/bylaws.pdf', version: '1.0', group: 'Governance' }]
        } as unknown as OrgConfig;

        const promise = service.loadConfig();
        httpMock.expectOne(API_ENDPOINTS.CONFIG).flush(withDocuments);
        await promise;

        expect(service.config()?.documents).toEqual(withDocuments.documents);
    });

    it('loadConfig falls back to the built-in defaults when the request fails', async () => {
        // Asserted against ORG_CONFIG_FALLBACK itself, not a hardcoded 'ghcaa'/'GHCAA' literal —
        // that generated file's content depends on which profile ORG_PROFILE built with (62.52),
        // so the real contract under test is "falls back to the generated defaults", not "falls
        // back to GHC's specific defaults".
        const promise = service.loadConfig();

        httpMock.expectOne(API_ENDPOINTS.CONFIG).error(new ProgressEvent('network error'));

        await promise;
        const cfg = service.config();
        expect(cfg).not.toBeNull();
        expect(cfg!.orgId).toBe(ORG_CONFIG_FALLBACK.orgId);
        expect(cfg!.branding.shortName).toBe(ORG_CONFIG_FALLBACK.branding.shortName);
        expect(cfg!.features.enableEvents).toBe(ORG_CONFIG_FALLBACK.features.enableEvents);
    });

    it('loadConfig never rejects, even on failure, so app bootstrap is never blocked', async () => {
        const promise = service.loadConfig();
        httpMock.expectOne(API_ENDPOINTS.CONFIG).error(new ProgressEvent('network error'));

        await expect(promise).resolves.toBeUndefined();
    });

    describe('isFeatureEnabled', () => {
        it('returns false for every feature before config has loaded', () => {
            expect(service.isFeatureEnabled('enableEvents')).toBe(false);
        });

        it('reflects the loaded config once available', async () => {
            const promise = service.loadConfig();
            httpMock.expectOne(API_ENDPOINTS.CONFIG).flush(minimalConfig);
            await promise;

            expect(service.isFeatureEnabled('enableEvents')).toBe(true);
            expect(service.isFeatureEnabled('enableJobHub')).toBe(false);
        });
    });

    describe('localePack', () => {
        it('returns undefined when no locale data has been loaded', () => {
            expect(service.localePack()).toBeUndefined();
        });

        it('returns the requested locale entry once loaded', async () => {
            const withLocale = {
                ...minimalConfig,
                localization: { locales: { en: { tagline: 'Test tagline' } } }
            } as unknown as OrgConfig;

            const promise = service.loadConfig();
            httpMock.expectOne(API_ENDPOINTS.CONFIG).flush(withLocale);
            await promise;

            expect(service.localePack('en')?.tagline).toBe('Test tagline');
            expect(service.localePack('bn')).toBeUndefined();
        });
    });

    describe('updateConfig', () => {
        it('PUTs the new config and updates the local signal only after success', async () => {
            const updated = { ...minimalConfig, orgId: 'updated' } as unknown as OrgConfig;

            const promise = service.updateConfig(updated);

            const req = httpMock.expectOne(API_ENDPOINTS.CONFIG);
            expect(req.request.method).toBe('PUT');
            expect(req.request.body).toEqual(updated);
            req.flush(null, { status: 204, statusText: 'No Content' });

            await promise;
            expect(service.config()).toEqual(updated);
        });

        it('does not update the local signal when the PUT fails', async () => {
            const updated = { ...minimalConfig, orgId: 'updated' } as unknown as OrgConfig;
            const promise = service.updateConfig(updated).catch(() => {});

            httpMock.expectOne(API_ENDPOINTS.CONFIG).error(new ProgressEvent('network error'));
            await promise;

            expect(service.config()).not.toEqual(updated);
        });
    });

    describe('branding color-token effect', () => {
        const setConfigAndFlush = (branding: Partial<OrgConfig['branding']>) => {
            service.config.set({ ...minimalConfig, branding: { ...minimalConfig.branding, ...branding } } as OrgConfig);
            TestBed.inject(ApplicationRef).tick();
        };

        afterEach(() => {
            // Undo cross-test document.documentElement pollution — setProperty/removeProperty
            // calls in the effect persist on the real DOM element between tests otherwise.
            const root = document.documentElement.style;
            ['--primary-color', '--accent-color', '--accent-color-rgb', '--accent-rgb',
                '--accent-gold-bright', '--accent-gold-dark', '--accent-text', '--shadow-gold',
                '--gold-gradient', '--tier-founding', '--glass-border', '--select-chevron-gold']
                .forEach(prop => root.removeProperty(prop));
        });

        it('does nothing before config has loaded', () => {
            TestBed.inject(ApplicationRef).tick();
            expect(document.documentElement.style.getPropertyValue('--accent-color')).toBe('');
        });

        it('sets the plain accent/primary tokens and their rgb tuple', () => {
            setConfigAndFlush({ primaryColor: '#111111', accentColor: '#c5a059' });
            const root = document.documentElement.style;
            expect(root.getPropertyValue('--primary-color')).toBe('#111111');
            expect(root.getPropertyValue('--accent-color')).toBe('#c5a059');
            expect(root.getPropertyValue('--accent-color-rgb')).toBe('197, 160, 89');
            expect(root.getPropertyValue('--accent-rgb')).toBe('197, 160, 89');
        });

        it('darkens --accent-gold-dark until it clears 4.5:1 against white', () => {
            setConfigAndFlush({ accentColor: '#c5a059' });
            const goldDark = document.documentElement.style.getPropertyValue('--accent-gold-dark');
            expect(goldDark).not.toBe('#c5a059');
            expect(goldDark).toMatch(/^#[0-9a-f]{6}$/);
        });

        it('uses the raw accent (not the AA-clamped one) for --accent-text/--tier-founding in dark theme', () => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ accentColor: '#c5a059' });
            const root = document.documentElement.style;
            expect(root.getPropertyValue('--accent-text')).toBe('#c5a059');
            expect(root.getPropertyValue('--tier-founding')).toBe('#c5a059');
        });

        it('uses the AA-clamped values for --accent-text/--tier-founding in light theme', () => {
            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ accentColor: '#c5a059' });
            const root = document.documentElement.style;
            expect(root.getPropertyValue('--accent-text')).not.toBe('#c5a059');
            expect(root.getPropertyValue('--tier-founding')).not.toBe('#c5a059');
        });

        it('removes the --glass-border override in dark theme, sets a tint in light theme', () => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ accentColor: '#c5a059' });
            expect(document.documentElement.style.getPropertyValue('--glass-border')).toBe('');

            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ accentColor: '#c5a059' });
            expect(document.documentElement.style.getPropertyValue('--glass-border')).toBe('rgba(197, 160, 89, 0.12)');
        });

        it('bakes the accent hex into the --select-chevron-gold data-URI', () => {
            setConfigAndFlush({ accentColor: '#c5a059' });
            const chevron = document.documentElement.style.getPropertyValue('--select-chevron-gold');
            expect(chevron).toContain("stroke='%23c5a059'");
        });
    });
});
