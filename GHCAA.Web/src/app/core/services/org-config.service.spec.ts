import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { OrgConfigService } from './org-config.service';
import { API_ENDPOINTS } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';

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

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [OrgConfigService, provideHttpClient(), provideHttpClientTesting()]
        });
        service = TestBed.inject(OrgConfigService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    it('starts with no config until loadConfig resolves', () => {
        expect(service.config()).toBeNull();
    });

    it('loadConfig fetches from the config endpoint and stores the result', async () => {
        const promise = service.loadConfig();

        const req = httpMock.expectOne(API_ENDPOINTS.CONFIG);
        expect(req.request.method).toBe('GET');
        req.flush(minimalConfig);

        await promise;
        expect(service.config()).toEqual(minimalConfig);
    });

    it('loadConfig falls back to the built-in GHCAA defaults when the request fails', async () => {
        const promise = service.loadConfig();

        httpMock.expectOne(API_ENDPOINTS.CONFIG).error(new ProgressEvent('network error'));

        await promise;
        const cfg = service.config();
        expect(cfg).not.toBeNull();
        expect(cfg!.orgId).toBe('ghcaa');
        expect(cfg!.branding.shortName).toBe('GHCAA');
        expect(cfg!.features.enableEvents).toBe(true);
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
});
