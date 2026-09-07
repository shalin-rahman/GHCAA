import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { AppCurrencyPipe } from './app-currency.pipe';
import { OrgConfigService } from '../services/org-config.service';
import { OrgConfig } from '../models/org-config.model';

describe('AppCurrencyPipe', () => {
  let pipe: AppCurrencyPipe;
  let orgConfig: OrgConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrgConfigService, provideHttpClient(), provideHttpClientTesting()]
    });
    orgConfig = TestBed.inject(OrgConfigService);
    pipe = TestBed.runInInjectionContext(() => new AppCurrencyPipe());
  });

  it('reads the symbol from the loaded OrgConfig rather than a hardcoded ৳', () => {
    orgConfig.config.set({ currency: { code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka' } } as OrgConfig);
    expect(pipe.transform(1234)).toBe('৳1,234');
  });

  it('renders a non-BDT profile\'s own symbol with no template change', () => {
    orgConfig.config.set({ currency: { code: 'USD', symbol: '$', name: 'US Dollar' } } as OrgConfig);
    expect(pipe.transform(1234)).toBe('$1,234');
  });

  it('supports the "code" display mode', () => {
    orgConfig.config.set({ currency: { code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka' } } as OrgConfig);
    expect(pipe.transform(1234, '1.0-0', 'code')).toBe('BDT 1,234');
  });

  it('degrades to a bare number before OrgConfig has loaded', () => {
    expect(orgConfig.config()).toBeNull();
    expect(pipe.transform(1234)).toBe('1,234');
  });
});
