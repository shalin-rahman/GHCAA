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
import {
    contrastRatio, DARK_TEXT_SURFACE, hexToRgb, LIGHT_TEXT_SURFACE, MIN_TEXT_CONTRAST, rgbString
} from '../utils/color-math';

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
            // The effect writes to the real <html> and <body>, so values would leak into the next test.
            const root = document.documentElement.style;
            ['--accent-color', '--accent-color-rgb', '--accent-rgb',
                '--accent-gold-bright', '--accent-gold-dark', '--shadow-gold',
                '--gold-gradient', '--tier-founding', '--glass-border', '--select-chevron-gold']
                .forEach(prop => root.removeProperty(prop));
            const body = document.body.style;
            ['--primary-color', '--primary-rgb', '--text-on-primary',
                '--accent-text', '--accent-hover-text', '--focus-ring-color', '--text-on-accent', '--shadow-accent-hover',
                '--accent-on-dark-text', '--accent-on-dark-hover-text']
                .forEach(prop => body.removeProperty(prop));
        });

        it('does nothing before config has loaded', () => {
            TestBed.inject(ApplicationRef).tick();
            expect(document.documentElement.style.getPropertyValue('--accent-color')).toBe('');
        });

        it('sets the plain accent/primary tokens and their rgb tuple', () => {
            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ primaryColor: '#111111', accentColor: '#c5a059' });
            const root = document.documentElement.style;
            expect(document.body.style.getPropertyValue('--primary-color')).toBe('#111111');
            expect(document.body.style.getPropertyValue('--primary-rgb')).toBe('17, 17, 17');
            expect(root.getPropertyValue('--accent-color')).toBe('#c5a059');
            expect(root.getPropertyValue('--accent-color-rgb')).toBe('197, 160, 89');
            expect(root.getPropertyValue('--accent-rgb')).toBe('197, 160, 89');
        });

        it.each([
            ['light', '#121212', '#121212'],
            ['dark', '#121212', '#1a1a1a']
        ] as const)('keeps the stock primary in %s theme (%s shows as %s)', (theme, primary, shown) => {
            themeServiceStub.theme = () => theme;
            setConfigAndFlush({ primaryColor: primary });
            expect(document.body.style.getPropertyValue('--primary-color')).toBe(shown);
            expect(document.body.style.getPropertyValue('--text-on-primary')).toBe('#ffffff');
        });

        it.each(['light', 'dark'] as const)('a custom primary reaches body in %s theme with a readable label', theme => {
            themeServiceStub.theme = () => theme;
            setConfigAndFlush({ primaryColor: '#1a237e' });
            const body = document.body.style;
            const primary = body.getPropertyValue('--primary-color');
            expect(primary).not.toBe(theme === 'dark' ? '#1a1a1a' : '#121212');
            expect(body.getPropertyValue('--primary-rgb')).toBe(rgbString(hexToRgb(primary)));
            expect(contrastRatio(body.getPropertyValue('--text-on-primary'), primary)).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
        });

        it('a light custom primary gets the dark label', () => {
            setConfigAndFlush({ primaryColor: '#f4d36f' });
            expect(document.body.style.getPropertyValue('--text-on-primary')).toBe('#171717');
        });

        it('darkens --accent-gold-dark until it clears 4.5:1 against white', () => {
            setConfigAndFlush({ accentColor: '#c5a059' });
            const goldDark = document.documentElement.style.getPropertyValue('--accent-gold-dark');
            expect(goldDark).not.toBe('#c5a059');
            expect(goldDark).toMatch(/^#[0-9a-f]{6}$/);
        });

        it('keeps the raw gold for --accent-text/--tier-founding in dark theme, since it already reads on the dark card', () => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ accentColor: '#c5a059' });
            expect(document.body.style.getPropertyValue('--accent-text')).toBe('#c5a059');
            expect(document.documentElement.style.getPropertyValue('--tier-founding')).toBe('#c5a059');
        });

        it('uses the AA-clamped values for --accent-text/--tier-founding in light theme', () => {
            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ accentColor: '#c5a059' });
            const accentText = document.body.style.getPropertyValue('--accent-text');
            expect(accentText).not.toBe('#c5a059');
            expect(contrastRatio(accentText, LIGHT_TEXT_SURFACE)).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
            expect(document.documentElement.style.getPropertyValue('--tier-founding')).not.toBe('#c5a059');
        });

        it('lightens a dark accent for --accent-text in dark theme so it still reaches 4.5:1', () => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ accentColor: '#2f6f4f' });
            const accentText = document.body.style.getPropertyValue('--accent-text');
            expect(accentText).not.toBe('#2f6f4f');
            expect(contrastRatio(accentText, DARK_TEXT_SURFACE)).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
        });

        it.each([
            ['#c5a059', '#171717'],
            ['#2f6f4f', '#ffffff']
        ])('sets --text-on-accent on body for accent %s to %s', (accent, label) => {
            setConfigAndFlush({ accentColor: accent });
            expect(document.body.style.getPropertyValue('--text-on-accent')).toBe(label);
        });

        it.each(['light', 'dark'] as const)('the --text-on-accent label reads on every --gold-gradient stop in %s theme', theme => {
            themeServiceStub.theme = () => theme;
            setConfigAndFlush({ accentColor: '#2e7d32' });
            const label = document.body.style.getPropertyValue('--text-on-accent');
            const stops = document.documentElement.style.getPropertyValue('--gold-gradient').match(/#[0-9a-f]{6}/g)!;
            expect(stops).toHaveLength(3);
            const worstForLabel = Math.min(...stops.map(s => contrastRatio(label, s)));
            expect(worstForLabel).toBeGreaterThanOrEqual(4.5);
        });

        it('keeps the stock gold on the stock primary for .on-dark text', () => {
            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ primaryColor: '#121212', accentColor: '#c5a059' });
            const body = document.body.style;
            expect(body.getPropertyValue('--accent-on-dark-text')).toBe('#c5a059');
            expect(body.getPropertyValue('--accent-on-dark-hover-text'))
                .toBe(document.documentElement.style.getPropertyValue('--accent-gold-bright'));
        });

        it.each(['light', 'dark'] as const)('lifts a custom accent until it reads on a custom primary in %s theme', theme => {
            themeServiceStub.theme = () => theme;
            setConfigAndFlush({ primaryColor: '#1a237e', accentColor: '#2e7d32' });
            const body = document.body.style;
            const onDark = body.getPropertyValue('--accent-on-dark-text');
            expect(onDark).not.toBe('#2e7d32');
            expect(contrastRatio(onDark, body.getPropertyValue('--primary-color'))).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
        });

        it('leaves the .on-dark accent unset when branding has no primary', () => {
            setConfigAndFlush({ primaryColor: undefined, accentColor: '#2e7d32' });
            expect(document.body.style.getPropertyValue('--accent-on-dark-text')).toBe('');
        });

        it('clears tokens left by an earlier config once branding drops those colors', () => {
            setConfigAndFlush({ primaryColor: '#1a237e', accentColor: '#2e7d32' });
            expect(document.body.style.getPropertyValue('--accent-on-dark-text')).not.toBe('');

            setConfigAndFlush({ primaryColor: undefined, accentColor: '#c5a059' });
            const body = document.body.style;
            expect(body.getPropertyValue('--primary-color')).toBe('');
            expect(body.getPropertyValue('--text-on-primary')).toBe('');
            expect(body.getPropertyValue('--accent-on-dark-text')).toBe('');
            expect(body.getPropertyValue('--text-on-accent')).toBe('#171717');

            setConfigAndFlush({ primaryColor: undefined, accentColor: undefined });
            expect(body.getPropertyValue('--accent-text')).toBe('');
            expect(body.getPropertyValue('--focus-ring-color')).toBe('');
            expect(document.documentElement.style.getPropertyValue('--accent-color')).toBe('');
            expect(document.documentElement.style.getPropertyValue('--gold-gradient')).toBe('');
        });

        it('darkens the .on-dark accent on a light primary instead of pushing it to white', () => {
            themeServiceStub.theme = () => 'light';
            setConfigAndFlush({ primaryColor: '#f4d36f', accentColor: '#c5a059' });
            const body = document.body.style;
            const onDark = body.getPropertyValue('--accent-on-dark-text');
            expect(onDark).not.toBe('#ffffff');
            expect(contrastRatio(onDark, '#f4d36f')).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
        });

        it.each(['red', 'rgb(0, 0, 0)', '#12345', '#11223344'])('treats a non-hex primary %s as unset', value => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ primaryColor: value, accentColor: '#c5a059' });
            expect(document.body.style.getPropertyValue('--primary-color')).toBe('');
            expect(document.body.style.getPropertyValue('--accent-text')).toBe('#c5a059');
        });

        it('lifts a dark accent for --tier-founding in dark theme', () => {
            themeServiceStub.theme = () => 'dark';
            setConfigAndFlush({ accentColor: '#2f6f4f' });
            const tier = document.documentElement.style.getPropertyValue('--tier-founding');
            expect(contrastRatio(tier, DARK_TEXT_SURFACE)).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
        });

        it.each(['light', 'dark'] as const)('derives hover text, focus ring and hover shadow from the accent in %s theme', theme => {
            themeServiceStub.theme = () => theme;
            setConfigAndFlush({ accentColor: '#2f6f4f' });
            const body = document.body.style;
            const surface = theme === 'dark' ? DARK_TEXT_SURFACE : LIGHT_TEXT_SURFACE;
            const hover = body.getPropertyValue('--accent-hover-text');
            const focus = body.getPropertyValue('--focus-ring-color');
            expect(hover).toMatch(/^#[0-9a-f]{6}$/);
            expect(hover).not.toBe('#f4d36f');
            expect(contrastRatio(hover, surface)).toBeGreaterThanOrEqual(MIN_TEXT_CONTRAST);
            expect(focus).toMatch(/^#[0-9a-f]{6}$/);
            expect(body.getPropertyValue('--shadow-accent-hover')).toBe('0 8px 25px rgba(47, 111, 79, 0.4)');
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
