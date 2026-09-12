import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { describe, expect, it, beforeEach } from 'vitest';
import { AppDatePipe } from './app-date.pipe';
import { OrgConfigService } from '../services/org-config.service';

describe('AppDatePipe', () => {
    let pipe: AppDatePipe;
    let orgConfig: OrgConfigService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [OrgConfigService, provideHttpClient()]
        });
        pipe = TestBed.runInInjectionContext(() => new AppDatePipe());
        orgConfig = TestBed.inject(OrgConfigService);
    });

    it('uses the default organization format before configuration loads', () => {
        expect(pipe.transform('2025-01-02')).toBe('02-01-2025');
    });

    it('uses the persisted organization format when it changes', () => {
        orgConfig.config.set({
            localization: { dateFormat: 'MM/dd/yyyy', locales: {} }
        } as never);

        expect(pipe.transform('2025-01-02')).toBe('01/02/2025');
    });

    it('returns an empty string for an empty value', () => {
        expect(pipe.transform(null)).toBe('');
    });
});
